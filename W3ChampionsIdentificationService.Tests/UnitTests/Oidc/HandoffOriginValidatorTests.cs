using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class HandoffOriginValidatorTests
{
    // The website login URL carries a path (/sso-continue); only its ORIGIN must match the
    // request Origin header (which is a bare scheme+host+port). The w3champions site is reachable
    // on both the apex and www, so both are accepted regardless of which is configured.
    private const string ApexLoginUrl = "https://w3champions.com/sso-continue";
    private const string WwwLoginUrl = "https://www.w3champions.com/sso-continue";
    private const string TestLoginUrl = "https://localhost:3000/sso-continue";

    [TestCase("https://w3champions.com", ApexLoginUrl,
        TestName = "Apex_config_accepts_apex_origin")]
    [TestCase("https://www.w3champions.com", ApexLoginUrl,
        TestName = "Apex_config_accepts_www_sibling_origin")]
    [TestCase("https://www.w3champions.com", WwwLoginUrl,
        TestName = "Www_config_accepts_www_origin")]
    [TestCase("https://w3champions.com", WwwLoginUrl,
        TestName = "Www_config_accepts_apex_sibling_origin")]
    [TestCase("https://W3CHAMPIONS.COM", ApexLoginUrl,
        TestName = "Apex_match_is_case_insensitive")]
    [TestCase("https://WWW.W3CHAMPIONS.COM", ApexLoginUrl,
        TestName = "Www_sibling_match_is_case_insensitive")]
    [TestCase("https://localhost:3000", TestLoginUrl,
        TestName = "Exact_test_website_origin_is_allowed")]
    [TestCase("https://www.localhost:3000", TestLoginUrl,
        TestName = "Test_config_accepts_www_sibling_with_port")]
    public void AllowedOrigin_ReturnsTrue(string requestOrigin, string websiteLoginUrl)
    {
        Assert.IsTrue(HandoffOriginValidator.IsAllowedOrigin(requestOrigin, websiteLoginUrl));
    }

    [TestCase("https://evil.com", ApexLoginUrl,
        TestName = "Different_origin_is_rejected")]
    [TestCase("https://www.evil.com", ApexLoginUrl,
        TestName = "Www_of_a_different_origin_is_rejected")]
    [TestCase("", ApexLoginUrl,
        TestName = "Empty_origin_is_rejected")]
    [TestCase(null, ApexLoginUrl,
        TestName = "Null_origin_is_rejected")]
    [TestCase("https://w3champions.com/evil", ApexLoginUrl,
        TestName = "Origin_with_extra_path_is_rejected")]
    [TestCase("http://w3champions.com", ApexLoginUrl,
        TestName = "Http_vs_https_mismatch_is_rejected")]
    [TestCase("https://w3champions.com:8443", ApexLoginUrl,
        TestName = "Different_port_is_rejected")]
    [TestCase("https://w3champions.com.evil.com", ApexLoginUrl,
        TestName = "Lookalike_suffix_origin_is_rejected")]
    [TestCase("https://www.w3champions.com.evil.com", ApexLoginUrl,
        TestName = "Www_lookalike_suffix_origin_is_rejected")]
    [TestCase("not-an-origin", ApexLoginUrl,
        TestName = "Unparseable_origin_is_rejected")]
    public void DisallowedOrigin_ReturnsFalse(string requestOrigin, string websiteLoginUrl)
    {
        Assert.IsFalse(HandoffOriginValidator.IsAllowedOrigin(requestOrigin, websiteLoginUrl));
    }

    [Test]
    public void EmptyWebsiteLoginUrl_RejectsEvenAMatchingLookingOrigin()
    {
        Assert.IsFalse(HandoffOriginValidator.IsAllowedOrigin("https://w3champions.com", ""));
    }
}
