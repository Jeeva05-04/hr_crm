using System.Linq;
using System.Security.Claims;

namespace hr_crm.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetDisplayName(this ClaimsPrincipal? user)
        {
            if (user == null)
                return null;

            // Prefer explicit name claims
            var name = user.FindFirst("name")?.Value
                       ?? user.FindFirst(ClaimTypes.Name)?.Value
                       ?? user.FindFirst("preferred_username")?.Value
                       ?? user.FindFirst("email")?.Value;

            if (!string.IsNullOrEmpty(name))
            {
                // treat numeric-only values (some identity providers set 'name' to user id) as not a real display name
                if (name.All(char.IsDigit))
                    name = null;

                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            // Fallback to given + family
            var given = user.FindFirst("given_name")?.Value;
            var family = user.FindFirst("family_name")?.Value;
            var parts = new[] { given, family }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (parts.Length > 0)
                return string.Join(' ', parts);

            // Last resort: return id/sub
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        }
    }
}
