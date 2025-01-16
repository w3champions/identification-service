namespace W3ChampionsIdentificationService.Config;

public interface IAppConfig
{
    string MongoConnectionString { get; }
    string DatabaseName { get; }
}
