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

        // HR Manager → full access
        if (role == "HR_MANAGER")
            return;

        if (role == "USER")
        {
            var path = context.HttpContext.Request.Path.Value;

            // allow attendance actions
            if (path.Contains("check-in") || path.Contains("check-out"))
                return;

            // allow GET only
            if (method != "GET")
            {
                context.Result = new ForbidResult();
            }
        }
    }
}