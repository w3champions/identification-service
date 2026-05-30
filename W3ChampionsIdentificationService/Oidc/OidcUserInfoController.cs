using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
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

        // The userinfo endpoint is authenticated via the access token, which carries only
        // `sub` as a standard claim (name/email destinations are IdentityToken-only — see
        // OidcAuthorizeController.SetDestinations). Re-derive the claims from `sub` (the
        // battleTag) gated on the scopes the access token was actually granted, mirroring
        // the authorize-endpoint gating.
        //
        // GetScopes() reads the `oi_scp` claims embedded in the access token by SetScopes()
        // in the authorize controller. OpenIddict 7.5.0 stores scopes as individual `oi_scp`
        // claims in the JWT and GetScopes(ClaimsPrincipal) reads them back reliably.
        var principal = result.Principal;
        var sub = principal.FindFirstValue(Claims.Subject);   // = battleTag
        var scopes = principal.GetScopes();                       // granted scopes from the access token

        var response = new Dictionary<string, object> { ["sub"] = sub };
        if (scopes.Contains(Scopes.Profile))
            response["name"] = sub;                               // name = full battletag (incl. #discriminator)
        if (scopes.Contains(Scopes.Email))
        {
            response["email"] = OidcClaimMapper.BattleTagToSyntheticEmail(sub);
            response["email_verified"] = false;
        }
        return Ok(response);
    }
}
