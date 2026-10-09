using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests.UnitTests.W3CAuthentication;

[TestFixture]
public class HttpRequestExtensionsTests
{
    private static string GetBearerToken(string authorization)
    {
        var httpContext = new DefaultHttpContext();
        if (authorization != null)
        {
            httpContext.Request.Headers.Authorization = authorization;
        }
        return httpContext.Request.GetBearerToken();
    }

    [TestCase("Bearer abc.def.ghi", ExpectedResult = "abc.def.ghi")]
    [TestCase("bearer abc", ExpectedResult = "abc")]
    [TestCase("BEARER abc", ExpectedResult = "abc")]
    [TestCase("  bEaReR   abc  ", ExpectedResult = "abc")]
    [TestCase("Bearer\tabc", ExpectedResult = null)]
    [TestCase("Bearer ", ExpectedResult = null)]
    [TestCase("Bearer    ", ExpectedResult = null)]
    [TestCase("Bearer", ExpectedResult = null)]
    [TestCase("Bearerabc", ExpectedResult = null)]
    [TestCase("Basic dXNlcjpwYXNz", ExpectedResult = null)]
    [TestCase("", ExpectedResult = null)]
    [TestCase(null, ExpectedResult = null)]
    public string GetBearerToken_ParsesHeader(string authorization) => GetBearerToken(authorization);

    [Test]
    public void GetBearerToken_MultipleAuthorizationHeaders_FailsClosed()
    {
        // StringValues.ToString() joins multiple values with a comma ("Bearer a,Bearer b"), so the token
        // would be "a,Bearer b". Neither header is picked; the mangled value fails JWT validation, so the
        // endpoints answer 401 instead of accepting either token.
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = new StringValues(new[] { "Bearer a", "Bearer b" });

        Assert.AreEqual("a,Bearer b", httpContext.Request.GetBearerToken());
    }

    [Test]
    public void GetJwt_BearerHeaderWins_OverQuery()
    {
        var request = new DefaultHttpContext().Request;
        request.Headers.Authorization = "Bearer header-token";

        Assert.AreEqual("header-token", request.GetJwt("query-token"));
    }

    [TestCase("Basic dXNlcjpwYXNz")]
    [TestCase("Bearer ")]
    [TestCase("Bearer")]
    [TestCase("")]
    [TestCase(null)]
    public void GetJwt_MalformedOrMissingHeader_FallsBackToQuery(string authorization)
    {
        var request = new DefaultHttpContext().Request;
        if (authorization != null)
        {
            request.Headers.Authorization = authorization;
        }

        Assert.AreEqual("query-token", request.GetJwt("query-token"));
    }

    [Test]
    public void GetJwt_NoHeaderNoQuery_ReturnsNull()
    {
        Assert.IsNull(new DefaultHttpContext().Request.GetJwt(null));
    }
}
