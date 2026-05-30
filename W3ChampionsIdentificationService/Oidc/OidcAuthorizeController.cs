using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Serilog;
using W3ChampionsIdentificationService.Config;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace W3ChampionsIdentificationService.Oidc;

[ApiController]
public class OidcAuthorizeController(IAppConfig appConfig) : ControllerBase
{
    private readonly IAppConfig _appConfig = appConfig;

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var oidcRequest = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request not available");

        // Check for an existing IdP session cookie.
        var result = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal == null)
        {
            // No IdP session — redirect the browser to the website for interactive login.
            var selfAbsoluteUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
            var redirectUrl = $"{_appConfig.WebsiteLoginUrl}?return={HttpUtility.UrlEncode(selfAbsoluteUrl)}";

            Log.Information("No IdP session — redirecting to website login at {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }

        var battleTag = result.Principal.FindFirstValue("battleTag")
            ?? throw new InvalidOperationException("battleTag claim missing from IdP session");

        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.AddClaim(Claims.Subject, battleTag);
        identity.AddClaim(Claims.Name, battleTag); // full battletag (incl. #discriminator) as display name — keeps users identifiable
        identity.AddClaim(Claims.Email, OidcClaimMapper.BattleTagToSyntheticEmail(battleTag));
        identity.AddClaim(Claims.EmailVerified, "false");

        identity.SetDestinations(claim => claim.Type switch
        {
            Claims.Subject       => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            Claims.Name          => new[] { Destinations.IdentityToken },
            Claims.Email         => new[] { Destinations.IdentityToken },
            Claims.EmailVerified => new[] { Destinations.IdentityToken },
            _                    => new[] { Destinations.AccessToken }
        });

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(oidcRequest.GetScopes());

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
