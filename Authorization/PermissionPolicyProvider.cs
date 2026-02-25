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
            var policy = new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                    context.User.HasClaim("perm", "CRM_FULL_ACCESS") ||
                    context.User.HasClaim("perm", policyName)
                )
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
    }
}