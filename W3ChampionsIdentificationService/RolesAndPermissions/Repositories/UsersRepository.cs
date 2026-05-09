using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.RolesAndPermissions.Repositories;

public class UsersRepository(MongoClient mongoClient, IAppConfig appConfig) : MongoDbRepositoryBase(mongoClient, appConfig), IUsersRepository
{
    public async Task<User> GetUser(string id)
    {
        if (id is null) return null;
        return await LoadFirst<User>(x => x.IdNormalized == id.ToLowerInvariant());
    }

    public async Task<List<User>> GetAllUsers(int? limit = 50, int? offset = 0)
    {
        return await LoadAll<User>(null, limit, offset);
    }

    public async Task CreateUser(User user)
    {
        await Insert(user);
    }

    public async Task UpdateUser(User user)
    {
        await Upsert(user);
    }

    public async Task DeleteUser(string id)
    {
        await Delete<User>(id);
    }

    public async Task CreateIndex()
    {
        var collection = CreateCollection<User>();
        var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.IdNormalized);
        var options = new CreateIndexOptions { Unique = true, Name = "IdNormalized_unique" };
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<User>(indexKeys, options));
    }

    public async Task MigrateIdNormalized()
    {
        var rawCollection = CreateClient().GetCollection<BsonDocument>(typeof(User).Name);

        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Exists("IdNormalized", false),
            Builders<BsonDocument>.Filter.Type("_id", BsonType.String)
        );
        var update = new[]
        {
            new BsonDocument("$set",
                new BsonDocument("IdNormalized",
                    new BsonDocument("$toLower", "$_id")))
        };

        await rawCollection.UpdateManyAsync(filter, PipelineDefinition<BsonDocument, BsonDocument>.Create(update));
    }
}
