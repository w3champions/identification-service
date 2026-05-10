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

    private const int MigrationBatchSize = 1000;

    // Idempotent at the row level: skips docs whose IdNormalized already equals the
    // canonical lowercase. Safe to re-run after a partial/interrupted execution —
    // already-migrated docs are no-ops on the next pass.
    //
    // We re-evaluate IdNormalized client-side with .NET ToLowerInvariant() so it
    // matches GetUser's lookup. MongoDB's $toLower only folds ASCII, which would
    // leave non-ASCII characters (e.g. 'Ǫ' U+01EA) uppercase and break lookups for
    // those users. We also process docs where IdNormalized exists but is stale —
    // legacy rows written before canonicalization stored a verbatim copy of _id.
    //
    // Writes are flushed in batches of MigrationBatchSize to bound client memory
    // and make progress visible to other replicas (so a crash mid-run doesn't
    // require reprocessing everything from scratch).
    public async Task MigrateIdNormalized()
    {
        var rawCollection = CreateClient().GetCollection<BsonDocument>(typeof(User).Name);

        var filter = Builders<BsonDocument>.Filter.Type("_id", BsonType.String);
        var projection = Builders<BsonDocument>.Projection
            .Include("_id")
            .Include("IdNormalized");

        var bulkOps = new List<WriteModel<BsonDocument>>(MigrationBatchSize);

        using var cursor = await rawCollection.FindAsync(filter,
            new FindOptions<BsonDocument, BsonDocument> { Projection = projection });
        while (await cursor.MoveNextAsync())
        {
            foreach (var doc in cursor.Current)
            {
                var id = doc["_id"].AsString;
                var canonical = id.ToLowerInvariant();
                var existing = doc.TryGetValue("IdNormalized", out var idn) && idn.IsString
                    ? idn.AsString
                    : null;
                if (existing == canonical) continue;

                bulkOps.Add(new UpdateOneModel<BsonDocument>(
                    Builders<BsonDocument>.Filter.Eq("_id", id),
                    Builders<BsonDocument>.Update.Set("IdNormalized", canonical)));

                if (bulkOps.Count >= MigrationBatchSize)
                {
                    await rawCollection.BulkWriteAsync(bulkOps);
                    bulkOps.Clear();
                }
            }
        }

        if (bulkOps.Count > 0)
        {
            await rawCollection.BulkWriteAsync(bulkOps);
        }
    }
}
