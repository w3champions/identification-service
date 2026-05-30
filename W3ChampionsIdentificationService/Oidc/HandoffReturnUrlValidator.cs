using System;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// Validates the `return` parameter in a handoff POST.
/// The only permitted origins are the prod and test identification-service hosts
/// (exact origin match, no subdomain wildcard).
/// </summary>
public static class HandoffReturnUrlValidator
{
    private static readonly string[] _allowed =
    [
        "https://identification-service.w3champions.com",
        "https://identification-service.test.w3champions.com",
    ];

    /// <summary>
    /// The origins that are permitted as handoff return-URL origins.
    /// Exposed so Startup can check that <c>OIDC_ISSUER</c> is among them and warn if not.
    /// </summary>
    public static IReadOnlyList<string> AllowedOrigins => _allowed;

    /// <summary>
    /// Returns true if <paramref name="returnUrl"/> is an absolute HTTPS URL
    /// whose scheme+host+port exactly match one of the allowed origins.
    /// </summary>
    public static bool IsAllowed(string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
            return false;
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            return false;
        if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            return false;

        var origin = uri.GetLeftPart(UriPartial.Authority);
        foreach (var allowed in _allowed)
        {
            if (string.Equals(origin, allowed, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
