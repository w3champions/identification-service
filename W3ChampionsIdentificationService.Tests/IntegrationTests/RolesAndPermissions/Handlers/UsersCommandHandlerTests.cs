using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissions.Handlers
{
    public class UsersCommandHandlerTests : IntegrationTestBase
    {
        Fixture _fixture;
        IPermissionsRepository _permissionsRepository;
        IRolesRepository _rolesRepository;
        IUsersRepository _usersRepository;
        RolesAndPermissionsValidator _validator;
        IUsersCommandHandler _usersCommandHandler;

        [SetUp]
        public void RolesCommandHandlerTestsSetup()
        {
            _fixture = new Fixture();
            _permissionsRepository = new PermissionsRepository(_mongoClient, _appConfig);
            _rolesRepository = new RolesRepository(_mongoClient, _appConfig);
            _usersRepository = new UsersRepository(_mongoClient, _appConfig);
            _validator = new RolesAndPermissionsValidator(_permissionsRepository, _rolesRepository, _usersRepository);
            _usersCommandHandler = new UsersCommandHandler(_usersRepository, _rolesRepository, _validator);
        }

        [Test]
        public async Task CreateUser_Success()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList,
            };

            // act
            await _usersCommandHandler.CreateUser(userdto);
            var result = await _usersRepository.GetUser(userdto.BattleTag);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userdto.BattleTag, result.Id);
            Assert.AreEqual(role.Permissions.Count, result.Permissions.Count);
        }

        [Test]
        public void CreateUser_RoleDoesntExist_ThrowsException()
        {
            // arrange
            var roleList = new List<string>();
            roleList.Add("nonExistentRole");

            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(userdto);
            });

            // assert
            Assert.AreEqual(404, ex.StatusCode);
            Assert.AreEqual($"Roles: '{string.Join(",", roleList)}' do not exist", ex.Message);
        }

        [Test]
        public void CreateUser_InvalidRequest_ThrowsException()
        {
            // arrange
            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = null,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(userdto);
            });

            // assert
            Assert.AreEqual(400, ex.StatusCode);
            Assert.AreEqual("A user cannot have no Roles", ex.Message);
        }

        [Test]
        public async Task CreateUser_AlreadyExists_ThrowException()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList,
            };

            // act
            await _usersCommandHandler.CreateUser(userdto);
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(userdto);
            });

            // assert
            Assert.AreEqual(409, ex.StatusCode);
            Assert.AreEqual($"User: {userdto.BattleTag} already exists", ex.Message);
        }

        [Test]
        public async Task CreateUser_NotFormattedLikeABattletag_ThrowException()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var userdto = new UserDTO()
            {
                BattleTag = "notabattletagformat#14",
                Roles = roleList,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(userdto);
            });

            // assert
            Assert.AreEqual(400, ex.StatusCode);
            Assert.AreEqual("BattleTag is not valid", ex.Message);
        }

        [Test]
        public async Task UpdateUser_Success()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();
            var role2 = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);
            roleList.Add(role2.Id);

            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(userdto);

            // act
            var result = await _usersRepository.GetUser(userdto.BattleTag);
            userdto.Roles.RemoveAt(userdto.Roles.Count -1);
            await _usersCommandHandler.UpdateUser(userdto);
            var resultAfterUpdate = await _usersRepository.GetUser(userdto.BattleTag);

            // assert
            Assert.NotNull(resultAfterUpdate);
            Assert.AreEqual(role.Permissions.Count, resultAfterUpdate.Permissions.Count);
        }

        [Test]
        public async Task UpdateUser_WithRoleThatDoesntExist_ThrowsException()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(userdto);

            var role2 = _fixture.Create<Role>();
            var roleList2 = new List<string>();
            roleList2.Add(role2.Id);

            var userdto2 = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList2,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.UpdateUser(userdto2);
            });

            // assert
            Assert.AreEqual(404, ex.StatusCode);
            Assert.AreEqual($"Roles: '{string.Join("','", roleList2)}' do not exist", ex.Message);
        }

        [Test]
        public async Task UpdateUser_WithNullRoles_ThrowsException()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(userdto);

            var userdto2 = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = null,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.UpdateUser(userdto2);
            });

            // assert
            Assert.AreEqual(400, ex.StatusCode);
            Assert.AreEqual("A user cannot have no Roles", ex.Message);
        }

        [Test]
        public async Task DeleteUser_Success()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var userdto = new UserDTO()
            {
                BattleTag = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(userdto);

            // act
            await _usersCommandHandler.DeleteUser(userdto.BattleTag);
            var result = await _usersRepository.GetUser(userdto.BattleTag);

            // assert
            Assert.IsNull(result);
        }


        private async Task AddPermissionsForRole(Role role)
        {
            foreach (var permission in role.Permissions)
            {
                await _permissionsRepository.CreatePermission(new Permission()
                {
                    Id = permission,
                    Description = permission,
                });
            }
        }
        
        private async Task<Role> CreateFixtureRoleInDb()
        {
            Role role = _fixture.Create<Role>();
            await AddPermissionsForRole(role);
            await _rolesRepository.CreateRole(role);
            return role;
        }
    }
}
