using AutoFixture;
using NUnit.Framework;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Tests.Integration;

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissionsHandlers
{
    public class PermissionsCommandHandlerTests : IntegrationTestBase
    {
        private Fixture _fixture;
        private IPermissionsRepository _permissionsRepository;
        private RolesAndPermissionsValidator _validator;
        
        [SetUp]
        public void PermissionsCommandHandlerTestsSetup()
        {
            _fixture = new Fixture();
            _permissionsRepository = new PermissionsRepository(_mongoClient, _appConfig);
            _validator = new RolesAndPermissionsValidator(_permissionsRepository);
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
            Assert.IsNotNull(result.Name);
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
            var ex = Assert.ThrowsAsync<HttpException>(async () => { 
                await permissionsCommandHandler.CreatePermission(permission); 
            }, "Does not throw a HttpException");
            Assert.AreEqual(ex.Message, $"Permission with id: {permission.Id} already exists", "Exception message was not correct");
            Assert.AreEqual(ex.StatusCode, 409, "Did not return correct status code");
            Assert.AreEqual(1, result.Count, "Should only have one record");
        }

        [Test]
        public async Task CreatePermission_WithName_AlreadyExists_ThrowsException()
        {
            // arrange
            var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
            var permission = _fixture.Create<Permission>();

            // act
            await permissionsCommandHandler.CreatePermission(permission);
            var result = await _permissionsRepository.GetAllPermissions();
            permission.Id = "differentId";

            // assert
            var ex = Assert.ThrowsAsync<HttpException>(async () => {
                await permissionsCommandHandler.CreatePermission(permission);
            }, "Does not throw a HttpException");
            Assert.AreEqual(ex.Message, $"Permission with name: '{permission.Name}' already exists", "Exception message was not correct");
            Assert.AreEqual(ex.StatusCode, 409, "Did not return correct status code");
            Assert.AreEqual(1, result.Count, "Should only have one record");
        }

        [Test]
        [TestCase(null, "b", "c")]
        [TestCase("a", null, "c")]
        [TestCase("a", "b", null)]
        public void CreatePermission_WithInvalidProperties_ThrowsValidationHttpException(string id, string name, string description)
        {
            // arrange
            var permissionsCommandHandler = new PermissionsCommandHandler(_permissionsRepository, _validator);
            var permission = new Permission()
            {
                Id = id,
                Name = name,
                Description = description,
            };

            // act
            var ex = Assert.ThrowsAsync<HttpException>(async () => {
                await permissionsCommandHandler.CreatePermission(permission);
            }, "Does not throw a HttpException");

            // assert
            StringAssert.Contains("cannot be null or empty", ex.Message, "Did not give the correct validation error");
            Assert.AreEqual(ex.StatusCode, 400, "Did not return correct status code");
        }

        // create permission tests:
        // test that it calls the repo CreatePermission() when valid
        // test that it doesnt call the repo CreatePermission() when invalid

        // delete permission tests:
        // check that it returns 404 when the permission doesnt exist
        // check that it deletes it when it is valid

        // update permission tests:
        // check it calls UpdatePermission() when valid
        // check it doesnt call UpdatePermission() when invalid

    }
}
