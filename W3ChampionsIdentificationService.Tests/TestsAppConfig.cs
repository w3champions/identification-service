using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.Tests
{
    public class TestsAppConfig : IAppConfig
    {
        public string MongoConnectionString => "mongodb://157.90.1.251:3712"; // "mongodb://localhost:27017";

        public string DatabaseName => "W3Champions-Identification-Service-Tests";
    }
}
