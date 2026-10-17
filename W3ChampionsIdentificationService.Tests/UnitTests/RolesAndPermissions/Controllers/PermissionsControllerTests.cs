using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.RolesAndPermissions.Controllers;
using W3ChampionsIdentificationService.WebApi.ActionFilters;

namespace W3ChampionsIdentificationService.Tests.UnitTests.RolesAndPermissions.Controllers;

[TestFixture]
public class PermissionsControllerTests
{
    private Mock<IPermissionsRepository> _permissionsRepositoryMock;
    private Mock<IPermissionsCommandHandler> _permissionsCommandHandlerMock;
    private PermissionsController _controller;

    [SetUp]
    public void SetUp()
    {
        _permissionsRepositoryMock = new Mock<IPermissionsRepository>();
        _permissionsCommandHandlerMock = new Mock<IPermissionsCommandHandler>();
        _controller = new PermissionsController(_permissionsRepositoryMock.Object, _permissionsCommandHandlerMock.Object);
    }

    private static MethodInfo GetAdminBattleTagsMethod() =>
        typeof(PermissionsController).GetMethod(nameof(PermissionsController.GetAdminBattleTags));

    [Test]
    public async Task GetAdminBattleTags_ReturnsOkWithTagsFromRepository()
    {
        _permissionsRepositoryMock
            .Setup(r => r.GetAdminBattleTags())
            .ReturnsAsync(new List<string> { "Foo#1234", "Bar#99" });

        var result = await _controller.GetAdminBattleTags();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected 200 OK, got {0}", result?.GetType().Name);
        var body = okResult.Value as AdminBattleTagsResponse;
        Assert.IsNotNull(body);
        CollectionAssert.AreEqual(new[] { "Foo#1234", "Bar#99" }, body.BattleTags);
    }

    [Test]
    public async Task GetAdminBattleTags_NoAdmins_ReturnsOkWithEmptyArray()
    {
        _permissionsRepositoryMock
            .Setup(r => r.GetAdminBattleTags())
            .ReturnsAsync(new List<string>());

        var result = await _controller.GetAdminBattleTags();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var body = okResult.Value as AdminBattleTagsResponse;
        Assert.IsNotNull(body);
        Assert.IsEmpty(body.BattleTags);
    }

    [Test]
    public void GetAdminBattleTags_IsGuardedBySecretFilterOnly()
    {
        var method = GetAdminBattleTagsMethod();

        var serviceFilter = method.GetCustomAttribute<ServiceFilterAttribute>();
        Assert.IsNotNull(serviceFilter);
        Assert.AreEqual(typeof(InternalSecretAuthFilter), serviceFilter.ServiceType);
        Assert.IsEmpty(method.GetCustomAttributes().OfType<HasPermissionsPermission>());
        Assert.IsEmpty(
            typeof(PermissionsController).GetCustomAttributes(true).OfType<HasPermissionsPermission>(),
            "A class-level HasPermissionsPermission would 401 the roster endpoint");
    }

    [Test]
    public void AdminBattleTagsResponse_SerializesWithCamelCaseBattleTagsProperty()
    {
        var json = JsonSerializer.Serialize(
            new AdminBattleTagsResponse(new List<string> { "Foo#1234" }),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.AreEqual("{\"battleTags\":[\"Foo#1234\"]}", json);
    }

    [Test]
    public void GetAdminBattleTags_RouteIsAdminBattletags()
    {
        var httpGet = GetAdminBattleTagsMethod().GetCustomAttribute<HttpGetAttribute>();

        Assert.IsNotNull(httpGet);
        Assert.AreEqual("admin-battletags", httpGet.Template);
    }
}
