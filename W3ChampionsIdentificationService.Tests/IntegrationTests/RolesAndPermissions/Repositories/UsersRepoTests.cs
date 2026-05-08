using AutoFixture;
using MongoDB.Driver;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.Repositories;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests.RolesAndPermissions.Repositories;

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

    [Test]
    public async Task GetUser_CaseInsensitiveMatch_ReturnsCanonicalUser()
    {
        // arrange
        var userRepo = new UsersRepository(_mongoClient, _appConfig);
        var canonical = new User { Id = "TORREN#11438", BnetId = "811045114", Roles = new List<string>() };
        await userRepo.CreateUser(canonical);

        // act
        var foundLower = await userRepo.GetUser("torren#11438");
        var foundMixed = await userRepo.GetUser("Torren#11438");
        var foundExact = await userRepo.GetUser("TORREN#11438");

        // assert
        Assert.IsNotNull(foundLower);
        Assert.AreEqual("TORREN#11438", foundLower.Id, "Lowercase query must return the canonical-cased user.");
        Assert.IsNotNull(foundMixed);
        Assert.AreEqual("TORREN#11438", foundMixed.Id);
        Assert.IsNotNull(foundExact);
        Assert.AreEqual("TORREN#11438", foundExact.Id);
    }

    [Test]
    public async Task GetUser_NoMatch_ReturnsNull()
    {
        // arrange
        var userRepo = new UsersRepository(_mongoClient, _appConfig);
        var canonical = new User { Id = "Existing#1111", BnetId = "1", Roles = new List<string>() };
        await userRepo.CreateUser(canonical);

        // act
        var result = await userRepo.GetUser("nonexistent#9999");

        // assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task CreateUser_PopulatesIdNormalizedInPersistedDocument()
    {
        // arrange
        var userRepo = new UsersRepository(_mongoClient, _appConfig);
        var user = new User { Id = "MixedCase#0001", BnetId = "x", Roles = new List<string>() };
        await userRepo.CreateUser(user);

        // act
        var coll = CreateCollection<User>();
        var persisted = await coll.Find(x => x.Id == "MixedCase#0001").FirstOrDefaultAsync();

        // assert
        Assert.IsNotNull(persisted);
        Assert.AreEqual("mixedcase#0001", persisted.IdNormalized);
    }

    [Test]
    public async Task CreateIndex_CreatesUniqueIndexOnIdNormalized()
    {
        // arrange
        var userRepo = new UsersRepository(_mongoClient, _appConfig);

        // act
        await userRepo.CreateIndex();

        // assert
        var coll = CreateCollection<User>();
        var indexes = await (await coll.Indexes.ListAsync()).ToListAsync();
        var idNormalizedIndex = indexes.FirstOrDefault(i => i["name"] == "IdNormalized_unique");

        Assert.IsNotNull(idNormalizedIndex,
            $"Expected an index named 'IdNormalized_unique' but found: {string.Join(",", indexes.Select(i => i["name"].AsString))}");
        Assert.IsTrue(idNormalizedIndex["unique"].AsBoolean,
            "Index must be unique to prevent future casing-duplicate inserts.");
    }

    [Test]
    public async Task CreateIndex_Idempotent_RunningTwiceDoesNotThrow()
    {
        // arrange
        var userRepo = new UsersRepository(_mongoClient, _appConfig);

        // act & assert
        await userRepo.CreateIndex();
        Assert.DoesNotThrowAsync(async () => await userRepo.CreateIndex());
    }

    [Test]
    public async Task MigrateIdNormalized_BackfillsMissingField()
    {
        // Insert a doc directly with IdNormalized field absent (raw BSON)
        var rawColl = CreateClient().GetCollection<MongoDB.Bson.BsonDocument>("User");
        await rawColl.InsertOneAsync(new MongoDB.Bson.BsonDocument
        {
            { "_id", "PreMigration#9999" },
            { "BnetId", "999" },
            { "Roles", new MongoDB.Bson.BsonArray() },
            // No IdNormalized
        });

        var userRepo = new UsersRepository(_mongoClient, _appConfig);
        await userRepo.MigrateIdNormalized();

        var migrated = await rawColl.Find(MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", "PreMigration#9999")).FirstOrDefaultAsync();
        Assert.IsNotNull(migrated);
        Assert.IsTrue(migrated.Contains("IdNormalized"), "Migration must add IdNormalized field.");
        Assert.AreEqual("premigration#9999", migrated["IdNormalized"].AsString);
    }

    [Test]
    public async Task MigrateIdNormalized_DoesNotOverwriteExistingField()
    {
        var userRepo = new UsersRepository(_mongoClient, _appConfig);
        var canonical = new User { Id = "Existing#1234", BnetId = "x", Roles = new System.Collections.Generic.List<string>() };
        await userRepo.CreateUser(canonical);
        // Setter populated IdNormalized to "existing#1234"

        await userRepo.MigrateIdNormalized();

        var rawColl = CreateClient().GetCollection<MongoDB.Bson.BsonDocument>("User");
        var doc = await rawColl.Find(MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", "Existing#1234")).FirstOrDefaultAsync();
        Assert.AreEqual("existing#1234", doc["IdNormalized"].AsString,
            "Migration should not modify already-populated IdNormalized values.");
    }

    [Test]
    public async Task MigrateIdNormalized_Idempotent_RunningTwiceMatchesNoDocs()
    {
        var rawColl = CreateClient().GetCollection<MongoDB.Bson.BsonDocument>("User");
        await rawColl.InsertOneAsync(new MongoDB.Bson.BsonDocument
        {
            { "_id", "Once#1" }, { "BnetId", "1" }, { "Roles", new MongoDB.Bson.BsonArray() }
        });

        var userRepo = new UsersRepository(_mongoClient, _appConfig);
        await userRepo.MigrateIdNormalized();
        Assert.DoesNotThrowAsync(async () => await userRepo.MigrateIdNormalized());

        var doc = await rawColl.Find(MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", "Once#1")).FirstOrDefaultAsync();
        Assert.AreEqual("once#1", doc["IdNormalized"].AsString);
    }
}
