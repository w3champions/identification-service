using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.W3CAuthentication;
using W3ChampionsIdentificationService.W3CAuthentication.Contracts;
using W3ChampionsIdentificationService.WebApi.ActionFilters;
using W3ChampionsIdentificationService.WebApi.ExceptionFilters;

namespace W3ChampionsIdentificationService.Tests.UnitTests.WebApi.ActionFilters;

/// <summary>
/// [HasPermissionsPermission] takes the token from "Authorization: Bearer" and only falls back to the
/// deprecated ?authorization= query parameter when no usable Bearer header is present.
/// </summary>
[TestFixture]
public class HasPermissionsPermissionFilterTests
{
    private const string AdminToken = "admin-token";
    private const string NoPermissionToken = "no-permission-token";
    private const string AdminBattleTag = "Admin#1234";
    private const string PlainBattleTag = "Plain#5678";

    private Mock<IW3CAuthenticationService> _authService;
    private Mock<IPermissionsRepository> _permissionsRepository;
    private HasPermissionsPermissionFilter _filter;

    [SetUp]
    public void SetUp()
    {
        // Unknown tokens fall through to Moq's default (null), i.e. an invalid token.
        _authService = new Mock<IW3CAuthenticationService>();
        _authService.Setup(s => s.GetUserByToken(AdminToken)).Returns(new W3CUserAuthentication { BattleTag = AdminBattleTag });
        _authService.Setup(s => s.GetUserByToken(NoPermissionToken)).Returns(new W3CUserAuthentication { BattleTag = PlainBattleTag });

        _permissionsRepository = new Mock<IPermissionsRepository>();
        _permissionsRepository.Setup(r => r.GetPermissionsForAdmin(AdminBattleTag))
            .ReturnsAsync([nameof(EPermission.Permissions)]);
        _permissionsRepository.Setup(r => r.GetPermissionsForAdmin(PlainBattleTag))
            .ReturnsAsync([nameof(EPermission.Moderation)]);

        _filter = new HasPermissionsPermissionFilter(_authService.Object, _permissionsRepository.Object);
    }

    private async Task<(ActionExecutingContext Context, bool NextInvoked)> Run(string authorizationHeader, string queryString)
    {
        var httpContext = new DefaultHttpContext();
        if (authorizationHeader != null)
        {
            httpContext.Request.Headers.Authorization = authorizationHeader;
        }
        if (queryString != null)
        {
            httpContext.Request.QueryString = new QueryString(queryString);
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), controller: null);

        var nextInvoked = false;
        Task<ActionExecutedContext> Next()
        {
            nextInvoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null));
        }

        await _filter.OnActionExecutionAsync(context, Next);
        return (context, nextInvoked);
    }

    private static void AssertAuthorized(ActionExecutingContext context, bool nextInvoked)
    {
        Assert.IsTrue(nextInvoked, "Expected the action to run");
        Assert.IsNull(context.Result, "No result may be set once the action ran");
        Assert.AreEqual(AdminBattleTag, context.ActionArguments["battleTag"]);
    }

    private static void AssertUnauthorized(ActionExecutingContext context, bool nextInvoked)
    {
        Assert.IsFalse(nextInvoked, "The action must not run");
        Assert.IsFalse(context.ActionArguments.ContainsKey("battleTag"));
        var unauthorized = context.Result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorized, "Expected 401, got {0}", context.Result?.GetType().Name);
        Assert.AreEqual("Permissing missing.", (unauthorized.Value as ErrorResult)?.Error);
    }

    [Test]
    public async Task HeaderWithPermission_InvokesActionAndInjectsBattleTag()
    {
        var (context, nextInvoked) = await Run($"Bearer {AdminToken}", null);
        AssertAuthorized(context, nextInvoked);
    }

    [Test]
    public async Task HeaderWins_OverQuery()
    {
        var (context, nextInvoked) = await Run($"Bearer {AdminToken}", $"?authorization={NoPermissionToken}");
        AssertAuthorized(context, nextInvoked);
        _authService.Verify(s => s.GetUserByToken(NoPermissionToken), Times.Never);
    }

    [Test]
    public async Task InvalidHeader_ValidQuery_HeaderWins_Unauthorized()
    {
        var (context, nextInvoked) = await Run("Bearer garbage", $"?authorization={AdminToken}");
        AssertUnauthorized(context, nextInvoked);
    }

    [Test]
    public async Task QueryOnly_LegacyFallback_InvokesAction()
    {
        var (context, nextInvoked) = await Run(null, $"?limit=10&authorization={AdminToken}");
        AssertAuthorized(context, nextInvoked);
    }

    [Test]
    public async Task MalformedHeader_ValidQuery_FallsBackToQuery()
    {
        var (context, nextInvoked) = await Run("Basic dXNlcjpwYXNz", $"?authorization={AdminToken}");
        AssertAuthorized(context, nextInvoked);
    }

    [TestCase(null, null)]
    [TestCase(null, "?authorization=")]
    [TestCase("Bearer ", null)]
    [TestCase(null, "?limit=10")]
    public async Task MissingToken_Unauthorized(string header, string query)
    {
        var (context, nextInvoked) = await Run(header, query);
        AssertUnauthorized(context, nextInvoked);
        _authService.Verify(s => s.GetUserByToken(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ValidTokenWithoutPermissionsPermission_Unauthorized()
    {
        var (context, nextInvoked) = await Run($"Bearer {NoPermissionToken}", null);
        AssertUnauthorized(context, nextInvoked);
    }

    [Test]
    public async Task InvalidToken_Unauthorized()
    {
        var (context, nextInvoked) = await Run("Bearer not.a.jwt", null);
        AssertUnauthorized(context, nextInvoked);
        _permissionsRepository.Verify(r => r.GetPermissionsForAdmin(It.IsAny<string>()), Times.Never);
    }
}
