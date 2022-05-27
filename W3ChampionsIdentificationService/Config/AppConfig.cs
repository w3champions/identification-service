using System;

namespace W3ChampionsIdentificationService.Config
{
    public class AppConfig : IAppConfig
    {
        public string MongoConnectionString
        {
            get => Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") 
                ?? "mongodb://localhost:27017"; // "mongodb://157.90.1.251:4210";
        }

        public string DatabaseName
        {
            get => "W3champions-Identification-Service";
        }

        public string TestsMongoConnectionString
        {
            get => Environment.GetEnvironmentVariable("TEST_MONGO_CONNECTION_STRING") 
                ?? "mongodb://localhost:27017";
        }

        public string TestsDatabaseName
        {
            get => "W3Champions-Identification-Service-Tests";
        }
    }
}
