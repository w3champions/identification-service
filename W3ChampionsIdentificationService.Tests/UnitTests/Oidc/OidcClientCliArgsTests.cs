using System;
using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc.Cli;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class OidcClientCliArgsTests
{
    [Test]
    public void ParseRegisterArgs_ValidArgs_ReturnsExpectedRecord()
    {
        var result = OidcClientCli.ParseRegisterArgs(
        [
            "register-client",
            "--client-id", "quackback",
            "--redirect-uri", "https://feedback.w3champions.com/callback",
            "--scopes", "openid,email"
        ]);

        Assert.AreEqual("quackback", result.ClientId);
        Assert.AreEqual("https://feedback.w3champions.com/callback", result.RedirectUri);
        Assert.AreEqual(new[] { "openid", "email" }, result.Scopes);
        Assert.IsFalse(result.Update);
    }

    [Test]
    public void ParseRegisterArgs_NoScopes_DefaultsToOpenidProfileEmail()
    {
        var result = OidcClientCli.ParseRegisterArgs(
        [
            "register-client",
            "--client-id", "quackback",
            "--redirect-uri", "https://feedback.w3champions.com/callback"
        ]);

        Assert.AreEqual(new[] { "openid", "profile", "email" }, result.Scopes);
    }

    [Test]
    public void ParseRegisterArgs_UpdateFlag_SetsUpdateTrue()
    {
        var result = OidcClientCli.ParseRegisterArgs(
        [
            "register-client",
            "--client-id", "quackback",
            "--redirect-uri", "https://feedback.w3champions.com/callback",
            "--update"
        ]);

        Assert.IsTrue(result.Update);
    }

    [Test]
    public void ParseRegisterArgs_TrailingValuelessFlag_ThrowsArgumentException()
    {
        // --client-id is the last token: must throw cleanly, not IndexOutOfRangeException.
        Assert.Throws<ArgumentException>(() =>
            OidcClientCli.ParseRegisterArgs(["register-client", "--client-id"]));
    }

    [Test]
    public void ParseRegisterArgs_MissingClientId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            OidcClientCli.ParseRegisterArgs(
            [
                "register-client",
                "--redirect-uri", "https://feedback.w3champions.com/callback"
            ]));
    }

    [Test]
    public void ParseRegisterArgs_MissingRedirectUri_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            OidcClientCli.ParseRegisterArgs(["register-client", "--client-id", "quackback"]));
    }

    [Test]
    public void ParseRegisterArgs_HttpRedirectUri_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            OidcClientCli.ParseRegisterArgs(
            [
                "register-client",
                "--client-id", "quackback",
                "--redirect-uri", "http://feedback.w3champions.com/callback"
            ]));
    }
}
