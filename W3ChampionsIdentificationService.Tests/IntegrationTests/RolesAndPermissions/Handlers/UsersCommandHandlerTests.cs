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
            var role2 = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);
            roleList.Add(role2.Id);

            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList,
            };

            // act
            await _usersCommandHandler.CreateUser(user);
            var result = await _usersRepository.GetUser(user.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.Roles.Count, result.Roles.Count);
        }

        [Test]
        public void CreateUser_RoleDoesntExist_ThrowsException()
        {
            // arrange
            var roleList = new List<string>();
            roleList.Add("nonExistentRole");

            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(user);
            });

            // assert
            Assert.AreEqual(404, ex.StatusCode);
            Assert.AreEqual($"Roles: '{string.Join(",", roleList)}' do not exist", ex.Message);
        }

        [Test]
        public void CreateUser_InvalidRequest_ThrowsException()
        {
            // arrange
            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = null,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(user);
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

            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList,
            };

            // act
            await _usersCommandHandler.CreateUser(user);
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(user);
            });

            // assert
            Assert.AreEqual(409, ex.StatusCode);
            Assert.AreEqual($"User: {user.Id} already exists", ex.Message);
        }

        [Test]
        public async Task CreateUser_NotFormattedLikeABattletag_ThrowException()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var user = new User()
            {
                Id = "notabattletagformat#14",
                Roles = roleList,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.CreateUser(user);
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
            var role3 = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);
            roleList.Add(role2.Id);
            roleList.Add(role3.Id);


            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(user);

            // act
            var result = await _usersRepository.GetUser(user.Id);
            user.Roles.RemoveAt(user.Roles.Count -1);
            await _usersCommandHandler.UpdateUser(user);
            var resultAfterUpdate = await _usersRepository.GetUser(user.Id);

            // assert
            Assert.NotNull(resultAfterUpdate);
            Assert.AreEqual(user.Roles.Count, resultAfterUpdate.Roles.Count);
        }

        [Test]
        public async Task UpdateUser_WithRoleThatDoesntExist_ThrowsException()
        {
            // arrange
            var role = await CreateFixtureRoleInDb();

            var roleList = new List<string>();
            roleList.Add(role.Id);

            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(user);

            var role2 = _fixture.Create<Role>();
            var roleList2 = new List<string>();
            roleList2.Add(role2.Id);

            var user2 = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList2,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.UpdateUser(user2);
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

            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(user);

            var user2 = new User()
            {
                Id = "cepheid#1467",
                Roles = null,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.UpdateUser(user2);
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

            var user = new User()
            {
                Id = "cepheid#1467",
                Roles = roleList,
            };
            await _usersCommandHandler.CreateUser(user);

            // act
            await _usersCommandHandler.DeleteUser(user.Id);
            var result = await _usersRepository.GetUser(user.Id);

            // assert
            Assert.IsNull(result);
        }

        [Test]
        public void DeleteUser_IdDoesntExist_ThrowsException()
        {
            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () =>
            {
                await _usersCommandHandler.DeleteUser("idThatDoesntExist#1234");
            });

            // assert
            Assert.AreEqual(404, ex.StatusCode);
            Assert.AreEqual($"Role with id: 'idThatDoesntExist#1234' not found", ex.Message);
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
