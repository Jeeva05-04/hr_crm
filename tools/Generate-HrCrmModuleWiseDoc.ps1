Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ClassPropertyCatalog {
    param([string[]]$Folders)
    $catalog = @{}
    foreach ($folder in $Folders) {
        if (-not (Test-Path $folder)) { continue }
        Get-ChildItem -Path $folder -Filter *.cs -File | ForEach-Object {
            $content = Get-Content $_.FullName -Raw
            $matches = [regex]::Matches($content, "(?ms)public\s+(?:partial\s+)?class\s+(?<name>\w+)\s*.*?\{(?<body>.*?)^\}")
            foreach ($m in $matches) {
                $props = [regex]::Matches($m.Groups["body"].Value, "(?m)^\s*public\s+(?!class\b)(?!interface\b)(?!enum\b)(?<type>[\w<>\.\?\[\], ]+?)\s+(?<name>\w+)\s*\{\s*get;\s*set;\s*\}") |
                    ForEach-Object { [pscustomobject]@{ Name = $_.Groups["name"].Value; Type = (($_.Groups["type"].Value -replace "\s+", " ").Trim()) } }
                if (@($props).Count -gt 0) { $catalog[$m.Groups["name"].Value] = [pscustomobject]@{ SourceFile = $_.Name; Properties = @($props) } }
            }
        }
    }
    return $catalog
}

function Get-ServiceMethodCatalog {
    param([string]$Folder)
    $catalog = @{}
    Get-ChildItem -Path $Folder -Filter I*.cs -File | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $intf = [regex]::Match($content, "public\s+interface\s+(?<name>\w+)")
        if (-not $intf.Success) { return }
        $methods = @{}
        [regex]::Matches($content, "(?m)^\s*Task(?:<(?<rtype>[^>]+)>)?\s+(?<name>\w+)\s*\(") | ForEach-Object {
            if ($_.Groups["rtype"].Success) { $methods[$_.Groups["name"].Value] = $_.Groups["rtype"].Value.Trim() }
            else { $methods[$_.Groups["name"].Value] = "void" }
        }
        $catalog[$intf.Groups["name"].Value] = $methods
    }
    return $catalog
}

function Split-TopLevel {
    param([string]$Text, [char]$Delimiter = ',')
    $items = New-Object System.Collections.Generic.List[string]
    $sb = New-Object System.Text.StringBuilder
    $paren = 0; $angle = 0; $bracket = 0
    foreach ($ch in $Text.ToCharArray()) {
        switch ($ch) {
            '(' { $paren++ }
            ')' { if ($paren -gt 0) { $paren-- } }
            '<' { $angle++ }
            '>' { if ($angle -gt 0) { $angle-- } }
            '[' { $bracket++ }
            ']' { if ($bracket -gt 0) { $bracket-- } }
        }
        if ($ch -eq $Delimiter -and $paren -eq 0 -and $angle -eq 0 -and $bracket -eq 0) {
            $items.Add($sb.ToString().Trim())
            $sb.Clear() | Out-Null
            continue
        }
        [void]$sb.Append($ch)
    }
    if ($sb.Length -gt 0) { $items.Add($sb.ToString().Trim()) }
    return @($items | Where-Object { $_ })
}

function Get-MethodBlocks {
    param([string]$Content)
    $pattern = "(?ms)(?<attrs>(?:\s*\[[^\]]+\]\s*)+)\s*public\s+async\s+Task<IActionResult>\s+(?<name>\w+)\s*\((?<params>.*?)\)\s*\{"
    $matches = [regex]::Matches($Content, $pattern)
    $blocks = @()
    foreach ($match in $matches) {
        $startBrace = $match.Index + $match.Length - 1
        $depth = 0
        $endIndex = $null
        for ($i = $startBrace; $i -lt $Content.Length; $i++) {
            if ($Content[$i] -eq '{') { $depth++ }
            elseif ($Content[$i] -eq '}') { $depth--; if ($depth -eq 0) { $endIndex = $i; break } }
        }
        if ($null -eq $endIndex) { continue }
        $blocks += [pscustomobject]@{ Name = $match.Groups["name"].Value; Attributes = $match.Groups["attrs"].Value; Parameters = $match.Groups["params"].Value; Body = $Content.Substring($startBrace + 1, $endIndex - $startBrace - 1) }
    }
    return @($blocks)
}

function Get-FieldTypeMap {
    param([string]$Content)
    $map = @{}
    [regex]::Matches($Content, "(?m)^\s*private\s+readonly\s+(?<type>[\w<>\.\?]+)\s+(?<name>_\w+)\s*;") | ForEach-Object { $map[$_.Groups["name"].Value] = $_.Groups["type"].Value }
    return $map
}

function Get-ParamMetadata {
    param([string]$ParameterText)
    $result = @()
    foreach ($part in (Split-TopLevel -Text $ParameterText)) {
        $clean = ($part -replace "\s*=\s*.+$", "").Trim()
        if (-not $clean) { continue }
        $source = "route/query"
        if ($clean -match "\[FromBody\]") { $source = "body" }
        elseif ($clean -match "\[FromForm\]") { $source = "form-data" }
        elseif ($clean -match "\[FromQuery\]") { $source = "query" }
        elseif ($clean -match "\[FromRoute\]") { $source = "route" }
        $clean = [regex]::Replace($clean, "\[[^\]]+\]\s*", "").Trim()
        $m = [regex]::Match($clean, "^(?<type>[\w<>\.\?\[\], ]+?)\s+(?<name>\w+)$")
        if ($m.Success) { $result += [pscustomobject]@{ Name = $m.Groups["name"].Value; Type = (($m.Groups["type"].Value -replace "\s+", " ").Trim()); Source = $source } }
    }
    return @($result)
}

function Resolve-ControllerRoute {
    param([string]$Content, [string]$ControllerClass)
    $route = "api/[controller]"
    $m = [regex]::Match($Content, '\[Route\("(?<route>[^"]+)"\)\]')
    if ($m.Success) { $route = $m.Groups["route"].Value }
    return ($route -replace "\[controller\]", ($ControllerClass -replace "(?i)controller$", ""))
}

function Merge-Route {
    param([string]$BaseRoute, [string]$MethodRoute)
    if ([string]::IsNullOrWhiteSpace($MethodRoute)) { return $BaseRoute }
    if ($MethodRoute.StartsWith("/")) { return $MethodRoute.TrimEnd("/") }
    return ($BaseRoute.TrimEnd("/") + "/" + $MethodRoute.TrimStart("/")).TrimEnd("/")
}

function Normalize-TypeName { param([string]$TypeName) return (($TypeName -replace "^DTO\.", "" -replace "^Entities\.", "").Trim()) }

function Describe-Type {
    param([string]$TypeName, [hashtable]$ModelCatalog)
    $typeName = Normalize-TypeName $TypeName
    $display = $typeName
    $fields = @()
    if ($typeName -match "^List<(.+)>$" -or $typeName -match "^IEnumerable<(.+)>$") {
        $inner = Normalize-TypeName $Matches[1].Trim()
        $display = "Array of $inner"
        if ($ModelCatalog.ContainsKey($inner)) { $fields = $ModelCatalog[$inner].Properties }
        return [pscustomobject]@{ Display = $display; Fields = @($fields) }
    }
    if ($ModelCatalog.ContainsKey($typeName)) { $fields = $ModelCatalog[$typeName].Properties }
    return [pscustomobject]@{ Display = $display; Fields = @($fields) }
}

function Get-BodyAssignments {
    param([string]$Body)
    $map = @{}
    [regex]::Matches($Body, "(?m)^\s*var\s+(?<var>\w+)\s*=\s*await\s+(?<svc>_\w+)\.(?<method>\w+)\s*\(") | ForEach-Object { $map[$_.Groups["var"].Value] = [pscustomobject]@{ ServiceField = $_.Groups["svc"].Value; MethodName = $_.Groups["method"].Value } }
    return $map
}

function Get-ModuleLabel {
    param([string]$ControllerClass)
    $map = @{ AttendenceController="Attendance Module"; BranchController="Branch Management Module"; BudgetChangeController="Budget Change Request Module"; ChatController="Chat Module"; DebugController="Debug Module"; DepartmentBudgetController="Department Budget Module"; DepartmentController="Department Module"; DepartmentRoleController="Department Role Module"; DigitalSignatureController="Digital Signature Module"; EmployeeOnboardingController="Employee Onboarding Module"; EmployeeTrainingController="Employee Training Module"; ExitInterviewController="Exit Interview Module"; JobOpeningController="Job Opening Module"; KnowledgeController="Knowledge Module"; LeadController="Lead Module"; LearningController="Learning Module"; LeaveController="Leave Module"; LogsController="Logs Module"; NotificationController="Notification Module"; OffBoardingController="Offboarding Module"; OnboardingInviteController="Onboarding Invite Module"; OvertimeApprovalController="Overtime Approval Module"; OvertimePolicyController="Overtime Policy Module"; OvertimeRecordController="Overtime Record Module"; PayrollController="Payroll Module"; ProjectController="Project Module"; RecruitmentController="Recruitment Module"; ShiftController="Shift Module"; TodoController="To-Do Module" }
    if ($map.ContainsKey($ControllerClass)) { return $map[$ControllerClass] }
    return (($ControllerClass -replace "Controller$", "") + " Module")
}

function Get-ModuleDescription {
    param([string]$ControllerClass)
    $key = ($ControllerClass -replace "Controller$", "")
    $map = @{ Attendence="Handles employee check-in, check-out, attendance history, total hours, and live location tracking."; Branch="Maintains branch master data."; BudgetChange="Manages budget change requests and approval actions."; Chat="Supports internal messaging and chat presence."; Debug="Provides technical endpoints for token and claim troubleshooting."; DepartmentBudget="Stores and exposes department budget records."; Department="Maintains department master records and department user lookup."; DepartmentRole="Manages department roles and user role assignment."; DigitalSignature="Handles signature requests, signing flow, and document retrieval."; EmployeeOnboarding="Captures onboarding forms and onboarding documents."; EmployeeTraining="Assigns training and tracks employee training records."; ExitInterview="Schedules and tracks exit interviews and feedback."; JobOpening="Maintains job openings used by recruitment."; Knowledge="Stores organization knowledge-base records."; Lead="Maintains lead assignment and lead status workflows."; Learning="Assigns courses and updates learning progress."; Leave="Covers leave types, leave applications, approval flow, holidays, balances, and encashment."; Logs="Exposes audit and log data stored by the application."; Notification="Returns user notifications and supports read/delete actions."; OffBoarding="Handles offboarding records and their status updates."; OnboardingInvite="Creates and validates onboarding invite tokens."; OvertimeApproval="Handles overtime approval requests."; OvertimePolicy="Maintains overtime policies."; OvertimeRecord="Returns overtime records and summaries."; Payroll="Handles payroll, payslips, salary configuration, bonuses, allowances, and deductions."; Project="Maintains project records and manager notifications."; Recruitment="Handles candidate recruitment, dashboards, resumes, interviews, and onboarding conversion."; Shift="Maintains shifts and user shift assignments."; Todo="Manages task assignments and task status updates." }
    if ($map.ContainsKey($key)) { return $map[$key] }
    return "Provides API operations for this business area."
}

function Get-EndpointPurpose {
    param([string]$MethodName, [string]$Verb, [string]$Route)
    switch -Regex ($MethodName) {
        "^(Get|GetAll|GetBy|History|Status|GetCurrent)" { return "Fetches data for $Route." }
        "^(Create|Add|Generate|Apply|Assign|Request|Send|Schedule|Process|PublicSubmit|Submit|Trigger)" { return "Creates or submits data for $Route." }
        "^(Update|Mark|Approve|Reject|Set)" { return "Updates existing data or changes state for $Route." }
        "^(Delete|Clear)" { return "Deletes or clears data for $Route." }
        "^(Download|View)" { return "Returns a downloadable or viewable file for $Route." }
        "^(Convert)" { return "Converts one business record into another workflow state for $Route." }
        default { return ("Handles the endpoint route " + $Route + ".") }
    }
}

function Get-AccessSummary {
    param([string]$ControllerContent, [pscustomobject]$Method)
    if ($Method.Attributes -match "\[AllowAnonymous\]") { return "No authentication required." }
    $parts = New-Object System.Collections.Generic.List[string]
    if ($ControllerContent -match "\[Authorize" -or $Method.Attributes -match "\[Authorize") { $parts.Add("JWT authentication required") }
    $perm = [regex]::Match($Method.Attributes, '\[HasPermission\("(?<p>[^"]+)"\)\]')
    if ($perm.Success) { $parts.Add(("Permission: " + $perm.Groups["p"].Value)) }
    $roles = [regex]::Match($Method.Attributes, 'Roles\s*=\s*"(?<r>[^"]+)"')
    if ($roles.Success) { $parts.Add(("Roles: " + $roles.Groups["r"].Value)) }
    if ($Method.Body -match "isHR" -or $Method.Body -match "HR_MANAGER" -or $Method.Body -match "SUPERADMIN") { $parts.Add("Additional role-based checks are applied in controller logic") }
    if ($Method.Body -match "dto\.UserId != tokenUserId" -or $Method.Body -match "userId != tokenUserId") { $parts.Add("Self-user validation is enforced for user-specific data") }
    if ($parts.Count -eq 0) { return "Review application auth filters for access control." }
    return ($parts -join "; ")
}

function Get-ResponseSections {
    param([string]$Body, [hashtable]$Assignments, [hashtable]$FieldTypes, [hashtable]$ServiceCatalog, [hashtable]$ModelCatalog)
    $success = New-Object System.Collections.Generic.List[string]
    $errors = New-Object System.Collections.Generic.List[string]
    if ($Body -match "return\s+File\s*\(") { $success.Add("Returns a file stream or downloadable file.") }
    [regex]::Matches($Body, "return\s+Ok\s*\(\s*new\s*\{(?<payload>.*?)\}\s*\)", "Singleline") | ForEach-Object {
        $fields = New-Object System.Collections.Generic.List[string]
        foreach ($segment in (Split-TopLevel -Text $_.Groups["payload"].Value)) {
            $s = $segment.Trim()
            if ($s -match "^(?<n>\w+)\s*=") { $fields.Add($Matches["n"]) }
            elseif ($s -match "^(?<n>\w+)$") { $fields.Add($Matches["n"]) }
        }
        if ($fields.Count -gt 0) { $success.Add("Returns JSON fields: " + ($fields -join ", ")) }
    }
    [regex]::Matches($Body, "return\s+Ok\s*\(\s*(?<expr>[^;\r\n]+?)\s*\)\s*;") | ForEach-Object {
        $expr = $_.Groups["expr"].Value.Trim()
        if ($expr -match "^new\s*\{") { return }
        if ($Assignments.ContainsKey($expr)) {
            $a = $Assignments[$expr]
            if ($FieldTypes.ContainsKey($a.ServiceField)) {
                $svcType = $FieldTypes[$a.ServiceField]
                if ($ServiceCatalog.ContainsKey($svcType) -and $ServiceCatalog[$svcType].ContainsKey($a.MethodName)) {
                    $desc = Describe-Type -TypeName $ServiceCatalog[$svcType][$a.MethodName] -ModelCatalog $ModelCatalog
                    $line = "Returns $($desc.Display)."
                    if ($desc.Fields.Count -gt 0) { $line += " Fields: " + (($desc.Fields | ForEach-Object { "$($_.Name) ($($_.Type))" }) -join ", ") }
                    $success.Add($line)
                }
            }
        } elseif ($expr -match '^"') { $success.Add("Returns success message text.") }
    }
    [regex]::Matches($Body, "return\s+BadRequest\s*\(\s*(?<expr>[^;]+?)\s*\)\s*;") | ForEach-Object { $errors.Add("400 Bad Request: " + $_.Groups["expr"].Value.Trim()) }
    [regex]::Matches($Body, "return\s+NotFound\s*\(\s*(?<expr>[^;]+?)\s*\)\s*;") | ForEach-Object { $errors.Add("404 Not Found: " + $_.Groups["expr"].Value.Trim()) }
    if ($Body -match "return\s+Unauthorized\s*\(" -or $Body -match "return\s+Unauthorized\s*;") { $errors.Add("401 Unauthorized") }
    if ($Body -match "return\s+Forbid\s*\(" -or $Body -match "return\s+Forbid\s*\(\s*\)\s*;") { $errors.Add("403 Forbidden") }
    if ($Body -match "return\s+Conflict\s*\(") { $errors.Add("409 Conflict") }
    [regex]::Matches($Body, "return\s+StatusCode\((?<code>\d+)") | ForEach-Object { $errors.Add(($_.Groups["code"].Value + " Server/Application Error")) }
    if ($success.Count -eq 0) { $success.Add("Returns a success result based on the controller logic.") }
    return [pscustomobject]@{ Success = @($success | Select-Object -Unique); Errors = @($errors | Select-Object -Unique) }
}

function Get-SampleValue {
    param([string]$TypeName, [string]$FieldName)

    $type = (Normalize-TypeName $TypeName)
    $name = $FieldName.ToLowerInvariant()

    if ($type -match '^List<(.+)>$' -or $type -match '^IEnumerable<(.+)>$') {
        return @((Get-SampleValue -TypeName $Matches[1] -FieldName $FieldName))
    }

    if ($type -match '\[\]$') {
        return @("string")
    }

    if ($type -in @('int', 'int?', 'long', 'long?', 'short', 'short?')) { return 1 }
    if ($type -in @('decimal', 'decimal?', 'double', 'double?', 'float', 'float?')) { return 1000.50 }
    if ($type -in @('bool', 'bool?')) { return $true }
    if ($type -eq 'DateOnly' -or $name -like '*date') { return '2026-04-02' }
    if ($type -eq 'TimeSpan' -or $name -like '*time' -or $name -like 'starttime' -or $name -like 'endtime') { return '09:00:00' }
    if ($type -eq 'DateTime' -or $type -eq 'DateTime?' -or $name -like '*datetime' -or $name -like '*at') { return '2026-04-02T10:00:00Z' }
    if ($type -eq 'IFormFile') { return 'file' }

    if ($name -like '*id') { return 1 }
    if ($name -like '*count') { return 1 }
    if ($name -like '*amount' -or $name -like '*salary' -or $name -like '*budget' -or $name -like '*rate' -or $name -like '*price') { return 1000.50 }
    if ($name -like '*percentage') { return 10 }
    if ($name -like '*email') { return 'user@example.com' }
    if ($name -like '*phone') { return '9876543210' }
    if ($name -like '*url' -or $name -like '*path') { return 'string' }
    if ($name -like '*status') { return 'string' }
    if ($name -like '*name' -or $name -like '*title' -or $name -like '*type' -or $name -like '*role' -or $name -like '*code' -or $name -like '*category') { return 'string' }

    return 'string'
}

function New-SampleObjectFromFields {
    param([object[]]$Fields)

    $obj = [ordered]@{}
    foreach ($field in $Fields) {
        $obj[$field.Name] = Get-SampleValue -TypeName $field.Type -FieldName $field.Name
    }
    return $obj
}

function Convert-SampleToJsonLines {
    param($Value)

    if ($null -eq $Value) {
        return @('{', '  "message": "Success"', '}')
    }

    $json = $Value | ConvertTo-Json -Depth 10
    return @($json -split "`r?`n")
}

function Get-SampleRequestJsonLines {
    param(
        [object[]]$Parameters,
        [hashtable]$ModelCatalog
    )

    $bodyParam = $Parameters | Where-Object { $_.Source -in @('body', 'form-data') } | Select-Object -First 1
    if ($null -ne $bodyParam) {
        $typeName = Normalize-TypeName $bodyParam.Type
        if ($ModelCatalog.ContainsKey($typeName)) {
            return Convert-SampleToJsonLines (New-SampleObjectFromFields -Fields $ModelCatalog[$typeName].Properties)
        }

        return Convert-SampleToJsonLines ([ordered]@{ value = Get-SampleValue -TypeName $typeName -FieldName $bodyParam.Name })
    }

    if ($Parameters.Count -gt 0) {
        $obj = [ordered]@{}
        foreach ($parameter in $Parameters) {
            $obj[$parameter.Name] = Get-SampleValue -TypeName $parameter.Type -FieldName $parameter.Name
        }
        return Convert-SampleToJsonLines $obj
    }

    return @('{', '  "message": "No request body"', '}')
}

function Get-SampleSuccessJsonLines {
    param(
        [string]$Body,
        [hashtable]$Assignments,
        [hashtable]$FieldTypes,
        [hashtable]$ServiceCatalog,
        [hashtable]$ModelCatalog
    )

    if ($Body -match 'return\s+File\s*\(') {
        return Convert-SampleToJsonLines ([ordered]@{
            fileName = 'string'
            contentType = 'application/octet-stream'
            description = 'File download response'
        })
    }

    $bestFields = @()
    foreach ($_match in [regex]::Matches($Body, 'return\s+Ok\s*\(\s*new\s*\{(?<payload>.*?)\}\s*\)', [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $fieldList = New-Object System.Collections.Generic.List[object]
        foreach ($segment in (Split-TopLevel -Text $_match.Groups['payload'].Value)) {
            $s = $segment.Trim()
            if ($s -match '^(?<name>\w+)\s*=') {
                $fieldList.Add([pscustomobject]@{ Name = $Matches['name']; Type = '' })
            }
            elseif ($s -match '^(?<name>\w+)$') {
                $fieldList.Add([pscustomobject]@{ Name = $Matches['name']; Type = '' })
            }
        }

        if ($fieldList.Count -gt $bestFields.Count) {
            $bestFields = @($fieldList.ToArray())
        }
    }

    if ($bestFields.Count -gt 0) {
        return Convert-SampleToJsonLines (New-SampleObjectFromFields -Fields $bestFields)
    }

    foreach ($_match in [regex]::Matches($Body, 'return\s+Ok\s*\(\s*(?<expr>[^;\r\n]+?)\s*\)\s*;')) {
        $expr = $_match.Groups['expr'].Value.Trim()
        if ($expr -match '^new\s*\{') { return }

        if ($Assignments.ContainsKey($expr)) {
            $assignment = $Assignments[$expr]
            if ($FieldTypes.ContainsKey($assignment.ServiceField)) {
                $serviceType = $FieldTypes[$assignment.ServiceField]
                if ($ServiceCatalog.ContainsKey($serviceType) -and $ServiceCatalog[$serviceType].ContainsKey($assignment.MethodName)) {
                    $desc = Describe-Type -TypeName $ServiceCatalog[$serviceType][$assignment.MethodName] -ModelCatalog $ModelCatalog
                    if ($desc.Display -like 'Array of *') {
                        return (Convert-SampleToJsonLines @((New-SampleObjectFromFields -Fields $desc.Fields)))
                    }
                    if ($desc.Fields.Count -gt 0) {
                        return (Convert-SampleToJsonLines (New-SampleObjectFromFields -Fields $desc.Fields))
                    }
                }
            }
        }
        elseif ($expr -match '^"(?<msg>.*)"$') {
            return (Convert-SampleToJsonLines ([ordered]@{ message = $Matches['msg'] }))
        }
    }

    return Convert-SampleToJsonLines ([ordered]@{ message = 'Success' })
}

function Get-OperationalNotes {
    param([string]$Body)
    $notes = New-Object System.Collections.Generic.List[string]
    if ($Body -match "CreateNotification") { $notes.Add("Triggers an in-app notification.") }
    if ($Body -match 'SendAsync\("EmployeeLocationUpdated"') { $notes.Add("Publishes a real-time SignalR location update event.") }
    if ($Body -match "ZipArchive") { $notes.Add("Can return a ZIP archive when requested.") }
    if ($Body -match "IFormFile" -or $Body -match "CopyToAsync" -or $Body -match "SaveResumeAsync") { $notes.Add("Includes file upload or file storage handling.") }
    if ($Body -match "CreateLog") { $notes.Add("Writes an audit or business log entry.") }
    return @($notes | Select-Object -Unique)
}

function Escape-Xml { param([string]$Text) if ($null -eq $Text) { return "" } return [System.Security.SecurityElement]::Escape($Text) }
function Add-DocParagraph2 {
    param([System.Collections.Generic.List[string]]$XmlLines,[string]$Text,[string]$Style='Normal')
    $safe = [System.Security.SecurityElement]::Escape($Text)
    $XmlLines.Add('<w:p><w:pPr><w:pStyle w:val="' + $Style + '"/></w:pPr><w:r><w:t xml:space="preserve">' + $safe + '</w:t></w:r></w:p>')
}
function Add-BlankLine2 { param([System.Collections.Generic.List[string]]$XmlLines) Add-DocParagraph2 -XmlLines $XmlLines -Text '' }

function New-DocxPackage {
    param([string]$OutputPath,[string[]]$ParagraphXml)
    if (Test-Path $OutputPath) { Remove-Item -LiteralPath $OutputPath -Force }
    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("hrcrm-doc-" + [guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $tempDir | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $tempDir "_rels") | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $tempDir "word") | Out-Null
    $contentTypes = @"
<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/word/document.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml""/>
  <Override PartName=""/word/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml""/>
</Types>
"@
    $rels = @"
<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""word/document.xml""/>
</Relationships>
"@
    $styles = @"
<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:styles xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:style w:type=""paragraph"" w:default=""1"" w:styleId=""Normal""><w:name w:val=""Normal""/><w:qFormat/><w:rPr><w:sz w:val=""22""/></w:rPr></w:style>
  <w:style w:type=""paragraph"" w:styleId=""Title""><w:name w:val=""Title""/><w:qFormat/><w:rPr><w:b/><w:sz w:val=""32""/></w:rPr></w:style>
  <w:style w:type=""paragraph"" w:styleId=""Heading1""><w:name w:val=""heading 1""/><w:qFormat/><w:rPr><w:b/><w:sz w:val=""28""/></w:rPr></w:style>
  <w:style w:type=""paragraph"" w:styleId=""Heading2""><w:name w:val=""heading 2""/><w:qFormat/><w:rPr><w:b/><w:sz w:val=""24""/></w:rPr></w:style>
</w:styles>
"@
    [System.IO.File]::WriteAllText((Join-Path $tempDir "[Content_Types].xml"), $contentTypes, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText((Join-Path $tempDir "_rels\.rels"), $rels, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText((Join-Path $tempDir "word\styles.xml"), $styles, [System.Text.Encoding]::UTF8)
    $doc = @('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>','<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">','<w:body>') + $ParagraphXml + @('<w:sectPr/>','</w:body>','</w:document>')
    [System.IO.File]::WriteAllText((Join-Path $tempDir "word\document.xml"), ($doc -join [Environment]::NewLine), [System.Text.Encoding]::UTF8)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $OutputPath)
    Remove-Item -LiteralPath $tempDir -Recurse -Force
}

$root = Split-Path -Parent $PSScriptRoot
$controllersPath = Join-Path $root "Controllers"
$dtoPath = Join-Path $root "DTO"
$entitiesPath = Join-Path $root "Entities"
$serviceInterfacePath = Join-Path $root "Service\Interface"
$docsPath = Join-Path $root "docs"
if (-not (Test-Path $docsPath)) { New-Item -ItemType Directory -Path $docsPath | Out-Null }

$modelCatalog = Get-ClassPropertyCatalog -Folders @($dtoPath, $entitiesPath)
$serviceCatalog = Get-ServiceMethodCatalog -Folder $serviceInterfacePath
$paragraphs = New-Object System.Collections.Generic.List[string]
Add-DocParagraph2 -XmlLines $paragraphs -Text "HR CRM Module-Wise API Documentation" -Style "Title"
Add-DocParagraph2 -XmlLines $paragraphs -Text ("Generated from source code on " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss"))
Add-DocParagraph2 -XmlLines $paragraphs -Text "This document explains the HR CRM API module by module. Each endpoint includes a short purpose note, a sample request, and one sample success response in JSON format."
Add-BlankLine2 -XmlLines $paragraphs
Add-DocParagraph2 -XmlLines $paragraphs -Text "System Overview" -Style "Heading1"
Add-DocParagraph2 -XmlLines $paragraphs -Text "Technology stack: ASP.NET Core Web API, .NET 8, Entity Framework Core, PostgreSQL, Swagger, JWT authentication, static files, and SignalR."
Add-DocParagraph2 -XmlLines $paragraphs -Text "Security exists in the codebase, but this document focuses on endpoint purpose and sample JSON structures."
Add-DocParagraph2 -XmlLines $paragraphs -Text "Real-time endpoints: /hubs/location and /hubs/notifications."
Add-BlankLine2 -XmlLines $paragraphs

$moduleCount = 0
$endpointCount = 0
foreach ($controllerFile in (Get-ChildItem -Path $controllersPath -Filter *.cs -File | Sort-Object Name)) {
    $content = Get-Content $controllerFile.FullName -Raw
    $classMatch = [regex]::Match($content, "class\s+(?<name>\w+Controller)\b")
    if (-not $classMatch.Success) { continue }
    $controllerClass = $classMatch.Groups["name"].Value
    $methods = @(Get-MethodBlocks -Content $content)
    if ($methods.Count -eq 0) { continue }
    $moduleCount++
    $baseRoute = Resolve-ControllerRoute -Content $content -ControllerClass $controllerClass
    $fieldTypes = Get-FieldTypeMap -Content $content
    Add-DocParagraph2 -XmlLines $paragraphs -Text (Get-ModuleLabel -ControllerClass $controllerClass) -Style "Heading1"
    Add-DocParagraph2 -XmlLines $paragraphs -Text ("Module description: " + (Get-ModuleDescription -ControllerClass $controllerClass))
    Add-DocParagraph2 -XmlLines $paragraphs -Text ("Base route: " + $baseRoute)
    Add-DocParagraph2 -XmlLines $paragraphs -Text ("Endpoints in this module: " + $methods.Count)
    Add-BlankLine2 -XmlLines $paragraphs
    foreach ($method in $methods) {
        $http = [regex]::Match($method.Attributes, '\[(?<verb>HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\("(?<route>[^"]*)"\))?\]')
        if (-not $http.Success) { continue }
        $verb = ($http.Groups["verb"].Value -replace "^Http", "").ToUpperInvariant()
        $fullRoute = Merge-Route -BaseRoute $baseRoute -MethodRoute $http.Groups["route"].Value
        $endpointCount++
        $parameters = @(Get-ParamMetadata -ParameterText $method.Parameters)
        $assignments = Get-BodyAssignments -Body $method.Body
        $requestJsonLines = @(Get-SampleRequestJsonLines -Parameters $parameters -ModelCatalog $modelCatalog)
        $responseJsonLines = @(Get-SampleSuccessJsonLines -Body $method.Body -Assignments $assignments -FieldTypes $fieldTypes -ServiceCatalog $serviceCatalog -ModelCatalog $modelCatalog)
        $notes = @(Get-OperationalNotes -Body $method.Body)
        Add-DocParagraph2 -XmlLines $paragraphs -Text ("Endpoint: $verb $fullRoute") -Style "Heading2"
        Add-DocParagraph2 -XmlLines $paragraphs -Text ("What it does: " + (Get-EndpointPurpose -MethodName $method.Name -Verb $verb -Route $fullRoute))
        Add-DocParagraph2 -XmlLines $paragraphs -Text "Sample Request:"
        foreach ($line in $requestJsonLines) { Add-DocParagraph2 -XmlLines $paragraphs -Text $line }
        Add-DocParagraph2 -XmlLines $paragraphs -Text "Sample Success Response:"
        foreach ($line in $responseJsonLines) { Add-DocParagraph2 -XmlLines $paragraphs -Text $line }
        foreach ($n in $notes) { Add-DocParagraph2 -XmlLines $paragraphs -Text ("Note: " + $n) }
        Add-BlankLine2 -XmlLines $paragraphs
    }
}

Add-DocParagraph2 -XmlLines $paragraphs -Text "Model Reference" -Style "Heading1"
Add-DocParagraph2 -XmlLines $paragraphs -Text "The following DTOs and entities are available in the current codebase and can be used as field-level references while integrating with the API."
foreach ($name in ($modelCatalog.Keys | Sort-Object)) {
    Add-DocParagraph2 -XmlLines $paragraphs -Text $name -Style "Heading2"
    Add-DocParagraph2 -XmlLines $paragraphs -Text ("Defined in: " + $modelCatalog[$name].SourceFile)
    Add-DocParagraph2 -XmlLines $paragraphs -Text ("Fields: " + (($modelCatalog[$name].Properties | ForEach-Object { "$($_.Name) ($($_.Type))" }) -join ", "))
}
Add-DocParagraph2 -XmlLines $paragraphs -Text "Document Summary" -Style "Heading1"
Add-DocParagraph2 -XmlLines $paragraphs -Text ("Total modules documented: " + $moduleCount)
Add-DocParagraph2 -XmlLines $paragraphs -Text ("Total endpoints documented: " + $endpointCount)

$docxPath = Join-Path $docsPath "HR_CRM_Module_Wise_API_Documentation.docx"
$txtPath = Join-Path $docsPath "HR_CRM_Module_Wise_API_Documentation.txt"
New-DocxPackage -OutputPath $docxPath -ParagraphXml $paragraphs
[System.IO.File]::WriteAllText($txtPath, (($paragraphs | ForEach-Object { $_ -replace "<[^>]+>", "" }) -join [Environment]::NewLine), [System.Text.Encoding]::UTF8)
Write-Output ("Generated: " + $docxPath)
Write-Output ("Generated: " + $txtPath)
