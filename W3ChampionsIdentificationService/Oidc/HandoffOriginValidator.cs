using System;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// Login-CSRF defense for the handoff POST.
///
/// The legitimate handoff is auto-submitted from the website's /sso-continue page, so its
/// Origin header is the website origin (= the origin of WEBSITE_LOGIN_URL). A cross-site
/// attacker auto-submitting a forged JWT carries a different Origin, so rejecting POSTs whose
/// Origin is not the website origin prevents session fixation: an attacker can no longer fixate
/// the victim's browser with an IdP session for the attacker's BattleTag.
///
/// The w3champions site is reachable on BOTH the apex (https://w3champions.com) and www
/// (https://www.w3champions.com). /sso-continue may be served from (or canonicalize to) either,
/// so the allowed set is the configured origin AND its apex/www sibling (same scheme + port).
/// This stays same-site — only the apex+www of the CONFIGURED host are accepted; any other host,
/// scheme, port, or lookalike suffix is still rejected.
/// </summary>
public static class HandoffOriginValidator
{
    private const string WwwPrefix = "www.";

    /// <summary>
    /// Returns true only if <paramref name="requestOrigin"/> is a bare origin (no path/query/fragment)
    /// whose scheme+host+port (case-insensitive) matches the origin of <paramref name="websiteLoginUrl"/>
    /// OR that origin's apex/www sibling. Returns false on null/empty/mismatch, or when either value
    /// is unparseable.
    /// </summary>
    public static bool IsAllowedOrigin(string requestOrigin, string websiteLoginUrl)
    {
        if (string.IsNullOrEmpty(requestOrigin))
            return false;
        if (string.IsNullOrEmpty(websiteLoginUrl))
            return false;

        if (!Uri.TryCreate(websiteLoginUrl, UriKind.Absolute, out var loginUri))
            return false;
        if (!Uri.TryCreate(requestOrigin, UriKind.Absolute, out var originUri))
            return false;

        // A real Origin header is a bare scheme+host+port with no path/query/fragment. Reject
        // anything carrying a path (e.g. "https://site/evil") so an extra-path lookalike can't
        // be folded onto an allowed authority by GetLeftPart below.
        if (originUri.AbsolutePath != "/" || !string.IsNullOrEmpty(originUri.Query) || !string.IsNullOrEmpty(originUri.Fragment))
            return false;

        var actualOrigin = originUri.GetLeftPart(UriPartial.Authority);

        foreach (var allowed in AllowedOriginsFor(loginUri))
        {
            if (string.Equals(actualOrigin, allowed, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// The configured origin plus its apex/www sibling (same scheme + port). If the host begins
    /// with "www.", the sibling drops it; otherwise the sibling prepends "www.".
    /// </summary>
    private static IEnumerable<string> AllowedOriginsFor(Uri loginUri)
    {
        var configured = loginUri.GetLeftPart(UriPartial.Authority);
        yield return configured;

        var host = loginUri.Host;
        var siblingHost = host.StartsWith(WwwPrefix, StringComparison.OrdinalIgnoreCase)
            ? host.Substring(WwwPrefix.Length)
            : WwwPrefix + host;

        // Rebuild the sibling origin preserving scheme + (non-default) port.
        var siblingBuilder = new UriBuilder(loginUri.Scheme, siblingHost);
        if (!loginUri.IsDefaultPort)
            siblingBuilder.Port = loginUri.Port;

        yield return siblingBuilder.Uri.GetLeftPart(UriPartial.Authority);
    }
}
