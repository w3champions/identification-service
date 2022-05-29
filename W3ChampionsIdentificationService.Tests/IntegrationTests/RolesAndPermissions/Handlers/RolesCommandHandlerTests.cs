using AutoFixture;
using NUnit.Framework;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissions.Handlers
{
    public class RolesCommandHandlerTests : IntegrationTestBase
    {
        Fixture _fixture;
        IPermissionsRepository _permissionsRepository;
        IRolesRepository _rolesRepository;
        IUsersRepository _usersRepository;
        RolesAndPermissionsValidator _validator;
        IRolesCommandHandler _rolesCommandHandler;

        [SetUp]
        public void RolesCommandHandlerTestsSetup()
        {
            _fixture = new Fixture();
            _permissionsRepository = new PermissionsRepository(_mongoClient, _appConfig);
            _rolesRepository = new RolesRepository(_mongoClient, _appConfig);
            _usersRepository = new UsersRepository(_mongoClient, _appConfig);
            _validator = new RolesAndPermissionsValidator(_permissionsRepository, _rolesRepository, _usersRepository);
            _rolesCommandHandler = new RolesCommandHandler(_rolesRepository, _permissionsRepository, _validator);
        }

        [Test]
        public async Task CreateRole_Success()
        {
            // arrange
            var role = _fixture.Create<Role>();
            role.Id = "moderator";
            await AddPermissionsForRole(role);

            // act
            await _rolesCommandHandler.CreateRole(role);
            var result = await _rolesRepository.GetRole(role.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(role.Id, result.Id);
            Assert.AreEqual(result.Permissions.Count, role.Permissions.Count);
            Assert.AreEqual(result.Permissions[0], role.Permissions[0]);
        }

        [Test]
        public async Task CreateRole_WithInvalidPermissions_ThrowsException()
        {
            // arrange
            var role = _fixture.Create<Role>();
            role.Id = "moderator";
            await AddPermissionsForRole(role);
            await _permissionsRepository.DeletePermission(role.Permissions[0]);

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _rolesCommandHandler.CreateRole(role);
            });

            // assert
            Assert.AreEqual(404, ex.StatusCode);
            Assert.AreEqual($"Permissions: '{role.Permissions[0]}' do not exist", ex.Message);
        }

        [Test]
        public async Task CreateRole_RoleIdAlreadyExists_ThrowsException()
        {
            // arrange
            var role = _fixture.Create<Role>();
            role.Id = "moderator";
            await AddPermissionsForRole(role);
            await _rolesCommandHandler.CreateRole(role);

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _rolesCommandHandler.CreateRole(role);
            });

            // assert
            Assert.AreEqual(409, ex.StatusCode);
            Assert.AreEqual($"Role with id: {role.Id} already exists", ex.Message);
        }

        [Test]
        [TestCase(null, "moderators the ladder", TestName = "RoleIdIsNull")]
        [TestCase("", "moderates the ladder", TestName = "RoleIdIsEmpty")]
        [TestCase("moderator", null, TestName = "RoleDescriptionIsNull")]
        [TestCase("moderator", "", TestName = "RoleDescriptionIsEmpty")]
        public async Task CreateRole_BadInput_ThrowsException(string id, string description)
        {
            // arrange
            var role = _fixture.Create<Role>();
            role.Id = id;
            role.Description = description;
            await AddPermissionsForRole(role);

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _rolesCommandHandler.CreateRole(role);
            });

            // assert
            Assert.AreEqual(400, ex.StatusCode);
            StringAssert.Contains("cannot be null or empty", ex.Message);
        }

        private async Task AddPermissionsForRole(Role role)
        {
            foreach(var permission in role.Permissions)
            {
                await _permissionsRepository.CreatePermission(new Permission()
                {
                    Id = permission,
                    Description = permission,
                });
            }
        }
    }
}
