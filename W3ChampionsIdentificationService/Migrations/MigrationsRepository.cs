using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.Migrations;

// One-shot migration registry backed by a `_migrations` collection.
//
// Idempotency contract:
//   1. Check the sentinel doc; if present, the migration already ran — skip.
//   2. Otherwise execute the migration body.
//   3. Only on success record `{ _id: migrationId, appliedAt: <utc> }`.
//
// If the migration throws or the process is killed before step 3, no sentinel
// is written and the next startup re-runs it. The migration body must be
// idempotent at the row level so a partial/resumed run converges to the
// correct end state.
//
// Multi-replica races are tolerated: two pods may both see "not applied" and
// both run the body. The second InsertOne hits the unique _id index and we
// swallow the DuplicateKey. Work is duplicated but the result is correct.
public class MigrationsRepository(MongoClient mongoClient, IAppConfig appConfig)
    : MongoDbRepositoryBase(mongoClient, appConfig), IMigrationsRepository
{
    private const string CollectionName = "_migrations";

    public async Task RunIfNeeded(string migrationId, Func<Task> migration)
    {
        var collection = CreateClient().GetCollection<BsonDocument>(CollectionName);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", migrationId);

        if (await collection.Find(filter).AnyAsync())
        {
            return;
        }

        Log.Information("Running migration {MigrationId}", migrationId);
        await migration();

        try
        {
            await collection.InsertOneAsync(new BsonDocument
            {
                ["_id"] = migrationId,
                ["appliedAt"] = DateTime.UtcNow,
            });
            Log.Information("Migration {MigrationId} recorded as applied", migrationId);
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Another replica recorded it concurrently. Our run was idempotent, so this is fine.
            Log.Information("Migration {MigrationId} already recorded by another process", migrationId);
        }
    }
}
