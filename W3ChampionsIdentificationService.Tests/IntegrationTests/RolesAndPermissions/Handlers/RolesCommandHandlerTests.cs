using AutoFixture;
using NUnit.Framework;
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
        public void CreateRole_Success()
        {
            // arrange
            var role = _fixture.Create<Role>();

            // act
            _rolesCommandHandler.CreateRole(role);
            var result = _rolesRepository.GetRole(role.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(role.Id, result.Id);
        }
    }
}
