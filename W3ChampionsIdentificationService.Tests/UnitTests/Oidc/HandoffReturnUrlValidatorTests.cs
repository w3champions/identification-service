using NUnit.Framework;
using W3ChampionsIdentificationService.Oidc;

namespace W3ChampionsIdentificationService.Tests.UnitTests.Oidc;

[TestFixture]
public class HandoffReturnUrlValidatorTests
{
    [TestCase("https://identification-service.w3champions.com/connect/authorize?code=abc",
        true, TestName = "Prod_origin_with_path_is_allowed")]
    [TestCase("https://identification-service.test.w3champions.com/connect/authorize?state=x",
        true, TestName = "Test_origin_with_path_is_allowed")]
    [TestCase("https://identification-service.w3champions.com",
        true, TestName = "Prod_origin_only_is_allowed")]
    public void AllowedOrigins_ReturnTrue(string url, bool expected)
    {
        Assert.AreEqual(expected, HandoffReturnUrlValidator.IsAllowed(url));
    }

    [TestCase("http://identification-service.w3champions.com/connect/authorize",
        TestName = "HTTP_is_rejected")]
    [TestCase("https://w3champions.com/connect/authorize",
        TestName = "Apex_domain_is_rejected")]
    [TestCase("https://evil.com/redirect?to=identification-service.w3champions.com",
        TestName = "Open_redirect_attempt_is_rejected")]
    [TestCase("https://identification-service.w3champions.com.evil.com/connect/authorize",
        TestName = "Subdomain_spoof_is_rejected")]
    [TestCase("",   TestName = "Empty_string_is_rejected")]
    [TestCase(null, TestName = "Null_is_rejected")]
    [TestCase("not-a-url", TestName = "Non_url_is_rejected")]
    [TestCase("javascript:alert(1)", TestName = "Javascript_scheme_is_rejected")]
    public void DisallowedValues_ReturnFalse(string url)
    {
        Assert.IsFalse(HandoffReturnUrlValidator.IsAllowed(url));
    }
}
