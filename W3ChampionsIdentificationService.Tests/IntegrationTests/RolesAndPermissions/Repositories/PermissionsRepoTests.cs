using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Repositories;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests.RolesAndPermissions.Repositories;

public class PermissionsRepoTests : IntegrationTestBase
{
    Fixture _fixture;

    [SetUp]
    public void PermissionsRepoTestsSetup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public async Task CreatePermission_ReadPermission_UpdatePermission_DeletePermission()
    {
        // arrange
        var permRepo = new PermissionsRepository(_mongoClient, _appConfig);
        var permission = _fixture.Create<Permission>();

        // act
        await permRepo.CreatePermission(permission);
        var document = await permRepo.GetPermission(permission.Id);

        var permission2 = _fixture.Create<Permission>();

        await permRepo.UpdatePermission(permission2);
        var document2 = await permRepo.GetPermission(permission.Id);

        await permRepo.DeletePermission(permission2.Id);
        var document3 = await permRepo.GetPermission(permission2.Id);

        // assert
        Assert.IsNotNull(document);
        Assert.AreEqual(permission.Id, document.Id);
        Assert.AreEqual(permission.Description, document.Description);
        Assert.IsNotNull(document2);
        Assert.AreEqual(permission.Id, document2.Id);
        Assert.AreEqual(permission.Description, document2.Description);
        Assert.IsNull(document3);
    }

    [Test]
    public async Task GetPermissions_SkipAndOffset_Success()
    {
        // arrange
        var permRepo = new PermissionsRepository(_mongoClient, _appConfig);
        var listOfPermissions = new List<Permission>();
        for (int i = 0; i < 10; i++)
        {
            var permission = _fixture.Create<Permission>();
            listOfPermissions.Add(permission);
            await permRepo.CreatePermission(permission);
        }

        // act
        var permissions = await permRepo.GetAllPermissions(2, 2);
        var allPermissions = await permRepo.GetAllPermissions();

        // assert
        Assert.AreEqual(2, permissions.Count);
        Assert.AreEqual(listOfPermissions[2].Id, permissions[0].Id);
        Assert.AreEqual(listOfPermissions[3].Id, permissions[1].Id);
        Assert.AreEqual(10, allPermissions.Count);
    }

    private static Permission BuildPermission(string battleTag, EPermission[] permissions)
    {
        return new Permission { BattleTag = battleTag, Description = "test", Permissions = permissions };
    }

    [Test]
    public async Task GetAdminBattleTags_ReturnsOnlyRowsWithPermissions()
    {
        // arrange
        var permRepo = new PermissionsRepository(_mongoClient, _appConfig);
        await permRepo.CreatePermission(BuildPermission("Foo#1234", [EPermission.Moderation]));
        await permRepo.CreatePermission(BuildPermission("Bar#99", [EPermission.Permissions, EPermission.Logs]));
        await permRepo.CreatePermission(BuildPermission("Empty#1", []));
        await permRepo.CreatePermission(BuildPermission("Null#2", null));

        // act
        var result = await permRepo.GetAdminBattleTags();

        // assert
        CollectionAssert.AreEquivalent(new[] { "Foo#1234", "Bar#99" }, result);
    }

    [Test]
    public async Task GetAdminBattleTags_NoRows_ReturnsEmptyList()
    {
        var permRepo = new PermissionsRepository(_mongoClient, _appConfig);

        var result = await permRepo.GetAdminBattleTags();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetAdminBattleTags_PreservesStoredCasing()
    {
        var permRepo = new PermissionsRepository(_mongoClient, _appConfig);
        await permRepo.CreatePermission(BuildPermission("MiXeD#7", [EPermission.Moderation]));

        var result = await permRepo.GetAdminBattleTags();

        CollectionAssert.AreEqual(new[] { "MiXeD#7" }, result);
    }

    [Test]
    public async Task GetPermissionsForAdmin_NullPermissions_ReturnsEmptyList()
    {
        var permRepo = new PermissionsRepository(_mongoClient, _appConfig);
        await permRepo.CreatePermission(BuildPermission("Null#2", null));

        var result = await permRepo.GetPermissionsForAdmin("Null#2");

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }
}
