using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// GET /connect/idp-logout — front-channel single logout.
/// Clears the __Host-w3c-idp-session SSO cookie so that logging out of the website also
/// ends the IdP's OIDC SSO session. The website loads this URL as a hidden image during
/// logout; a subsequent OIDC login (e.g. Quackback) then finds no IdP session, bounces
/// back to the website, and — with no W3ChampionsJWT — re-prompts for Battle.net instead
/// of silently reusing the (up-to-30-min) IdP session.
///
/// No CSRF/Origin gate: ending a session is not a sensitive state change worth protecting
/// (a forced logout is, at worst, a re-login prompt), and a cross-origin &lt;img&gt; GET
/// carries no Origin header to check anyway. SignOutAsync emits the clearing Set-Cookie
/// whether or not a session is present, so the call is safe and idempotent.
/// </summary>
[ApiController]
public class OidcLogoutController : ControllerBase
{
    [HttpGet("~/connect/idp-logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Headers.CacheControl = "no-store";
        return NoContent();
    }
}
