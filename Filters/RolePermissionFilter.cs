using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

public class RolePermissionFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var role = (user.FindFirst("role")?.Value
                 ?? user.FindFirst(ClaimTypes.Role)?.Value
                   )?.ToUpper();

        // HR Manager → full access, no restrictions
        if (role == "HR_MANAGER")
            return;

        if (role == "USER" || role == "HR_USER")
        {
            // Resolve token userId — check both "sub" and ClaimTypes.NameIdentifier
            var tokenUserId = user.FindFirst("sub")?.Value
                           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var path = context.HttpContext.Request.Path.Value ?? string.Empty;
            var method = context.HttpContext.Request.Method;

            // Prevent accessing another user's data via userId route parameter
            // (applies to all methods — GET, PUT, DELETE, etc.)
            if (context.RouteData.Values.ContainsKey("userId"))
            {
                var routeUserId = context.RouteData.Values["userId"]?.ToString();

                if (!string.IsNullOrEmpty(tokenUserId) && routeUserId != tokenUserId)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            // All other access control is handled by [HasPermission] attributes
            // on each individual endpoint — do not block here based on HTTP method
        }
    }
}
