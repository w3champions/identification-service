using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace W3ChampionsIdentificationService.Oidc;

/// <summary>
/// Validates a w3c JWT (RS256, no iss/aud, with exp) presented at the handoff endpoint.
/// Uses the same JWT_PUBLIC_KEY the rest of the service uses today — SEPARATE from the
/// OIDC signing key.
/// </summary>
public class HandoffJwtValidator
{
    private readonly RsaSecurityKey _publicKey;

    public HandoffJwtValidator(string rsaPemPublicKey)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(rsaPemPublicKey);
        _publicKey = new RsaSecurityKey(rsa);
    }

    /// <summary>
    /// Returns the battleTag extracted from the JWT, or throws <see cref="HandoffValidationException"/>
    /// if the token is invalid, expired, or forged.
    /// </summary>
    public string ValidateAndExtractBattleTag(string jwt)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,        // enforce exp — must NOT be ValidateLifetime=false
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _publicKey,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(jwt, parameters, out _);
            var battleTag = principal.Claims.FirstOrDefault(c => c.Type == "battleTag")?.Value;
            if (string.IsNullOrEmpty(battleTag))
                throw new HandoffValidationException("battleTag claim missing");
            return battleTag;
        }
        catch (SecurityTokenExpiredException ex)
        {
            throw new HandoffValidationException("Token has expired", ex);
        }
        catch (SecurityTokenException ex)
        {
            throw new HandoffValidationException("Token validation failed", ex);
        }
    }
}

public class HandoffValidationException : Exception
{
    public HandoffValidationException(string message, Exception inner = null)
        : base(message, inner) { }
}
