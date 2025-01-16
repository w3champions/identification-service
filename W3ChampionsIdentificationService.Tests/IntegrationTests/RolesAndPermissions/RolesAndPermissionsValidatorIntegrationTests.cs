using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.RolesAndPermissions.Repositories;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests.RolesAndPermissions;

public class RolesAndPermissionsValidatorIntegrationTests : IntegrationTestBase
{
    Fixture _fixture;
    IPermissionsRepository _permissionsRepository;
    IRolesRepository _rolesRepository;
    IUsersRepository _usersRepository;
    RolesAndPermissionsValidator _validator;

    [SetUp]
    public void RolesAndPermissionsValidatorIntegrationSetup()
    {
        _fixture = new Fixture();
        _permissionsRepository = new PermissionsRepository(_mongoClient, _appConfig);
        _rolesRepository = new RolesRepository(_mongoClient, _appConfig);
        _usersRepository = new UsersRepository(_mongoClient, _appConfig);
        _validator = new RolesAndPermissionsValidator(_permissionsRepository, _rolesRepository, _usersRepository);
    }

    [Test]
    [Ignore("Ignore test because of Permissions design change")]
    public async Task ValidatePermissionList_PermissionsExist_Success()
    {
        // arrange
        var permissions = new List<Permission>();
        for (var i = 0; i < 2; i++)
        {
            var perm = _fixture.Create<Permission>();
            permissions.Add(perm);
            await _permissionsRepository.CreatePermission(perm);
        }

        // act & assert
        Assert.DoesNotThrowAsync(async () =>
        {
            await _validator.ValidatePermissionList(permissions.Select(p => p.Id).ToList());
        }, "Unexpected HttpException");
    }

    [Test]
    [Ignore("Ignore test because of Permissions design change")]
    public async Task ValidatePermissionList_OnePermissionDoesNotExist_ThrowsHttpException()
    {
        // arrange
        var permissions = new List<Permission>();
        for (var i = 0; i < 2; i++)
        {
            var perm = _fixture.Create<Permission>();
            permissions.Add(perm);
            if (i != 1)
            {
                await _permissionsRepository.CreatePermission(perm);
            }
        }

        // act & assert
        var ex = Assert.ThrowsAsync<HttpException>(async () => await _validator.ValidatePermissionList(permissions.Select(x => x.Id).ToList()));

        Assert.AreEqual(404, ex.StatusCode);
        StringAssert.Contains("Permissions: ", ex.Message);
        StringAssert.Contains("do not exist", ex.Message);
    }

    [Test]
    [Ignore("Ignore test because of Permissions design change")]
    [TestCase("canMute", "Can mute a player", TestName = "PermissionValidationHttp_CorrectlyFormattedPermission_Success")]
    [TestCase("", "Can mute a player", TestName = "PermissionValidationHttp_IdIsEmptyString_Throws400")]
    [TestCase(null, "Can mute a player", TestName = "PermissionValidationHttp_IdIsNull_Throws400")]
    [TestCase("canMute", "", TestName = "PermissionValidationHttp_DescriptionIsEmptyString_Throws400")]
    [TestCase("canMute", null, TestName = "PermissionValidationHttp_DescriptionIsNull_Throws400")]
    public void PermissionsValidation_ThrowsCorrectErrors(string id, string description)
    {
        var permission = new Permission()
        {
            Description = description
        };

        if (!string.IsNullOrEmpty(permission.Id) &&
            !string.IsNullOrEmpty(permission.Description))
        {
            Assert.DoesNotThrow(() => _validator.ValidatePermission(permission));
            Assert.Pass();
        }

        var exception = Assert.Throws<HttpException>(() => _validator.ValidatePermission(permission));
        Assert.AreEqual(400, exception.StatusCode);
        StringAssert.Contains("cannot be null or empty", exception.Message);
    }

    [Test]
    [Ignore("Ignore test because of Permissions design change")]
    [TestCase("moderator", "access to moderation tools", TestName = "RoleValidationHttp_CorrectlyFormattedRole_Success")]
    [TestCase("", "access to moderation tools", TestName = "RoleValidationHttp_IdIsEmptyString_Throws400")]
    [TestCase(null, "access to moderation tools", TestName = "RoleValidationHttp_IdIsNull_Throws400")]
    [TestCase("moderator", "", TestName = "RoleValidationHttp_DescriptionIsEmptyString_Throws400")]
    [TestCase("moderator", null, TestName = "RoleValidationHttp_DescriptionIsNull_Throws400")]
    public void RoleValidation_ThrowsCorrectErrors(string id, string description)
    {
        var role = new Role()
        {
            Id = id,
            Description = description
        };

        if (!string.IsNullOrEmpty(role.Id) &&
            !string.IsNullOrEmpty(role.Description))
        {
            Assert.DoesNotThrow(() => _validator.ValidateRole(role));
            Assert.Pass();
        }

        var exception = Assert.Throws<HttpException>(() => _validator.ValidateRole(role));
        Assert.AreEqual(400, exception.StatusCode);
        StringAssert.Contains("cannot be null or empty", exception.Message);
    }

    public void RoleListValidation_WhenDuplicates_ThrowsError()
    {
        // arrange
        var role1 = _fixture.Create<Role>();
        var role2 = _fixture.Create<Role>();

        var roleList = new List<Role>();

        roleList.Add(role1);
        roleList.Add(role2);
        roleList.Add(role1);

        // act
        var ex = Assert.ThrowsAsync<HttpException>(async () =>
        {
            await _validator.ValidateRoleList(roleList.Select(x => x.Id).ToList());
        });

        Assert.AreEqual(400, ex.StatusCode);
        Assert.AreEqual("Cannot have duplicate Roles", ex.Message);
    }

    [Test]
    [Ignore("Ignore test because of Permissions design change")]
    [TestCase("Cepheid#1467", TestName = "UserValidationHttp_CorrectlyFormattedPermission_Success")]
    [TestCase("", TestName = "UserValidationHttp_BattleTagIsEmptyString_Throws400")]
    [TestCase(null, TestName = "UserValidationHttp_BattleTagIsNull_Throws400")]
    [TestCase("incorrectlyFormattedBattletag#1", TestName = "UserValidationHttp_BattleTagIsIncorrectFormat_Throws400")]
    public void UserValidation_ThrowsCorrectErrors(string tag)
    {
        var user = new User()
        {
            Id = tag,
            Roles = new List<string>()
        };

        if (!string.IsNullOrEmpty(user.Id) &&
            user.Id == "Cepheid#1467")
        {
            Assert.DoesNotThrowAsync(async () => await _validator.ValidateCreateUser(user));
            Assert.Pass();
        }

        if (user.Id == "incorrectlyFormattedBattletag#1")
        {
            var ex = Assert.ThrowsAsync<HttpException>(async () => await _validator.ValidateCreateUser(user));
            Assert.AreEqual("BattleTag is not valid", ex.Message);
            Assert.AreEqual(400, ex.StatusCode);
            Assert.Pass();
        }

        var exception = Assert.ThrowsAsync<HttpException>(async () => await _validator.ValidateCreateUser(user));
        Assert.AreEqual(400, exception.StatusCode);
        StringAssert.Contains("cannot be null or empty", exception.Message);
    }
}
