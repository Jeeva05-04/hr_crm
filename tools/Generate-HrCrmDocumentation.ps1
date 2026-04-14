Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-ClassPropertyCatalog {
    param(
        [string[]]$Folders
    )

    $catalog = @{}

    foreach ($folder in $Folders) {
        if (-not (Test-Path $folder)) { continue }

        Get-ChildItem -Path $folder -Filter *.cs -File | ForEach-Object {
            $content = Get-Content $_.FullName -Raw

            $classMatches = [regex]::Matches(
                $content,
                '(?ms)public\s+(?:partial\s+)?class\s+(?<name>\w+)\s*.*?\{(?<body>.*?)^\}'
            )

            foreach ($classMatch in $classMatches) {
                $className = $classMatch.Groups['name'].Value
                $classBody = $classMatch.Groups['body'].Value

                $properties = New-Object System.Collections.Generic.List[object]
                $propertyMatches = [regex]::Matches(
                    $classBody,
                    '(?m)^\s*public\s+(?!class\b)(?!interface\b)(?!enum\b)(?<type>[\w<>\.\?\[\], ]+?)\s+(?<name>\w+)\s*\{\s*get;\s*set;\s*\}'
                )

                foreach ($propertyMatch in $propertyMatches) {
                    $properties.Add([pscustomobject]@{
                        Name = $propertyMatch.Groups['name'].Value
                        Type = ($propertyMatch.Groups['type'].Value -replace '\s+', ' ').Trim()
                    })
                }

                if ($properties.Count -gt 0) {
                    $catalog[$className] = [pscustomobject]@{
                        Name = $className
                        SourceFile = $_.Name
                        Properties = $properties
                    }
                }
            }
        }
    }

    return $catalog
}

function Get-ServiceMethodCatalog {
    param(
        [string]$Folder
    )

    $catalog = @{}

    Get-ChildItem -Path $Folder -Filter I*.cs -File | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $interfaceMatch = [regex]::Match($content, 'public\s+interface\s+(?<name>\w+)')
        if (-not $interfaceMatch.Success) { return }

        $interfaceName = $interfaceMatch.Groups['name'].Value
        $methods = @{}

        $methodMatches = [regex]::Matches(
            $content,
            '(?m)^\s*Task(?:<(?<rtype>[^>]+)>)?\s+(?<name>\w+)\s*\((?<params>.*?)\)\s*;'
        )

        foreach ($methodMatch in $methodMatches) {
            $methodName = $methodMatch.Groups['name'].Value
            $returnType = if ($methodMatch.Groups['rtype'].Success) {
                $methodMatch.Groups['rtype'].Value.Trim()
            }
            else {
                'void'
            }

            $methods[$methodName] = $returnType
        }

        $catalog[$interfaceName] = $methods
    }

    return $catalog
}

function Split-TopLevel {
    param(
        [string]$Text,
        [char]$Delimiter = ','
    )

    $items = New-Object System.Collections.Generic.List[string]
    $current = New-Object System.Text.StringBuilder
    $depthParen = 0
    $depthAngle = 0
    $depthBracket = 0

    foreach ($char in $Text.ToCharArray()) {
        switch ($char) {
            '(' { $depthParen++ }
            ')' { if ($depthParen -gt 0) { $depthParen-- } }
            '<' { $depthAngle++ }
            '>' { if ($depthAngle -gt 0) { $depthAngle-- } }
            '[' { $depthBracket++ }
            ']' { if ($depthBracket -gt 0) { $depthBracket-- } }
        }

        if ($char -eq $Delimiter -and $depthParen -eq 0 -and $depthAngle -eq 0 -and $depthBracket -eq 0) {
            $items.Add($current.ToString().Trim())
            $current.Clear() | Out-Null
            continue
        }

        [void]$current.Append($char)
    }

    if ($current.Length -gt 0) {
        $items.Add($current.ToString().Trim())
    }

    return $items | Where-Object { $_ }
}

function Get-MethodBlocks {
    param(
        [string]$Content
    )

    $pattern = '(?ms)(?<attrs>(?:\s*\[[^\]]+\]\s*)+)\s*public\s+async\s+Task<IActionResult>\s+(?<name>\w+)\s*\((?<params>.*?)\)\s*\{'
    $matches = [regex]::Matches($Content, $pattern)
    $blocks = @()

    foreach ($match in $matches) {
        $startBrace = $match.Index + $match.Length - 1
        $depth = 0
        $endIndex = $null

        for ($i = $startBrace; $i -lt $Content.Length; $i++) {
            $char = $Content[$i]
            if ($char -eq '{') {
                $depth++
            }
            elseif ($char -eq '}') {
                $depth--
                if ($depth -eq 0) {
                    $endIndex = $i
                    break
                }
            }
        }

        if ($null -eq $endIndex) { continue }

        $body = $Content.Substring($startBrace + 1, $endIndex - $startBrace - 1)
        $blocks += [pscustomobject]@{
            Name = $match.Groups['name'].Value
            Attributes = $match.Groups['attrs'].Value
            Parameters = $match.Groups['params'].Value
            Body = $body
        }
    }

    return $blocks
}

function Get-FieldTypeMap {
    param(
        [string]$Content
    )

    $map = @{}
    $fieldMatches = [regex]::Matches(
        $Content,
        '(?m)^\s*private\s+readonly\s+(?<type>[\w<>\.\?]+)\s+(?<name>_\w+)\s*;'
    )

    foreach ($fieldMatch in $fieldMatches) {
        $map[$fieldMatch.Groups['name'].Value] = $fieldMatch.Groups['type'].Value
    }

    return $map
}

function Get-ParamMetadata {
    param(
        [string]$ParameterText
    )

    $result = New-Object System.Collections.Generic.List[object]
    foreach ($part in (Split-TopLevel -Text $ParameterText)) {
        $clean = ($part -replace '\s*=\s*.+$', '').Trim()
        if (-not $clean) { continue }

        $source = 'route/query'
        if ($clean -match '\[FromBody\]') { $source = 'body' }
        elseif ($clean -match '\[FromForm\]') { $source = 'form-data' }
        elseif ($clean -match '\[FromQuery\]') { $source = 'query' }
        elseif ($clean -match '\[FromRoute\]') { $source = 'route' }

        $withoutAttributes = [regex]::Replace($clean, '\[[^\]]+\]\s*', '').Trim()
        $paramMatch = [regex]::Match($withoutAttributes, '^(?<type>[\w<>\.\?\[\], ]+?)\s+(?<name>\w+)$')
        if (-not $paramMatch.Success) { continue }

        $result.Add([pscustomobject]@{
            Name = $paramMatch.Groups['name'].Value
            Type = ($paramMatch.Groups['type'].Value -replace '\s+', ' ').Trim()
            Source = $source
        })
    }

    return $result
}

function Resolve-ControllerRoute {
    param(
        [string]$Content,
        [string]$ControllerClass
    )

    $routeMatch = [regex]::Match($Content, '\[Route\("(?<route>[^"]+)"\)\]')
    $route = if ($routeMatch.Success) { $routeMatch.Groups['route'].Value } else { 'api/[controller]' }

    $controllerToken = ($ControllerClass -replace '(?i)controller$', '')
    return $route -replace '\[controller\]', $controllerToken
}

function Merge-Route {
    param(
        [string]$BaseRoute,
        [string]$MethodRoute
    )

    if ([string]::IsNullOrWhiteSpace($MethodRoute)) {
        return $BaseRoute
    }

    if ($MethodRoute.StartsWith('/')) {
        return $MethodRoute.TrimEnd('/')
    }

    return ($BaseRoute.TrimEnd('/') + '/' + $MethodRoute.TrimStart('/')).TrimEnd('/')
}

function Get-BodyAssignments {
    param(
        [string]$Body
    )

    $map = @{}
    $matches = [regex]::Matches($Body, '(?m)^\s*var\s+(?<var>\w+)\s*=\s*await\s+(?<svc>_\w+)\.(?<method>\w+)\s*\(')
    foreach ($m in $matches) {
        $map[$m.Groups['var'].Value] = [pscustomobject]@{
            ServiceField = $m.Groups['svc'].Value
            MethodName = $m.Groups['method'].Value
        }
    }
    return $map
}

function Normalize-TypeName {
    param(
        [string]$TypeName
    )

    $value = ($TypeName -replace '^DTO\.', '' -replace '^Entities\.', '').Trim()
    return $value
}

function Describe-Type {
    param(
        [string]$TypeName,
        [hashtable]$ModelCatalog
    )

    $typeName = Normalize-TypeName $TypeName
    $summary = [pscustomobject]@{
        DisplayType = $typeName
        Fields = @()
    }

    if ($typeName -match '^List<(.+)>$') {
        $inner = $Matches[1].Trim()
        $summary.DisplayType = "Array of $inner"
        $innerNormalized = Normalize-TypeName $inner
        if ($ModelCatalog.ContainsKey($innerNormalized)) {
            $summary.Fields = $ModelCatalog[$innerNormalized].Properties
        }
        return $summary
    }

    if ($typeName -match '^IEnumerable<(.+)>$') {
        $inner = $Matches[1].Trim()
        $summary.DisplayType = "Array of $inner"
        $innerNormalized = Normalize-TypeName $inner
        if ($ModelCatalog.ContainsKey($innerNormalized)) {
            $summary.Fields = $ModelCatalog[$innerNormalized].Properties
        }
        return $summary
    }

    if ($typeName -match '^\((.+)\)$') {
        return $summary
    }

    if ($ModelCatalog.ContainsKey($typeName)) {
        $summary.Fields = $ModelCatalog[$typeName].Properties
    }

    return $summary
}

function Get-ResponseSummary {
    param(
        [string]$Body,
        [hashtable]$Assignments,
        [hashtable]$FieldTypes,
        [hashtable]$ServiceCatalog,
        [hashtable]$ModelCatalog
    )

    $responses = New-Object System.Collections.Generic.List[string]

    $fileMatch = [regex]::Match($Body, 'return\s+File\s*\(')
    if ($fileMatch.Success) {
        $responses.Add('Success response: file download/stream.')
    }

    $okAnonMatches = [regex]::Matches($Body, 'return\s+Ok\s*\(\s*new\s*\{(?<payload>.*?)\}\s*\)', 'Singleline')
    foreach ($okAnonMatch in $okAnonMatches) {
        $payload = $okAnonMatch.Groups['payload'].Value
        $fieldNames = New-Object System.Collections.Generic.List[string]
        foreach ($segment in (Split-TopLevel -Text $payload)) {
            $segmentTrim = $segment.Trim()
            if (-not $segmentTrim) { continue }

            if ($segmentTrim -match '^(?<name>\w+)\s*=') {
                $fieldNames.Add($Matches['name'])
            }
            elseif ($segmentTrim -match '^(?<name>\w+)$') {
                $fieldNames.Add($Matches['name'])
            }
            elseif ($segmentTrim -match '^(?<name>\w+)\s*,') {
                $fieldNames.Add($Matches['name'])
            }
        }

        if ($fieldNames.Count -gt 0) {
            $responses.Add("Success response: JSON object with fields: $($fieldNames -join ', ').")
        }
    }

    $okVarMatches = [regex]::Matches($Body, 'return\s+Ok\s*\(\s*(?<expr>[^;\r\n]+?)\s*\)\s*;')
    foreach ($okVarMatch in $okVarMatches) {
        $expr = $okVarMatch.Groups['expr'].Value.Trim()
        if ($expr -match '^new\s*\{') { continue }
        if ($expr -match '^await\s+(?<svc>_\w+)\.(?<method>\w+)\s*\(') {
            $svc = $Matches['svc']
            $method = $Matches['method']
            if ($FieldTypes.ContainsKey($svc)) {
                $svcType = $FieldTypes[$svc]
                if ($ServiceCatalog.ContainsKey($svcType) -and $ServiceCatalog[$svcType].ContainsKey($method)) {
                    $typeDescription = Describe-Type -TypeName $ServiceCatalog[$svcType][$method] -ModelCatalog $ModelCatalog
                    $text = "Success response: $($typeDescription.DisplayType)."
                    if ($typeDescription.Fields.Count -gt 0) {
                        $fieldText = ($typeDescription.Fields | ForEach-Object { "$($_.Name): $($_.Type)" }) -join '; '
                        $text += " Fields: $fieldText"
                    }
                    $responses.Add($text)
                }
            }
            continue
        }

        if ($Assignments.ContainsKey($expr)) {
            $assignment = $Assignments[$expr]
            if ($FieldTypes.ContainsKey($assignment.ServiceField)) {
                $svcType = $FieldTypes[$assignment.ServiceField]
                if ($ServiceCatalog.ContainsKey($svcType) -and $ServiceCatalog[$svcType].ContainsKey($assignment.MethodName)) {
                    $typeDescription = Describe-Type -TypeName $ServiceCatalog[$svcType][$assignment.MethodName] -ModelCatalog $ModelCatalog
                    $text = "Success response: $($typeDescription.DisplayType)."
                    if ($typeDescription.Fields.Count -gt 0) {
                        $fieldText = ($typeDescription.Fields | ForEach-Object { "$($_.Name): $($_.Type)" }) -join '; '
                        $text += " Fields: $fieldText"
                    }
                    $responses.Add($text)
                }
            }
        }
        elseif ($expr -match '^"') {
            $responses.Add("Success response: plain string: $expr")
        }
    }

    $badRequestMatches = [regex]::Matches($Body, 'return\s+BadRequest\s*\(\s*(?<expr>[^;]+?)\s*\)\s*;')
    foreach ($m in $badRequestMatches) {
        $responses.Add("Possible error response: 400 Bad Request with payload/expression [$($m.Groups['expr'].Value.Trim())].")
    }

    $notFoundMatches = [regex]::Matches($Body, 'return\s+NotFound\s*\(\s*(?<expr>[^;]+?)\s*\)\s*;')
    foreach ($m in $notFoundMatches) {
        $responses.Add("Possible error response: 404 Not Found with payload/expression [$($m.Groups['expr'].Value.Trim())].")
    }

    if ($Body -match 'return\s+Unauthorized\s*\(' -or $Body -match 'return\s+Unauthorized\s*;') {
        $responses.Add('Possible error response: 401 Unauthorized.')
    }

    if ($Body -match 'return\s+Forbid\s*\(' -or $Body -match 'return\s+Forbid\s*\(\s*\)\s*;') {
        $responses.Add('Possible error response: 403 Forbidden.')
    }

    if ($Body -match 'return\s+Conflict\s*\(') {
        $responses.Add('Possible error response: 409 Conflict.')
    }

    if ($responses.Count -eq 0) {
        $responses.Add('Response shape requires manual review; controller uses indirect logic not fully inferable by script.')
    }

    return $responses
}

function Escape-Xml {
    param([string]$Text)

    if ($null -eq $Text) { return '' }
    return [System.Security.SecurityElement]::Escape($Text)
}

function Add-DocParagraph {
    param(
        [System.Collections.Generic.List[string]]$XmlLines,
        [string]$Text,
        [string]$Style = 'Normal'
    )

    $safe = Escape-Xml $Text
    $XmlLines.Add("<w:p><w:pPr><w:pStyle w:val=""$Style""/></w:pPr><w:r><w:t xml:space=""preserve"">$safe</w:t></w:r></w:p>")
}

function New-DocxPackage {
    param(
        [string]$OutputPath,
        [string[]]$ParagraphXml
    )

    if (Test-Path $OutputPath) {
        Remove-Item -LiteralPath $OutputPath -Force
    }

    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("hrcrm-doc-" + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $tempDir | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $tempDir '_rels') | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $tempDir 'word') | Out-Null

    $contentTypes = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
  <Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
</Types>
'@

    $rels = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
</Relationships>
'@

    $styles = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:style w:type="paragraph" w:default="1" w:styleId="Normal">
    <w:name w:val="Normal"/>
    <w:qFormat/>
    <w:rPr>
      <w:sz w:val="22"/>
    </w:rPr>
  </w:style>
  <w:style w:type="paragraph" w:styleId="Title">
    <w:name w:val="Title"/>
    <w:qFormat/>
    <w:rPr>
      <w:b/>
      <w:sz w:val="32"/>
    </w:rPr>
  </w:style>
  <w:style w:type="paragraph" w:styleId="Heading1">
    <w:name w:val="heading 1"/>
    <w:qFormat/>
    <w:rPr>
      <w:b/>
      <w:sz w:val="28"/>
    </w:rPr>
  </w:style>
  <w:style w:type="paragraph" w:styleId="Heading2">
    <w:name w:val="heading 2"/>
    <w:qFormat/>
    <w:rPr>
      <w:b/>
      <w:sz w:val="24"/>
    </w:rPr>
  </w:style>
</w:styles>
'@

    $documentBody = @()
    $documentBody += '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
    $documentBody += '<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">'
    $documentBody += '<w:body>'
    $documentBody += $ParagraphXml
    $documentBody += '<w:sectPr/>'
    $documentBody += '</w:body>'
    $documentBody += '</w:document>'

    [System.IO.File]::WriteAllText((Join-Path $tempDir '[Content_Types].xml'), $contentTypes, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText((Join-Path $tempDir '_rels\.rels'), $rels, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText((Join-Path $tempDir 'word\styles.xml'), $styles, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText((Join-Path $tempDir 'word\document.xml'), ($documentBody -join [Environment]::NewLine), [System.Text.Encoding]::UTF8)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $OutputPath)
    Remove-Item -LiteralPath $tempDir -Recurse -Force
}

$root = Split-Path -Parent $PSScriptRoot
$controllersPath = Join-Path $root 'Controllers'
$dtoPath = Join-Path $root 'DTO'
$entitiesPath = Join-Path $root 'Entities'
$serviceInterfacePath = Join-Path $root 'Service\Interface'
$docsPath = Join-Path $root 'docs'

if (-not (Test-Path $docsPath)) {
    New-Item -ItemType Directory -Path $docsPath | Out-Null
}

$modelCatalog = Get-ClassPropertyCatalog -Folders @($dtoPath, $entitiesPath)
$serviceCatalog = Get-ServiceMethodCatalog -Folder $serviceInterfacePath

$paragraphs = New-Object System.Collections.Generic.List[string]

Add-DocParagraph -XmlLines $paragraphs -Text 'HR CRM API Documentation' -Style 'Title'
Add-DocParagraph -XmlLines $paragraphs -Text ("Generated from source code on " + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
Add-DocParagraph -XmlLines $paragraphs -Text 'Scope: controllers, DTOs, entities, authentication setup, permissions, SignalR hubs, and observed controller response shapes.'

$programContent = Get-Content (Join-Path $root 'Program.cs') -Raw
Add-DocParagraph -XmlLines $paragraphs -Text 'Platform Overview' -Style 'Heading1'
Add-DocParagraph -XmlLines $paragraphs -Text 'Framework: ASP.NET Core Web API on .NET 8 with Entity Framework Core and PostgreSQL.'
Add-DocParagraph -XmlLines $paragraphs -Text 'Authentication: JWT Bearer authentication is enabled globally for protected endpoints.'
Add-DocParagraph -XmlLines $paragraphs -Text 'Authorization: role-based policies plus per-endpoint custom permission checks via HasPermission attributes.'
Add-DocParagraph -XmlLines $paragraphs -Text 'Swagger: enabled in the application startup.'
Add-DocParagraph -XmlLines $paragraphs -Text 'SignalR hubs: /hubs/location and /hubs/notifications.'
Add-DocParagraph -XmlLines $paragraphs -Text 'Static files: enabled, with onboarding and signature files served from web root when available.'

$controllerFiles = Get-ChildItem -Path $controllersPath -Filter *.cs -File | Sort-Object Name
$endpointCount = 0
$controllersProcessed = @()

foreach ($controllerFile in $controllerFiles) {
    $content = Get-Content $controllerFile.FullName -Raw
    $classMatch = [regex]::Match($content, 'class\s+(?<name>\w+Controller)\b')
    if (-not $classMatch.Success) { continue }

    $controllerClass = $classMatch.Groups['name'].Value
    $baseRoute = Resolve-ControllerRoute -Content $content -ControllerClass $controllerClass
    $fieldTypes = Get-FieldTypeMap -Content $content
    $methods = @(Get-MethodBlocks -Content $content)
    if ($methods.Count -eq 0) { continue }

    $controllersProcessed += $controllerClass
    Add-DocParagraph -XmlLines $paragraphs -Text ($controllerClass -replace 'Controller$', '') -Style 'Heading1'
    Add-DocParagraph -XmlLines $paragraphs -Text ("Base route: " + $baseRoute)

    $controllerAuth = if ($content -match '\[Authorize(?:\((?<args>[^\)]*)\))?\]') {
        if ($Matches['args']) { "Protected. $($Matches['args'])" } else { 'Protected.' }
    }
    else {
        'No controller-level authorize attribute found.'
    }
    Add-DocParagraph -XmlLines $paragraphs -Text ("Authorization: " + $controllerAuth)

    foreach ($method in $methods) {
        $httpMatch = [regex]::Match($method.Attributes, '\[(?<verb>HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\("(?<route>[^"]*)"\))?\]')
        if (-not $httpMatch.Success) { continue }

        $verb = ($httpMatch.Groups['verb'].Value -replace '^Http', '').ToUpperInvariant()
        $subRoute = $httpMatch.Groups['route'].Value
        $fullRoute = Merge-Route -BaseRoute $baseRoute -MethodRoute $subRoute
        $endpointCount++

        Add-DocParagraph -XmlLines $paragraphs -Text ("$verb $fullRoute") -Style 'Heading2'
        Add-DocParagraph -XmlLines $paragraphs -Text ("Action: " + $method.Name)

        $allowAnonymous = $method.Attributes -match '\[AllowAnonymous\]'
        $permissionMatch = [regex]::Match($method.Attributes, '\[HasPermission\("(?<perm>[^"]+)"\)\]')
        $roleAuthorizeMatch = [regex]::Match($method.Attributes, '\[Authorize\((?<args>[^\)]*)\)\]')

        if ($allowAnonymous) {
            Add-DocParagraph -XmlLines $paragraphs -Text 'Security: anonymous access allowed for this endpoint.'
        }
        else {
            $securityParts = New-Object System.Collections.Generic.List[string]
            $securityParts.Add('authentication required')
            if ($permissionMatch.Success) {
                $securityParts.Add("permission: $($permissionMatch.Groups['perm'].Value)")
            }
            if ($roleAuthorizeMatch.Success) {
                $securityParts.Add("authorize args: $($roleAuthorizeMatch.Groups['args'].Value)")
            }
            Add-DocParagraph -XmlLines $paragraphs -Text ("Security: " + ($securityParts -join '; '))
        }

        $parameters = @(Get-ParamMetadata -ParameterText $method.Parameters)
        if ($parameters.Count -gt 0) {
            Add-DocParagraph -XmlLines $paragraphs -Text ('Parameters: ' + (($parameters | ForEach-Object { "$($_.Name) [$($_.Type)] from $($_.Source)" }) -join '; '))
        }
        else {
            Add-DocParagraph -XmlLines $paragraphs -Text 'Parameters: none.'
        }

        $bodyPayload = @($parameters | Where-Object { $_.Source -in @('body', 'form-data') })
        if ($bodyPayload.Count -gt 0) {
            foreach ($payload in $bodyPayload) {
                $payloadType = Normalize-TypeName $payload.Type
                Add-DocParagraph -XmlLines $paragraphs -Text ("Request payload type: $payloadType")
                if ($modelCatalog.ContainsKey($payloadType)) {
                    $fieldText = ($modelCatalog[$payloadType].Properties | ForEach-Object { "$($_.Name): $($_.Type)" }) -join '; '
                    Add-DocParagraph -XmlLines $paragraphs -Text ("Request payload fields: $fieldText")
                }
            }
        }

        $assignments = Get-BodyAssignments -Body $method.Body
        $responses = Get-ResponseSummary -Body $method.Body -Assignments $assignments -FieldTypes $fieldTypes -ServiceCatalog $serviceCatalog -ModelCatalog $modelCatalog
        foreach ($response in $responses | Select-Object -Unique) {
            Add-DocParagraph -XmlLines $paragraphs -Text $response
        }
    }
}

Add-DocParagraph -XmlLines $paragraphs -Text 'DTO and Entity Reference' -Style 'Heading1'
foreach ($modelName in ($modelCatalog.Keys | Sort-Object)) {
    $model = $modelCatalog[$modelName]
    Add-DocParagraph -XmlLines $paragraphs -Text $model.Name -Style 'Heading2'
    Add-DocParagraph -XmlLines $paragraphs -Text ("Source file: " + $model.SourceFile)
    Add-DocParagraph -XmlLines $paragraphs -Text ('Fields: ' + (($model.Properties | ForEach-Object { "$($_.Name): $($_.Type)" }) -join '; '))
}

Add-DocParagraph -XmlLines $paragraphs -Text 'Summary' -Style 'Heading1'
Add-DocParagraph -XmlLines $paragraphs -Text ("Controllers documented: " + $controllersProcessed.Count)
Add-DocParagraph -XmlLines $paragraphs -Text ("Endpoints documented: " + $endpointCount)
Add-DocParagraph -XmlLines $paragraphs -Text 'Note: response contracts were inferred from controller code and service interface signatures. Endpoints returning anonymous objects, strings, or file downloads are described from observed controller logic.'

$docxPath = Join-Path $docsPath 'HR_CRM_API_Documentation.docx'
New-DocxPackage -OutputPath $docxPath -ParagraphXml $paragraphs

$markdownPath = Join-Path $docsPath 'HR_CRM_API_Documentation_Generated.txt'
$plainText = ($paragraphs | ForEach-Object { ($_ -replace '<[^>]+>', '') }) -join [Environment]::NewLine
[System.IO.File]::WriteAllText($markdownPath, $plainText, [System.Text.Encoding]::UTF8)

Write-Output "Generated: $docxPath"
Write-Output "Generated: $markdownPath"
