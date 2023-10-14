using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions;

// All of the following tests are skipped for now, because of a Permissions design change.

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissions.Repositories
{
    public class RolesRepoTests : IntegrationTestBase
    {
        Fixture _fixture;

        [SetUp]
        public void RolesRepoTestsSetup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public async Task CreateRole_ReadRole_UpdateRole_DeleteRole()
        {
            // arrange
            var roleRepo = new RolesRepository(_mongoClient, _appConfig);
            var role = _fixture.Create<Role>();

            // act
            await roleRepo.CreateRole(role);
            var doc1 = await roleRepo.GetRole(role.Id);

            var role2 = _fixture.Create<Role>();
            role2.Id = role.Id;
            await roleRepo.UpdateRole(role2);
            var doc2 = await roleRepo.GetRole(role.Id);

            await roleRepo.DeleteRole(role.Id);
            var doc3 = await roleRepo.GetRole(role.Id);

            // assert
            Assert.IsNotNull(doc1);
            Assert.AreEqual(role.Id, doc1.Id);
            Assert.AreEqual(role.Description, doc1.Description);
            Assert.AreEqual(role.Permissions, doc1.Permissions);
            Assert.IsNotNull(doc2);
            Assert.AreEqual(role.Id, doc2.Id);
            Assert.AreEqual(role2.Description, doc2.Description);
            Assert.AreEqual(role2.Permissions, doc2.Permissions);
            Assert.IsNull(doc3);
        }

        [Test]
        public async Task GetRoles_SkipAndOffset_Success()
        {
            // arrange
            var roleRepo = new RolesRepository(_mongoClient, _appConfig);
            var listOfRoles = new List<Role>();
            for (int i = 0; i < 10; i++)
            {
                var role = _fixture.Create<Role>();
                role.Id = (i + 1).ToString();
                listOfRoles.Add(role);
                await roleRepo.CreateRole(role);
            }

            // act
            var roles = await roleRepo.GetAllRoles(null, 3, 3);
            var allRoles = await roleRepo.GetAllRoles();

            // assert
            Assert.AreEqual(3, roles.Count);
            Assert.AreEqual(listOfRoles[3].Id, roles[0].Id);
            Assert.AreEqual(listOfRoles[4].Id, roles[1].Id);
            Assert.AreEqual(listOfRoles[5].Id, roles[2].Id);
            Assert.AreEqual(10, allRoles.Count);
        }
    }
}
