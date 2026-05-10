using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Migrations;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests.Migrations;

public class MigrationsRepoTests : IntegrationTestBase
{
    [Test]
    public async Task RunIfNeeded_FirstCall_RunsMigrationAndRecordsSentinel()
    {
        var repo = new MigrationsRepository(_mongoClient, _appConfig);
        var ran = 0;

        await repo.RunIfNeeded("test_v1", () => { ran++; return Task.CompletedTask; });

        Assert.AreEqual(1, ran, "Migration body must run on first call.");

        var sentinels = CreateClient().GetCollection<BsonDocument>("_migrations");
        var doc = await sentinels.Find(Builders<BsonDocument>.Filter.Eq("_id", "test_v1")).FirstOrDefaultAsync();
        Assert.IsNotNull(doc, "Sentinel must be recorded after successful run.");
        Assert.IsTrue(doc.Contains("appliedAt"), "Sentinel must record appliedAt timestamp.");
    }

    [Test]
    public async Task RunIfNeeded_SubsequentCall_DoesNotRunMigrationAgain()
    {
        var repo = new MigrationsRepository(_mongoClient, _appConfig);
        var ran = 0;

        await repo.RunIfNeeded("test_v1", () => { ran++; return Task.CompletedTask; });
        await repo.RunIfNeeded("test_v1", () => { ran++; return Task.CompletedTask; });
        await repo.RunIfNeeded("test_v1", () => { ran++; return Task.CompletedTask; });

        Assert.AreEqual(1, ran, "Migration body must run exactly once across repeated calls.");
    }

    [Test]
    public async Task RunIfNeeded_MigrationThrows_NoSentinelWritten_AndRetryRunsAgain()
    {
        var repo = new MigrationsRepository(_mongoClient, _appConfig);
        var attempts = 0;

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repo.RunIfNeeded("test_v1", () =>
            {
                attempts++;
                throw new InvalidOperationException("simulated mid-migration crash");
            }));

        var sentinels = CreateClient().GetCollection<BsonDocument>("_migrations");
        var doc = await sentinels.Find(Builders<BsonDocument>.Filter.Eq("_id", "test_v1")).FirstOrDefaultAsync();
        Assert.IsNull(doc, "Sentinel must NOT be written when migration body throws.");

        // Retry on next "startup" must run the body again because no sentinel exists.
        await repo.RunIfNeeded("test_v1", () => { attempts++; return Task.CompletedTask; });
        Assert.AreEqual(2, attempts, "Failed migration must be retried on next call.");

        var docAfter = await sentinels.Find(Builders<BsonDocument>.Filter.Eq("_id", "test_v1")).FirstOrDefaultAsync();
        Assert.IsNotNull(docAfter, "Sentinel must be written once the retry succeeds.");
    }

    [Test]
    public async Task RunIfNeeded_DifferentMigrationIds_RunIndependently()
    {
        var repo = new MigrationsRepository(_mongoClient, _appConfig);
        var ranA = 0;
        var ranB = 0;

        await repo.RunIfNeeded("a", () => { ranA++; return Task.CompletedTask; });
        await repo.RunIfNeeded("b", () => { ranB++; return Task.CompletedTask; });
        await repo.RunIfNeeded("a", () => { ranA++; return Task.CompletedTask; });

        Assert.AreEqual(1, ranA);
        Assert.AreEqual(1, ranB);
    }
}
