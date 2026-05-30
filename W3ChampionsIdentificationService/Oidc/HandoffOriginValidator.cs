using System;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// Login-CSRF defense for the handoff POST.
///
/// The legitimate handoff is auto-submitted from the website's /sso-continue page, so its
/// Origin header is the website origin (= the origin of WEBSITE_LOGIN_URL). A cross-site
/// attacker auto-submitting a forged JWT carries a different Origin, so rejecting POSTs whose
/// Origin does not exactly match the website origin prevents session fixation: an attacker can
/// no longer fixate the victim's browser with an IdP session for the attacker's BattleTag.
/// </summary>
public static class HandoffOriginValidator
{
    /// <summary>
    /// Returns true only if <paramref name="requestOrigin"/> is non-empty and its scheme+host+port
    /// exactly match the origin of <paramref name="websiteLoginUrl"/> (case-insensitive).
    /// Returns false on null/empty/mismatch, or when either value is unparseable.
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
        // be folded onto the expected authority by GetLeftPart below.
        if (originUri.AbsolutePath != "/" || !string.IsNullOrEmpty(originUri.Query) || !string.IsNullOrEmpty(originUri.Fragment))
            return false;

        var expectedOrigin = loginUri.GetLeftPart(UriPartial.Authority);
        var actualOrigin = originUri.GetLeftPart(UriPartial.Authority);

        return string.Equals(actualOrigin, expectedOrigin, StringComparison.OrdinalIgnoreCase);
    }
}
