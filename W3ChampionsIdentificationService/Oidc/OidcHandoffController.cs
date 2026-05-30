using System;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// POST /connect/handoff
/// Accepts the w3c JWT from the website, validates it, establishes the IdP session
/// cookie, then 302-redirects back to /connect/authorize to complete the OIDC flow.
/// </summary>
[ApiController]
public class OidcHandoffController(IAppConfig appConfig) : ControllerBase
{
    private readonly IAppConfig _appConfig = appConfig;

    private static readonly string JwtPublicKey =
        Regex.Unescape(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY") ?? "");

    // Parse the public key once for the process (thread-safe). Avoids per-request
    // native RSA allocation / PEM re-parsing on the SSO hot path.
    private static readonly Lazy<HandoffJwtValidator> Validator =
        new(() => new HandoffJwtValidator(JwtPublicKey));

    [HttpPost("~/connect/handoff")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Handoff([FromForm] HandoffRequest request)
    {
        // Login-CSRF defense (FIRST check): the legitimate handoff is auto-submitted from the
        // website's /sso-continue page, so its Origin is the website origin. A cross-site page
        // auto-submitting an attacker-controlled JWT carries a foreign Origin — reject it before
        // touching the token, so it can never fixate an IdP session for the attacker's BattleTag.
        var origin = Request.Headers.Origin.ToString();
        if (!HandoffOriginValidator.IsAllowedOrigin(origin, _appConfig.WebsiteLoginUrl))
        {
            Log.Warning("Handoff rejected: Origin '{Origin}' does not match the website login origin", origin);
            return BadRequest("invalid origin");
        }

        if (string.IsNullOrEmpty(request.Jwt))
        {
            Log.Warning("Handoff called without jwt form field");
            return BadRequest("jwt is required");
        }

        if (!HandoffReturnUrlValidator.IsAllowed(request.Return))
        {
            Log.Warning("Handoff called with disallowed return URL: {Return}", request.Return);
            return BadRequest("return URL is not allowed");
        }

        string battleTag;
        try
        {
            battleTag = Validator.Value.ValidateAndExtractBattleTag(request.Jwt);
        }
        catch (HandoffValidationException ex)
        {
            Log.Warning("Handoff JWT validation failed: {Message}", ex.Message);
            return Unauthorized("Invalid or expired token");
        }

        // Establish the IdP session cookie.
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim("battleTag", battleTag));

        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        Log.Information("IdP session established for {BattleTag}, resuming OIDC flow", battleTag);

        return Redirect(request.Return);
    }
}

public class HandoffRequest
{
    public string Jwt { get; set; }
    public string Return { get; set; }
}
