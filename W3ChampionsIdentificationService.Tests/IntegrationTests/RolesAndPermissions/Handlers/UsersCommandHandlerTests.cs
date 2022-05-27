using AutoFixture;
using NUnit.Framework;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissions.Handlers
{
    public class UsersCommandHandlerTests : IntegrationTestBase
    {
        Fixture _fixture;
        IPermissionsRepository _permissionsRepository;
        IRolesRepository _rolesRepository;
        IUsersRepository _usersRepository;

        [SetUp]
        public void UsersCommandHandlerTestsSetup()
        {
            _fixture = new Fixture();
            _permissionsRepository = new PermissionsRepository(_mongoClient, _appConfig);
            _rolesRepository = new RolesRepository(_mongoClient, _appConfig);
            _usersRepository = new UsersRepository(_mongoClient, _appConfig);
        }

        [Test]
        public void TestExists()
        {
            Assert.Pass();
        }
    }
}
