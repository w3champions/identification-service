using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions;

namespace W3ChampionsIdentificationService.Tests.Integration.RolesAndPermissions.Repositories
{
    public class UsersRepoTests : IntegrationTestBase
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
            var userRepo = new UsersRepository(_mongoClient, _appConfig);
            var user = _fixture.Create<User>();

            // act
            await userRepo.CreateUser(user);
            var doc1 = await userRepo.GetUser(user.Id);

            var user2 = _fixture.Create<User>();
            user2.Id = user.Id;
            await userRepo.UpdateUser(user2);
            var doc2 = await userRepo.GetUser(user.Id);

            await userRepo.DeleteUser(user.Id);
            var doc3 = await userRepo.GetUser(user.Id);

            // assert
            Assert.IsNotNull(doc1, "User is null after creation");
            Assert.AreEqual(user.Id, doc1.Id, "User's ID is not correct after creation");
            Assert.AreEqual(user.Roles, doc1.Roles, "User's Permissions are not correct after creation");
            Assert.IsNotNull(doc2, "User is null after update");
            Assert.AreEqual(user.Id, doc2.Id, "User's ID is not correct after update");
            Assert.AreEqual(user2.Roles, doc2.Roles, "User's permissions are not correct after update");
            Assert.IsNull(doc3, "User was not null after deletion");
        }

        [Test]
        public async Task GetRoles_SkipAndOffset_Success()
        {
            // arrange
            var userRepo = new UsersRepository(_mongoClient, _appConfig);
            var listOfUsers = new List<User>();
            for (int i = 0; i < 10; i++)
            {
                var user = _fixture.Create<User>();
                user.Id = (i + 1).ToString();
                listOfUsers.Add(user);
                await userRepo.CreateUser(user);
            }

            // act
            var users = await userRepo.GetAllUsers(4, 4);
            var allUsers = await userRepo.GetAllUsers();

            // assert
            Assert.AreEqual(4, users.Count, "Wrong number of users returned");
            Assert.AreEqual(listOfUsers[4].Id, users[0].Id, "First user is not correct");
            Assert.AreEqual(listOfUsers[5].Id, users[1].Id, "Second user is not correct");
            Assert.AreEqual(listOfUsers[6].Id, users[2].Id, "Third user is not correct");
            Assert.AreEqual(listOfUsers[7].Id, users[3].Id, "Fourth user is not correct");
            Assert.AreEqual(10, allUsers.Count, "Incorrect number of users returned by GetAllUsers()");
        }
    }
}
