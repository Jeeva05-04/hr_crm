using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

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

        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        var method = context.HttpContext.Request.Method;

        Console.WriteLine("ROLE: " + role);

        // ADMIN → full access
        if (role == "ADMIN")
            return;

        // HR_MANAGER → full access
        if (role == "HR_MANAGER")
            return;

        // HR_USER → only GET
        if (role == "HR_USER")
        {
            if (method != "GET")
            {
                context.Result = new ForbidResult();
            }
        }
    }
}