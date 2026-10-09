using NUnit.Framework;
using W3ChampionsIdentificationService.W3CAuthentication;
using W3ChampionsIdentificationService.RolesAndPermissions;
using System.Collections.Generic;

namespace W3ChampionsIdentificationService.Tests.UnitTests;

[TestFixture]
public class JWTTests
{
    private readonly string _privateKey = TestJwtKeys.PrivateKey;
    private readonly string _publicKey = TestJwtKeys.PublicKey;

    private readonly string _wrongPublicKey =
        "MIIBCgKCAQEAwpRjp/3Xh8MtwsYxI6CqnxtHlEclWS5kQ1gbhR6WUUhm+Sizl3E5rgdVo6SxuUEGphF2Vih1NYGEbL4fWNqY+jqhtZb8AA/1qLSOVl9BdDw1nM0upuY0IPl9qPAnrveDjxR8WGdTywW6Hjf30gTkxCbgyISMoEOaKycmWlk+kS3GoonOLGLnGJKT4QniIC5LxOBRyjRYXyhq0Jd8r/bhLeyKVbCSz5mWjQhNe90lV/8oc34GvemlPe1//jufL3ZlJ3rK34axnEgqFEVebpGgYEV+5DUO/3qAWOwtbxiuqqQE7EywaotPFPEhenHxBBKlcRuycKQnBq94w068E2TVbQIDAQAB";

    [Test]
    public void TestPropertyMapping()
    {
        List<string> permissions = new List<string> { nameof(EPermission.Permissions), nameof(EPermission.Moderation), nameof(EPermission.Tournaments) };
        var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, permissions);

        Assert.IsTrue(userAuthentication.IsAdmin);
        Assert.AreEqual("modmoto", userAuthentication.Name);
        Assert.AreEqual("modmoto#2809", userAuthentication.BattleTag);
        Assert.AreEqual(3, userAuthentication.Permissions.Count);
        Assert.IsTrue(userAuthentication.Permissions.Contains(nameof(EPermission.Permissions)));
        Assert.IsTrue(userAuthentication.Permissions.Contains(nameof(EPermission.Moderation)));
        Assert.IsTrue(userAuthentication.Permissions.Contains(nameof(EPermission.Tournaments)));
    }

    [Test]
    public void TestJwtTokenGeneration()
    {
        List<string> permissions = new List<string> { "Permissions", "Moderation", "Tournaments" };
        var userAuthentication = W3CUserAuthentication.Create("notsuperadmin#2809", _privateKey, permissions);

        Assert.Greater(userAuthentication.JWT.Length, 800);
        // Assert.AreEqual(3, userAuthentication.Permissions.Count);
    }

    [Test]
    public void JwtCanBeValidated()
    {
        List<string> permissions = new List<string> { "Permissions", "Moderation", "Tournaments" };
        var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, permissions);

        var decode = W3CUserAuthentication.FromJWT(userAuthentication.JWT, _publicKey);

        Assert.AreEqual("modmoto#2809", decode.BattleTag);
        Assert.AreEqual(true, decode.IsAdmin);
        Assert.AreEqual("modmoto", decode.Name);
        Assert.AreEqual(userAuthentication.JWT, decode.JWT);
    }

    [Test]
    public void InvalidSecretThrows()
    {
        List<string> permissions = new List<string> { "Permissions", "Moderation", "Tournaments" };
        var userAuthentication = W3CUserAuthentication.Create("modmoto#2809", _privateKey, permissions);

        Assert.IsNull(W3CUserAuthentication.FromJWT(userAuthentication.JWT, _wrongPublicKey));
    }

    // run this test to generate secrets and copy the values from the tuple.
    /*[Test]
    public void SecretGeneration()
    {
        var tuple = W3CUserAuthentication.CreatePublicAndPrivateKey();

        Assert.IsNotNull(tuple.Item1); // private key
        Assert.IsNotNull(tuple.Item2); // public key
    }*/
}
