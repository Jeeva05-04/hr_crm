using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace hr_crm.Authorization
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {

            // Only handle policies that start with "Permission:"
            if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName.Substring("Permission:".Length);

            // Remove "Permission:" prefix if exists
            var permission = policyName.Replace("Permission:", "");

            var policy = new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                    context.User.HasClaim("perm", "CRM_FULL_ACCESS") ||
                    context.User.HasClaim("perm", permission)
                )
                .Build();


                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        context.User.HasClaim("perm", "CRM_FULL_ACCESS") ||
                        context.User.HasClaim("perm", permission)
                    )
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}