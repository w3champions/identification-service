using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.RolesAndPermissions.Controllers;

namespace W3ChampionsIdentificationService.Tests.UnitTests.RolesAndPermissions.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private Mock<IUsersRepository> _usersRepositoryMock;
    private Mock<IUsersCommandHandler> _usersCommandHandlerMock;
    private UsersController _controller;

    [SetUp]
    public void SetUp()
    {
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _usersCommandHandlerMock = new Mock<IUsersCommandHandler>();
        _controller = new UsersController(_usersRepositoryMock.Object, _usersCommandHandlerMock.Object);
    }

    [Test]
    public async Task Exists_UserFound_ReturnsOkWithCanonicalId()
    {
        const string canonicalBattleTag = "TORREN#11438";
        const string queryWithDifferentCasing = "torren#11438";

        _usersRepositoryMock
            .Setup(r => r.GetUser(queryWithDifferentCasing))
            .ReturnsAsync(new User { Id = canonicalBattleTag, BnetId = "811045114" });

        var result = await _controller.Exists(queryWithDifferentCasing);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected 200 OK with body, got {0}", result?.GetType().Name);
        Assert.AreEqual(200, okResult.StatusCode);

        var body = okResult.Value as UserExistsResponse;
        Assert.IsNotNull(body, "Response body should be a UserExistsResponse");
        Assert.AreEqual(canonicalBattleTag, body.Id,
            "Body must contain the canonical Id from the matched user, not the request casing.");
    }

    [Test]
    public async Task Exists_UserFoundWithExactCasing_ReturnsOkWithSameCanonicalId()
    {
        const string canonicalBattleTag = "Faro#2494";

        _usersRepositoryMock
            .Setup(r => r.GetUser(canonicalBattleTag))
            .ReturnsAsync(new User { Id = canonicalBattleTag });

        var result = await _controller.Exists(canonicalBattleTag);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var body = okResult.Value as UserExistsResponse;
        Assert.IsNotNull(body);
        Assert.AreEqual(canonicalBattleTag, body.Id);
    }

    [Test]
    public async Task Exists_UserNotFound_ReturnsNotFound()
    {
        _usersRepositoryMock
            .Setup(r => r.GetUser(It.IsAny<string>()))
            .ReturnsAsync((User)null);

        var result = await _controller.Exists("nonexistent#9999");

        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [Test]
    public async Task Exists_NullId_ReturnsBadRequest()
    {
        var result = await _controller.Exists(null);

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        _usersRepositoryMock.Verify(r => r.GetUser(It.IsAny<string>()), Times.Never);
    }
}
