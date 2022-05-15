using NUnit.Framework;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.Tests.Integration;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests
{
    public class PermissionsCrudTests : IntegrationTestBase
    {
        [Test]
        public async void CreatePermission_ReadPermission_UpdatePermission_DeletePermission()
        {
            // arrange
            var mongoCollection = CreateCollection<Permission>();
            var permission = new Permission()
            {
                Id = "1",
                Name = "BanPlayer",
                Description = "Can Ban players from game modes",
            };


            // act
            mongoCollection.InsertOne(permission);
            var document = await mongoCollection.FindAsync(x => x.Id == permission.Id).ToListAsync();

            // test
        }
    }
}
