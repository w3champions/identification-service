using System;

namespace W3ChampionsIdentificationService.Config
{
    public class AppConfig : IAppConfig
    {
        public string MongoConnectionString
        {
            get => Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? "mongodb://localhost:27017";
        }

        public string DatabaseName
        {
            get => "W3champions-Identification-Service";
        }
    }
}
