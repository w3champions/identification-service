using MongoDB.Driver;
using NUnit.Framework;
using System.Threading.Tasks;
using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.Tests.Integration
{
    public class IntegrationTestBase
    {
        protected readonly IAppConfig _appConfig;
        protected readonly MongoClient MongoClient;

        public IntegrationTestBase()
        {
            _appConfig = new AppConfig();
             MongoClient = new MongoClient(_appConfig.TestsMongoConnectionString);
        }

        private string DatabaseName = "W3Champions-Update-Service-Tests";
        private MongoClient _mongoClient; 

        [SetUp]
        public async Task Setup()
        {
            await _mongoClient.DropDatabaseAsync(_appConfig.TestsDatabaseName);
        }

        protected IMongoDatabase CreateClient()
        {
            var database = _mongoClient.GetDatabase(_appConfig.TestsDatabaseName);
            return database;
        }

        protected IMongoCollection<T> CreateCollection<T>(string collectionName = null)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>((collectionName ?? typeof(T).Name));
            return mongoCollection;
        }
    }
}