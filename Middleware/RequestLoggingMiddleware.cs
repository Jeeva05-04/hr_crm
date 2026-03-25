using hr_crm.Service;
using hr_crm.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.IO;
using System.Text;
using System.Text.Json;

namespace hr_crm.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var user = context.User;
                string? userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;
                string? userName = user.GetDisplayName();
                var method = context.Request.Method;
                var path = context.Request.Path;
                var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;

                // Skip logging for internal or static paths
                var p = context.Request.Path.Value ?? string.Empty;
                if (p.StartsWith("/swagger") || p.StartsWith("/hubs") || p.StartsWith("/favicon.ico"))
                {
                    await _next(context);
                    return;
                }

                // Try to read a short request body (best-effort, non-blocking)
                string? requestBody = null;
                try
                {
                    if (!HttpMethods.IsGet(method) && context.Request.ContentLength > 0 && context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
                    {
                        context.Request.EnableBuffering();
                        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                        requestBody = await reader.ReadToEndAsync();
                        requestBody = (requestBody ?? string.Empty).Trim();
                        if (requestBody.Length > 1000) requestBody = requestBody.Substring(0, 1000) + "...";
                        context.Request.Body.Position = 0;
                    }
                }
                catch { /* ignore body read errors */ }

                // Capture endpoint/route info if available
                var endpoint = context.GetEndpoint();
                var endpointDisplay = endpoint?.DisplayName;
                var routeValues = context.Request.RouteValues != null && context.Request.RouteValues.Count > 0
                    ? string.Join(';', context.Request.RouteValues.Select(kv => kv.Key + "=" + kv.Value))
                    : string.Empty;

                // Let the request execute
                await _next(context);

                var status = context.Response.StatusCode;

                // Build descriptive action and details
                var action = new StringBuilder();
                action.Append(method).Append(' ').Append(path);
                if (!string.IsNullOrEmpty(endpointDisplay)) action.Append(' ').Append('[').Append(endpointDisplay).Append(']');
                if (!string.IsNullOrEmpty(routeValues)) action.Append(' ').Append('{').Append(routeValues).Append('}');
                action.Append(" -> ").Append(status);

                var details = new StringBuilder();
                details.Append("RemoteIp=").Append(context.Connection.RemoteIpAddress);
                if (!string.IsNullOrEmpty(query)) details.Append("; Query=").Append(query);
                if (!string.IsNullOrEmpty(requestBody)) details.Append("; Body=").Append(requestBody);

                int? uid = null;
                if (int.TryParse(userId, out var parsed)) uid = parsed;

                // Resolve scoped LoggingService per-request from the request services
                var loggingService = context.RequestServices.GetService<LoggingService>();
                if (loggingService != null)
                {
                    await loggingService.CreateLog(uid, userName, action.ToString(), details.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create request log");
            }
        }
    }
}
