using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        // Returns the Authorization header (if present) so you can confirm the token is sent
        [HttpGet("auth-header")]
        [AllowAnonymous]
        public IActionResult GetAuthHeader()
        {
            var auth = Request.Headers["Authorization"].FirstOrDefault();
            return Ok(new
            {
                HasAuthorizationHeader = !string.IsNullOrEmpty(auth),
                AuthorizationHeader = auth
            });
        }

        // Decode the JWT without validating to inspect claims (useful for debugging)
        [HttpGet("decode-token")]
        [AllowAnonymous]
        public IActionResult DecodeToken()
        {
            var auth = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer "))
                return BadRequest(new { Message = "No Bearer token found in Authorization header." });

            var token = auth.Substring("Bearer ".Length).Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var payload = jwt.Claims.Select(c => new { c.Type, c.Value }).ToList();
                return Ok(new { Header = jwt.Header, Payload = payload });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = "Failed to decode token.", Error = ex.Message });
            }
        }

        // Shows what the server sees for authenticated requests (claims). Requires a valid token.
        [HttpGet("claims")]
        [Authorize]
        public IActionResult GetClaims()
        {
            return Ok(new
            {
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                AuthenticationType = User?.Identity?.AuthenticationType,
                Claims = User?.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}
