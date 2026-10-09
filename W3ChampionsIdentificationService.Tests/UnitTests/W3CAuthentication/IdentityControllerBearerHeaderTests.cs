using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Identity;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService.Tests.UnitTests.W3CAuthentication;

/// <summary>
/// The api/identity endpoints take the token from "Authorization: Bearer" and only fall back to the
/// deprecated ?jwt= query parameter when no usable Bearer header is present.
/// </summary>
[TestFixture]
public class IdentityControllerBearerHeaderTests
{
    private const string BattleTag = "TestPlayer#9999";
    private const string MicrosoftSub = "microsoft-sub";

    private string _originalJwtPublicKey;
    private string _validJwt;
    private Mock<IMicrosoftAuthenticationService> _microsoftAuthenticationService;
    private Mock<IMicrosoftIdentityRepository> _microsoftIdentityRepository;

    [SetUp]
    public void SetUp()
    {
        _originalJwtPublicKey = Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY");
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY", TestJwtKeys.PublicKey);
        _validJwt = W3CUserAuthentication.Create(BattleTag, TestJwtKeys.PrivateKey, new List<string>()).JWT;

        _microsoftAuthenticationService = new Mock<IMicrosoftAuthenticationService>();
        _microsoftAuthenticationService.Setup(s => s.GetIdToken("code", "redirect")).ReturnsAsync("id-token");
        _microsoftAuthenticationService.Setup(s => s.GetUser("id-token")).ReturnsAsync(new MicrosoftUser { sub = MicrosoftSub });

        _microsoftIdentityRepository = new Mock<IMicrosoftIdentityRepository>();
        _microsoftIdentityRepository.Setup(r => r.GetIdentityByBattleTag(BattleTag))
            .ReturnsAsync(new MicrosoftIdentity { Id = MicrosoftSub, battleTag = BattleTag });
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY", _originalJwtPublicKey);
    }

    private IdentityController CreateController(string authorization)
    {
        var controller = new IdentityController(
            Mock.Of<IBlizzardAuthenticationService>(),
            _microsoftAuthenticationService.Object,
            _microsoftIdentityRepository.Object);

        var httpContext = new DefaultHttpContext();
        if (authorization != null)
        {
            httpContext.Request.Headers.Authorization = authorization;
        }
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    // Model binding is bypassed in a unit test, so the query value is passed directly.
    private Task<IActionResult> GetLinked(string authorization, string queryJwt) =>
        CreateController(authorization).GetMicrosoftIdentity(queryJwt);

    private Task<IActionResult> Link(string authorization, string queryJwt) =>
        CreateController(authorization).LinkMicrosoftIdentity(queryJwt, "code", "redirect");

    private static void AssertUnauthorized(IActionResult result)
    {
        var unauthorized = result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorized, "Expected 401, got {0}", result?.GetType().Name);
        Assert.AreEqual("Sorry Hackerboi", unauthorized.Value);
    }

    private void AssertLinkedLookup(IActionResult result)
    {
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok, "Expected 200 OK, got {0}", result?.GetType().Name);
        Assert.AreEqual(true, ok.Value);
        _microsoftIdentityRepository.Verify(r => r.GetIdentityByBattleTag(BattleTag), Times.Once);
    }

    private void AssertLinked(IActionResult result)
    {
        Assert.IsInstanceOf<OkResult>(result);
        _microsoftIdentityRepository.Verify(r => r.LinkBattleTag(MicrosoftSub, BattleTag), Times.Once);
    }

    private void AssertNotLinked(IActionResult result)
    {
        AssertUnauthorized(result);
        _microsoftAuthenticationService.Verify(s => s.GetIdToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _microsoftIdentityRepository.Verify(r => r.LinkBattleTag(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Linked_HeaderOnly_ReturnsOk() => AssertLinkedLookup(await GetLinked($"Bearer {_validJwt}", null));

    [Test]
    public async Task Linked_QueryOnly_LegacyFallback_ReturnsOk() => AssertLinkedLookup(await GetLinked(null, _validJwt));

    [Test]
    public async Task Linked_ValidHeader_GarbageQuery_HeaderWins() =>
        AssertLinkedLookup(await GetLinked($"Bearer {_validJwt}", "garbage"));

    [Test]
    public async Task Linked_InvalidHeader_ValidQuery_HeaderWins_Unauthorized() =>
        AssertUnauthorized(await GetLinked("Bearer garbage", _validJwt));

    [TestCase(null)]
    [TestCase("")]
    public async Task Linked_NoToken_Unauthorized(string query) => AssertUnauthorized(await GetLinked(null, query));

    [Test]
    public async Task Link_HeaderOnly_LinksIdentity() => AssertLinked(await Link($"Bearer {_validJwt}", null));

    [Test]
    public async Task Link_QueryOnly_LegacyFallback_LinksIdentity() => AssertLinked(await Link(null, _validJwt));

    [Test]
    public async Task Link_InvalidHeader_ValidQuery_HeaderWins_Unauthorized() =>
        AssertNotLinked(await Link("Bearer garbage", _validJwt));

    [TestCase(null)]
    [TestCase("")]
    public async Task Link_NoToken_Unauthorized(string query) => AssertNotLinked(await Link(null, query));
}
