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
            get => "W3Champions-Identification-Service";
        }

        public string WebSiteBackendSecret
        {
            get => Environment.GetEnvironmentVariable("WEBSITE_BACKEND_TO_ID_SERVICE_SECRET");
        }
    }
}
