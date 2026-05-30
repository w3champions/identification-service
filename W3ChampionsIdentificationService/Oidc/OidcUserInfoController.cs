using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace W3ChampionsIdentificationService.Oidc;

[ApiController]
public class OidcUserInfoController : ControllerBase
{
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var result = await HttpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        if (result?.Principal == null)
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var principal = result.Principal;
        var sub   = principal.FindFirstValue(Claims.Subject);
        var name  = principal.FindFirstValue(Claims.Name);
        var email = principal.FindFirstValue(Claims.Email);

        return Ok(new
        {
            sub,
            name,
            email,
            email_verified = false,
        });
    }
}
