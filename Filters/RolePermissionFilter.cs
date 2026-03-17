using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

        // Check role from both raw "role" claim and ClaimTypes.Role (handles upper/lowercase)
        var role = (user.FindFirst("role")?.Value
                 ?? user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                   )?.ToUpper();

        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path.Value;

        // HR Manager → full access
        if (role == "HR_MANAGER")
            return;

        if (role == "USER" || role == "HR_USER")
        {
            // allow attendance actions
            if (path.Contains("checkin") || path.Contains("check-out"))
                return;

            // allow users to delete their own notifications
            if (method == "DELETE" && path.Contains("/notification", System.StringComparison.OrdinalIgnoreCase))
                return;

            // USER → only GET allowed
            if (method != "GET")
            {
                context.Result = new ForbidResult();
                return;
            }

            // Prevent accessing other users data
            var tokenUserId = user.FindFirst("sub")?.Value;

            if (context.RouteData.Values.ContainsKey("userId"))
            {
                var routeUserId = context.RouteData.Values["userId"]?.ToString();

                if (routeUserId != tokenUserId)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }
}