using System;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// Maps a w3c battleTag to the OIDC claims released to a relying-party client.
/// </summary>
public static class OidcClaimMapper
{
    // Generic, non-deliverable domain for identities with no real email.
    // The reserved ".invalid" TLD (RFC 6761) guarantees the address is never
    // routable — we are NOT minting real/valid emails, only a unique, stable
    // identifier to satisfy OIDC clients that require an email claim. This is
    // a GENERIC IdP behaviour (no relying-party name appears here).
    private const string SyntheticEmailDomain = "w3champions.invalid";

    /// <summary>
    /// Derives a synthetic, non-deliverable email for a w3c identity that has no
    /// real email, used when an OIDC client requests the `email` scope. Replaces
    /// '#' with '-' so the local-part is RFC-5321 legal.
    /// Example: "Modmoto#2809" → "modmoto-2809@w3champions.invalid"
    /// </summary>
    public static string BattleTagToSyntheticEmail(string battleTag)
    {
        if (string.IsNullOrEmpty(battleTag))
            throw new ArgumentException("battleTag must not be empty", nameof(battleTag));
        var localPart = battleTag.Replace('#', '-').ToLowerInvariant();
        return $"{localPart}@{SyntheticEmailDomain}";
    }
}
