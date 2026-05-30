using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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

    // GET-only: OIDC relying parties (including Better Auth / Quackback) initiate the
    // authorization request via a GET redirect. OIDC params live in the query string, which
    // the website-handoff resume URL ($"{issuerBase}{Request.Path}{Request.QueryString}")
    // already preserves. POST support is dropped because form-body params are NOT included
    // in that resume URL, so the resumed GET would arrive with no client_id/redirect_uri/
    // scope/state/PKCE and OpenIddict would reject the flow.
    [HttpGet("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var oidcRequest = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request not available");

        // Check for an existing IdP session cookie and resolve the prompt directives together so
        // the none/login handling stays coherent.
        var result = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        var hasSession = result.Succeeded && result.Principal != null;
        var promptNone = oidcRequest.HasPromptValue(PromptValues.None);
        var promptLogin = oidcRequest.HasPromptValue(PromptValues.Login);

        // prompt=login forces reauthentication: discard any existing IdP session so the user is
        // sent back through login (account switch / fresh login) instead of being silently served
        // the previous BattleTag.
        if (promptLogin && hasSession)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Log.Information("prompt=login for client {ClientId} — cleared existing IdP session to force reauthentication", oidcRequest.ClientId);
            hasSession = false;
        }

        if (!hasSession)
        {
            // OIDC silent auth: a prompt=none authorize request with no IdP session must NOT
            // trigger interactive login. Per the spec it returns an immediate login_required
            // error to the client's redirect_uri (silent-auth callers expect this, never an
            // interactive redirect). OpenIddict translates this Forbid into the standard error
            // response on the token/authorize channel. (prompt=none with prompt=login is a
            // contradictory request; we honor prompt=none here and return login_required rather
            // than redirecting to interactive login.)
            if (promptNone)
            {
                Log.Information("prompt=none with no IdP session for client {ClientId} — returning login_required", oidcRequest.ClientId);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in.",
                    }));
            }

            // No IdP session — redirect the browser to the website for interactive login.
            // Build the self URL from the canonical, startup-validated OIDC issuer rather than
            // Request.Host: behind the Traefik→nginx-proxy chain X-Forwarded-Host is NOT trusted
            // (Startup only forwards For/Proto), so Request.Host is the internal Docker host. The
            // query string carries client_id/redirect_uri/scope/state/PKCE and is preserved; OpenIddict
            // re-parses the request from the query, not the host.
            //
            // STRIP `prompt` from the return URL: prompt=login means "force reauth ONCE". If we kept
            // it, the post-handoff continuation would still carry prompt=login → we'd sign out the
            // freshly-set session and redirect again → infinite loop. Removing it means the resumed
            // request has no prompt, so the controller then sees hasSession && !promptLogin and issues
            // the code. This service only handles prompt=none/login and has no consent screen
            // (ConsentType.Implicit), so there's no prompt=consent semantics to preserve.
            var query = QueryHelpers.ParseQuery(Request.QueryString.Value ?? "");
            query.Remove("prompt");
            var strippedQuery = QueryString.Create(
                query.SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string>(kv.Key, v))));
            var issuerBase = _appConfig.OidcIssuer.TrimEnd('/');   // guard against a trailing slash → "//connect"
            var selfAbsoluteUrl = $"{issuerBase}{Request.Path}{strippedQuery}";
            var redirectUrl = $"{_appConfig.WebsiteLoginUrl}?return={HttpUtility.UrlEncode(selfAbsoluteUrl)}";

            Log.Information("No IdP session for client {ClientId} — redirecting to website login {LoginUrl}", oidcRequest.ClientId, _appConfig.WebsiteLoginUrl);
            return Redirect(redirectUrl);
        }

        var battleTag = result.Principal.FindFirstValue("battleTag")
            ?? throw new InvalidOperationException("battleTag claim missing from IdP session");

        var scopes = oidcRequest.GetScopes();

        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.AddClaim(Claims.Subject, battleTag);
        if (scopes.Contains(Scopes.Profile))
            identity.AddClaim(Claims.Name, battleTag); // full battletag (incl. #discriminator) — keeps users identifiable
        if (scopes.Contains(Scopes.Email))
        {
            identity.AddClaim(Claims.Email, OidcClaimMapper.BattleTagToSyntheticEmail(battleTag));
            identity.AddClaim(new Claim(Claims.EmailVerified, "false", ClaimValueTypes.Boolean)); // typed bool → serialized as JSON false, matching userinfo
        }

        var principal = new ClaimsPrincipal(identity);

        // Order matters: SetScopes FIRST so it adds the private scope claims (oi_scp), THEN assign
        // destinations over the FULL claim set (principal-level SetDestinations iterates all claims).
        // Otherwise the oi_scp claims, added after destination assignment, would get no destination
        // and be omitted from the access token — leaving OidcUserInfoController.GetScopes() empty so
        // userinfo drops name/email. The `_ => AccessToken` default routes oi_scp into the access token.
        principal.SetScopes(scopes);
        principal.SetDestinations(claim => claim.Type switch
        {
            Claims.Subject => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            Claims.Name => new[] { Destinations.IdentityToken },
            Claims.Email => new[] { Destinations.IdentityToken },
            Claims.EmailVerified => new[] { Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        });

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
