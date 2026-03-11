using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RolePermissionFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var role = user.FindFirst("role")?.Value;
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path.Value;

        // HR Manager → full access
        if (role == "HR_MANAGER")
            return;

        if (role == "USER")
        {
            // allow attendance actions
            if (path.Contains("check-in") || path.Contains("check-out"))
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