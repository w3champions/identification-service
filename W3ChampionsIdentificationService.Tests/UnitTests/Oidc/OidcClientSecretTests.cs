using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc.Cli;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class OidcClientSecretTests
{
    [Test]
    public void GeneratedSecret_Is32BytesBase64_Length44()
    {
        Assert.AreEqual(44, OidcClientCli.GenerateSecret().Length);
    }

    [Test]
    public void TwoGeneratedSecrets_AreNotEqual()
    {
        Assert.AreNotEqual(OidcClientCli.GenerateSecret(), OidcClientCli.GenerateSecret());
    }

    [TestCase("https://feedback.w3champions.com/api/auth/oauth2/callback/custom-oidc", true)]
    [TestCase("https://example.com/callback", true)]
    [TestCase("http://example.com/callback",  false, TestName = "HTTP_rejected")]
    [TestCase("",                              false, TestName = "Empty_rejected")]
    [TestCase(null,                            false, TestName = "Null_rejected")]
    [TestCase("not-a-url",                     false, TestName = "Relative_rejected")]
    public void RedirectUri_HttpsGuard(string uri, bool shouldPass)
    {
        Assert.AreEqual(shouldPass, OidcClientCli.IsValidHttpsRedirectUri(uri));
    }
}
