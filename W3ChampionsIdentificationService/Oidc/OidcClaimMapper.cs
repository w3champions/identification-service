using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

    private static readonly Regex SafeLocalPart = new("^[a-z0-9._-]+$", RegexOptions.Compiled);

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

        var candidate = battleTag.Replace('#', '-').ToLowerInvariant();
        // Normal ASCII battletags keep a readable local-part (e.g. "modmoto-2809").
        // Anything with non-ASCII / unsafe chars falls back to a deterministic,
        // collision-free ASCII local-part derived from a hash of the ORIGINAL battletag,
        // so the email is always RFC-5321-legal and unique per identity. (The human-
        // readable identity is the `name` claim = full battletag, not this email.)
        var localPart = SafeLocalPart.IsMatch(candidate)
            ? candidate
            : "u-" + Sha256Hex(battleTag);

        return $"{localPart}@{SyntheticEmailDomain}";
    }

    private static string Sha256Hex(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(32);
        for (int i = 0; i < 16; i++)            // 16 bytes -> 128 bits -> 32 hex chars
            sb.Append(hash[i].ToString("x2"));
        return sb.ToString();
    }
}
