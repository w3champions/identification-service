using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Twitch;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests.UnitTests.W3CAuthentication;

/// <summary>
/// GET /api/oauth/user-info takes the token from "Authorization: Bearer" and only falls
/// back to the deprecated ?jwt= query parameter when no usable Bearer header is present.
/// </summary>
[TestFixture]
public class UserInfoBearerHeaderTests
{
    private string _originalJwtPublicKey;
    private string _validJwt;

    [SetUp]
    public void SetUp()
    {
        _originalJwtPublicKey = Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY");
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY", TestJwtKeys.PublicKey);
        _validJwt = W3CUserAuthentication.Create("TestPlayer#9999", TestJwtKeys.PrivateKey, new List<string> { "Permissions", "Moderation" }).JWT;
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY", _originalJwtPublicKey);
    }

    private static IActionResult Call(string authorization, string queryJwt) =>
        CallWithHeaders(authorization == null ? StringValues.Empty : new StringValues(authorization), queryJwt);

    private static IActionResult CallWithHeaders(StringValues authorization, string queryJwt)
    {
        var controller = new AuthorizationController(
            Mock.Of<IBlizzardAuthenticationService>(),
            Mock.Of<ITwitchAuthenticationService>(),
            Mock.Of<IMicrosoftAuthenticationService>(),
            Mock.Of<IUsersRepository>(),
            Mock.Of<IRolesRepository>(),
            Mock.Of<IPermissionsRepository>(),
            Mock.Of<IMicrosoftIdentityRepository>());

        var httpContext = new DefaultHttpContext();
        if (!StringValues.IsNullOrEmpty(authorization))
        {
            httpContext.Request.Headers.Authorization = authorization;
        }
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Model binding is bypassed in a unit test, so pass the query value directly.
        return controller.GetUserInfo(queryJwt);
    }

    private static void AssertUnauthorized(IActionResult result)
    {
        var unauthorized = result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorized, "Expected 401, got {0}", result?.GetType().Name);
        Assert.AreEqual("Sorry Hackerboi", unauthorized.Value);
    }

    private static void AssertOkWithUser(IActionResult result)
    {
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok, "Expected 200 OK, got {0}", result?.GetType().Name);
        var body = ok.Value as W3CUserAuthentication;
        Assert.IsNotNull(body);
        // Same response shape as the legacy query path (see ExistingOauthEndpointsRegressionTests).
        Assert.AreEqual("TestPlayer#9999", body.BattleTag);
        Assert.AreEqual("TestPlayer", body.Name);
        Assert.IsTrue(body.IsAdmin);
    }

    [Test]
    public void HeaderOnly_ReturnsOk() => AssertOkWithUser(Call($"Bearer {_validJwt}", null));

    [Test]
    public void HeaderScheme_IsCaseInsensitive_AndWhitespaceTrimmed() =>
        AssertOkWithUser(Call($"  bEaReR   {_validJwt}  ", null));

    [Test]
    public void QueryOnly_LegacyFallback_ReturnsOk() => AssertOkWithUser(Call(null, _validJwt));

    [Test]
    public void ValidHeader_GarbageQuery_HeaderWins() =>
        AssertOkWithUser(Call($"Bearer {_validJwt}", "garbage"));

    [Test]
    public void InvalidHeader_ValidQuery_HeaderWins_Unauthorized() =>
        AssertUnauthorized(Call("Bearer garbage", _validJwt));

    [TestCase("Basic dXNlcjpwYXNz")]
    [TestCase("Bearer ")]
    [TestCase("Bearer")]
    [TestCase("")]
    public void MalformedHeader_NoQuery_Unauthorized(string header) =>
        AssertUnauthorized(Call(header, null));

    [Test]
    public void MalformedHeader_ValidQuery_FallsBackToQuery() =>
        AssertOkWithUser(Call("Basic dXNlcjpwYXNz", _validJwt));

    [TestCase(null)]
    [TestCase("")]
    public void NoToken_Unauthorized(string query) => AssertUnauthorized(Call(null, query));

    [Test]
    public void MultipleAuthorizationHeaders_FailClosed_Unauthorized() =>
        AssertUnauthorized(CallWithHeaders(new StringValues(new[] { $"Bearer {_validJwt}", $"Bearer {_validJwt}" }), null));

    [Test]
    public void InvalidTokenInHeader_Unauthorized() => AssertUnauthorized(Call("Bearer not.a.jwt", null));
}
