using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class HandoffJwtValidatorTests
{
    private static readonly RSA _testRsa;
    private static readonly string _privatePem;
    private static readonly string _publicPem;
    private static readonly RSA _wrongRsa;
    private static readonly string _wrongPublicPem;

    static HandoffJwtValidatorTests()
    {
        _testRsa = RSA.Create(2048);
        _privatePem = _testRsa.ExportRSAPrivateKeyPem();
        _publicPem  = _testRsa.ExportSubjectPublicKeyInfoPem();
        _wrongRsa = RSA.Create(2048);
        _wrongPublicPem = _wrongRsa.ExportSubjectPublicKeyInfoPem();
    }

    private static string MakeJwt(string battleTag, TimeSpan? ttl = null, RSA signingKey = null)
    {
        var rsa = signingKey ?? _testRsa;
        var creds = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };
        var expires = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(10));
        var token = new JwtSecurityToken(
            claims: new[] { new Claim("battleTag", battleTag) },
            expires: expires,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Test]
    public void ValidToken_ReturnsBattleTag()
    {
        var validator = new HandoffJwtValidator(_publicPem);
        var jwt = MakeJwt("Modmoto#2809");
        var result = validator.ValidateAndExtractBattleTag(jwt);
        Assert.AreEqual("Modmoto#2809", result);
    }

    [Test]
    public void ExpiredToken_ThrowsHandoffValidationException()
    {
        var validator = new HandoffJwtValidator(_publicPem);
        var jwt = MakeJwt("Modmoto#2809", ttl: TimeSpan.FromSeconds(-1));
        Assert.Throws<HandoffValidationException>(() =>
            validator.ValidateAndExtractBattleTag(jwt));
    }

    [Test]
    public void ForgedToken_WrongKey_ThrowsHandoffValidationException()
    {
        var validator = new HandoffJwtValidator(_publicPem);
        var jwt = MakeJwt("Modmoto#2809", signingKey: _wrongRsa);
        Assert.Throws<HandoffValidationException>(() =>
            validator.ValidateAndExtractBattleTag(jwt));
    }

    [Test]
    public void TamperedToken_ThrowsHandoffValidationException()
    {
        var validator = new HandoffJwtValidator(_publicPem);
        var jwt = MakeJwt("Modmoto#2809");
        var parts = jwt.Split('.');
        var sig = parts[2].ToCharArray();
        sig[0] = sig[0] == 'A' ? 'B' : 'A';
        var tampered = $"{parts[0]}.{parts[1]}.{new string(sig)}";
        Assert.Throws<HandoffValidationException>(() =>
            validator.ValidateAndExtractBattleTag(tampered));
    }

    [Test]
    public void TokenMissingBattleTagClaim_ThrowsHandoffValidationException()
    {
        var creds = new SigningCredentials(
            new RsaSecurityKey(_testRsa), SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };
        var token = new JwtSecurityToken(
            claims: new[] { new Claim("sub", "someuser") },
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        var validator = new HandoffJwtValidator(_publicPem);
        Assert.Throws<HandoffValidationException>(() =>
            validator.ValidateAndExtractBattleTag(jwt));
    }
}
