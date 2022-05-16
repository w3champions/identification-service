using MongoDB.Driver;
using NUnit.Framework;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.Tests.Integration;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests
{
    public class PermissionsCrudTests : IntegrationTestBase
    {
        [Test]
        public async Task CreatePermission_ReadPermission_UpdatePermission_DeletePermission()
        {
            // arrange
            var permRepo = new PermissionsRepository(_mongoClient, _appConfig);
            var permission = new Permission()
            {
                Id = "1",
                Name = "BanPlayer",
                Description = "Can Ban players from game modes",
            };

            // act
            await permRepo.CreatePermission(permission);
            var document = await permRepo.GetPermission(permission.Id);

            var permission2 = new Permission()
            {
                Id = permission.Id,
                Name = "BanPlayerFromGameModes",
                Description = "Ban Players"
            };

            await permRepo.UpdatePermission(permission2);
            var document2 = await permRepo.GetPermission(permission.Id);

            await permRepo.DeletePermission(permission2.Id);
            var document3 = await permRepo.GetPermission(permission2.Id);

            // test
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
    }
}
