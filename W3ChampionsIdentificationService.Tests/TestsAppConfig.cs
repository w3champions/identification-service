using W3ChampionsIdentificationService.Config;

namespace W3ChampionsIdentificationService.Tests
{
    public class TestsAppConfig : IAppConfig
    {
        public string MongoConnectionString => "mongodb://localhost:27017";

        public string DatabaseName => "W3champions-Identification-Service-Tests";
    }
}
