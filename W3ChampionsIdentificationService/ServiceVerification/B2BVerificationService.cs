using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace W3ChampionsIdentificationService.ServiceVerification;

public class B2BVerificationService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;

    public B2BVerificationService(string secret, string issuer)
    {
        _issuer = issuer;
        _key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(secret));
        _audience = "w3c-identification-service";
    }

    public bool Verify(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = _key
        };

        try
        {
            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        } catch
        {
            return false;
        }
    }
}