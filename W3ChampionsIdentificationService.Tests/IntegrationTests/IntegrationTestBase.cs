using MongoDB.Driver;
using NUnit.Framework;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.Tests.IntegrationTests;

public class IntegrationTestBase
{
    protected readonly IAppConfig _appConfig;
    protected readonly MongoClient _mongoClient;

    public IntegrationTestBase()
    {
        _appConfig = new TestsAppConfig();
        _mongoClient = new MongoClient(_appConfig.MongoConnectionString);
    }

    [SetUp]
    public async Task Setup()
    {
        await _mongoClient.DropDatabaseAsync(_appConfig.DatabaseName);
    }

    protected IMongoDatabase CreateClient()
    {
        var database = _mongoClient.GetDatabase(_appConfig.DatabaseName);
        return database;
    }

    protected IMongoCollection<T> CreateCollection<T>(string collectionName = null)
    {
        var mongoDatabase = CreateClient();
        var mongoCollection = mongoDatabase.GetCollection<T>((collectionName ?? typeof(T).Name));
        return mongoCollection;
    }
}
