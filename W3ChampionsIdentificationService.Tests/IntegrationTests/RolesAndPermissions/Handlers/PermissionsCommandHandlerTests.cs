﻿using AutoFixture;
using NUnit.Framework;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.RolesAndPermissions.Repositories;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests.RolesAndPermissions.Handlers;

public class PermissionsCommandHandlerTests : IntegrationTestBase
{
    Fixture _fixture;
    IPermissionsRepository _permissionsRepository;
    IRolesRepository _rolesRepository;
    IUsersRepository _usersRepository;
    RolesAndPermissionsValidator _validator;

    [SetUp]
    public void PermissionsCommandHandlerTestsSetup()
    {
        _fixture = new Fixture();
        _permissionsRepository = new PermissionsRepository(_mongoClient, _appConfig);
        _rolesRepository = new RolesRepository(_mongoClient, _appConfig);
        _usersRepository = new UsersRepository(_mongoClient, _appConfig);
        _validator = new RolesAndPermissionsValidator(_permissionsRepository, _rolesRepository, _usersRepository);
    }

    [Test]
    public async Task CreatePermission_WhenInputIsValid_Success()
    {
        // arrange
        var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
        var permission = _fixture.Create<Permission>();

        // act
        await permissionsCommandHandler.CreatePermission(permission);
        var result = await _permissionsRepository.GetPermission(permission.Id);

        // assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Id);
        Assert.IsNotNull(result.Description);
    }

    [Test]
    public async Task CreatePermission_WithId_AlreadyExists_ThrowsException_AndIsIdempotent()
    {
        // arrange
        var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
        var permission = _fixture.Create<Permission>();

        // act
        await permissionsCommandHandler.CreatePermission(permission);
        var result = await _permissionsRepository.GetAllPermissions();

        // assert
        var ex = Assert.ThrowsAsync<HttpException>(async () =>
        {
            await permissionsCommandHandler.CreatePermission(permission);
        }, "Does not throw a HttpException");
        Assert.AreEqual(ex.Message, $"Permission with id: {permission.Id} already exists", "Exception message was not correct");
        Assert.AreEqual(ex.StatusCode, 409, "Did not return correct status code");
        Assert.AreEqual(1, result.Count, "Should only have one record");
    }

    [Test]
    [TestCase(null, "b", TestName = "CreatePermission_WithInvalidProperties_IdIsNull_Throws400")]
    [TestCase("a", null, TestName = "CreatePermission_WithInvalidProperties_DescriptionIsNull_Throws400")]
    public void CreatePermission_WithInvalidProperties_ThrowsValidationHttpException(string description)
    {
        // arrange
        var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
        var permission = new Permission()
        {
            Description = description,
        };

        // act
        var ex = Assert.ThrowsAsync<HttpException>(async () =>
        {
            await permissionsCommandHandler.CreatePermission(permission);
        }, "Does not throw a HttpException");

        // assert
        StringAssert.Contains("cannot be null or empty", ex.Message, "Did not give the correct validation error");
        Assert.AreEqual(ex.StatusCode, 400, "Did not return correct status code");
    }

    [Test]
    public async Task DeletePermission_ExistsInDatabase_Success()
    {
        // arrange
        var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
        var permission = _fixture.Create<Permission>();

        // act
        await permissionsCommandHandler.CreatePermission(permission);
        var createdPermission = await _permissionsRepository.GetPermission(permission.Id);
        await permissionsCommandHandler.DeletePermission(permission.Id);
        var deletedPermission = await _permissionsRepository.GetPermission(permission.Id);

        // assert
        Assert.IsNotNull(createdPermission, "Created permission does not exist in the database");
        Assert.IsNull(deletedPermission, "Deleted permission still exists in the database");
    }

    [Test]
    public void DeletePermission_DoesNotExist_ThrowsCorrectException()
    {
        // arrange
        var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
        var id = "1";

        // act
        var ex = Assert.ThrowsAsync<HttpException>(async () =>
        {
            await permissionsCommandHandler.DeletePermission(id);
        }, "Does not throw HttpException");

        // assert
        StringAssert.Contains($"Permission with id: ${id} not found", ex.Message, "Did not give the correct validation error");
        Assert.AreEqual(ex.StatusCode, 404, "Did not return correct status code");
    }

    [Test]
    [TestCase(null, "b", TestName = "UpdatePermission_InvalidFormats_IdIsNull_Throws400")]
    [TestCase("a", null, TestName = "UpdatePermission_InvalidFormats_DescriptionIsNull_Throws400")]
    public void UpdatePermission_InvalidFormats_ThrowsCorrectExceptions(string description)
    {
        // arrange
        var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
        var startingPermission = _fixture.Create<Permission>();

        var permission = new Permission()
        {
            Description = description,
        };

        // act
        var ex = Assert.ThrowsAsync<HttpException>(async () =>
        {
            await permissionsCommandHandler.UpdatePermission(permission);
        }, "Does not throw a HttpException");

        // assert
        StringAssert.Contains("cannot be null or empty", ex.Message, "Did not give the correct validation error");
        Assert.AreEqual(ex.StatusCode, 400, "Did not return correct status code");
    }
}
