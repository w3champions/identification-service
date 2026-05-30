using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class HandoffOriginValidatorTests
{
    // The website login URL carries a path (/sso-continue); only its ORIGIN must match the
    // request Origin header (which is a bare scheme+host+port).
    private const string ProdLoginUrl = "https://www.w3champions.com/sso-continue";
    private const string TestLoginUrl = "https://localhost:3000/sso-continue";

    [TestCase("https://www.w3champions.com", ProdLoginUrl,
        TestName = "Exact_prod_website_origin_is_allowed")]
    [TestCase("https://localhost:3000", TestLoginUrl,
        TestName = "Exact_test_website_origin_is_allowed")]
    [TestCase("https://WWW.W3CHAMPIONS.COM", ProdLoginUrl,
        TestName = "Origin_match_is_case_insensitive")]
    public void AllowedOrigin_ReturnsTrue(string requestOrigin, string websiteLoginUrl)
    {
        Assert.IsTrue(HandoffOriginValidator.IsAllowedOrigin(requestOrigin, websiteLoginUrl));
    }

    [TestCase("https://evil.com", ProdLoginUrl,
        TestName = "Different_origin_is_rejected")]
    [TestCase("", ProdLoginUrl,
        TestName = "Empty_origin_is_rejected")]
    [TestCase(null, ProdLoginUrl,
        TestName = "Null_origin_is_rejected")]
    [TestCase("https://www.w3champions.com/evil", ProdLoginUrl,
        TestName = "Origin_with_extra_path_is_rejected")]
    [TestCase("http://www.w3champions.com", ProdLoginUrl,
        TestName = "Http_vs_https_mismatch_is_rejected")]
    [TestCase("https://www.w3champions.com:8443", ProdLoginUrl,
        TestName = "Different_port_is_rejected")]
    [TestCase("https://www.w3champions.com.evil.com", ProdLoginUrl,
        TestName = "Lookalike_suffix_origin_is_rejected")]
    [TestCase("not-an-origin", ProdLoginUrl,
        TestName = "Unparseable_origin_is_rejected")]
    public void DisallowedOrigin_ReturnsFalse(string requestOrigin, string websiteLoginUrl)
    {
        Assert.IsFalse(HandoffOriginValidator.IsAllowedOrigin(requestOrigin, websiteLoginUrl));
    }

    [Test]
    public void EmptyWebsiteLoginUrl_RejectsEvenAMatchingLookingOrigin()
    {
        Assert.IsFalse(HandoffOriginValidator.IsAllowedOrigin("https://www.w3champions.com", ""));
    }
}
