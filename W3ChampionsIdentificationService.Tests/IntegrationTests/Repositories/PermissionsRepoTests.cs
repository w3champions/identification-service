using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions;

namespace W3ChampionsIdentificationService.Tests.Integration.Repositories
{
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
            permission2.Id = permission.Id;

            await permRepo.UpdatePermission(permission2);
            var document2 = await permRepo.GetPermission(permission.Id);

            await permRepo.DeletePermission(permission2.Id);
            var document3 = await permRepo.GetPermission(permission2.Id);

            // assert
            Assert.IsNotNull(document);
            Assert.AreEqual(permission.Id, document.Id);
            Assert.AreEqual(permission.Name, document.Name);
            Assert.AreEqual(permission.Description, document.Description);
            Assert.IsNotNull(document2);
            Assert.AreEqual(permission.Id, document2.Id);
            Assert.AreEqual(permission2.Name, document2.Name);
            Assert.AreEqual(permission2.Description, document2.Description);
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
                permission.Id = (i + 1).ToString(); 
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
    }
}
