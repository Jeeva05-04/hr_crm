using hr_crm.Service;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace hr_crm.Filters
{
    public class AuditActionFilter : IAsyncActionFilter
    {
        private readonly LoggingService _loggingService;

        public AuditActionFilter(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Capture info before action executes
            var http = context.HttpContext;
            var user = http.User;
            string? userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;
            // Prefer friendly display name using extension
            string? userName = hr_crm.Extensions.ClaimsPrincipalExtensions.GetDisplayName(user);

            var controller = context.RouteData.Values.TryGetValue("controller", out var c) ? c?.ToString() : null;
            var action = context.RouteData.Values.TryGetValue("action", out var a) ? a?.ToString() : null;

            // Serialize action parameters (best-effort)
            string argsJson = string.Empty;
            try
            {
                if (context.ActionArguments != null && context.ActionArguments.Count > 0)
                {
                    var options = new JsonSerializerOptions { WriteIndented = false };
                    argsJson = JsonSerializer.Serialize(context.ActionArguments, options);
                    if (argsJson.Length > 2000) argsJson = argsJson.Substring(0, 2000) + "...";
                }
            }
            catch { argsJson = string.Empty; }

            // Track duration
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Execute the action
            var executed = await next();
            sw.Stop();

            // Build action and details with richer, production-ready info
            var ctxRequest = executed.HttpContext.Request;
            var ctxResponse = executed.HttpContext.Response;
            var status = ctxResponse?.StatusCode ?? 0;

            // Action: HTTP_METHOD PATH → Controller.Action (Status)
            var path = ctxRequest?.Path.HasValue == true ? ctxRequest.Path.Value : "";
            var query = ctxRequest?.QueryString.HasValue == true ? ctxRequest.QueryString.Value : string.Empty;
            var actionDesc = $"{ctxRequest.Method} {path}{query} → {controller}.{action} (Status: {status})";

            var detailsSb = new System.Text.StringBuilder();
            detailsSb.Append("DurationMs=").Append(sw.ElapsedMilliseconds);
            detailsSb.Append("; RemoteIp=").Append(executed.HttpContext.Connection.RemoteIpAddress);
            var ua = ctxRequest.Headers["User-Agent"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ua)) detailsSb.Append("; UserAgent=").Append(ua.Length > 200 ? ua.Substring(0, 200) + "..." : ua);
            if (!string.IsNullOrEmpty(query)) detailsSb.Append("; Query=").Append(query);
            if (!string.IsNullOrEmpty(argsJson)) detailsSb.Append("; Args=").Append(argsJson);

            // Include exception info if action failed
            if (executed.Exception != null)
            {
                detailsSb.Append("; Exception=").Append(executed.Exception.GetType().Name).Append(": ").Append(executed.Exception.Message);
                if (executed.Exception.StackTrace != null)
                {
                    var st = executed.Exception.StackTrace;
                    detailsSb.Append("; StackTrace=").Append(st.Length > 1000 ? st.Substring(0, 1000) + "..." : st);
                }
            }

            // Include authenticated roles if available
            try
            {
                var roles = user?.Claims?.Where(cl => cl.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase) || cl.Type == System.Security.Claims.ClaimTypes.Role)
                                .Select(cl => cl.Value).Distinct().ToList();
                if (roles != null && roles.Any()) detailsSb.Append("; Roles=").Append(string.Join(',', roles));
            }
            catch { }

            int? uid = null;
            if (int.TryParse(userId, out var parsed)) uid = parsed;

            // Fire-and-forget logging (await to ensure persistence)
            try
            {
                var uaShort = ua != null ? (ua.Length > 1000 ? ua.Substring(0, 1000) : ua) : null;
                var dur = (int)Math.Min(sw.ElapsedMilliseconds, int.MaxValue);
                var remoteIp = executed.HttpContext.Connection.RemoteIpAddress?.ToString();
                var traceId = executed.HttpContext.TraceIdentifier;

                await _loggingService.CreateLog(
                    uid,
                    userName,
                    actionDesc,
                    detailsSb.ToString(),
                    statusCode: status,
                    durationMs: dur,
                    controllerName: controller,
                    actionName: action,
                    userAgent: uaShort,
                    correlationId: traceId
                );
            }
            catch { /* don't let logging failures affect request */ }
        }
    }
}
