namespace W3ChampionsIdentificationService.Config;

public interface IAppConfig
{
    string MongoConnectionString { get; }
    string DatabaseName { get; }
    string OidcSigningKeyPem { get; }
    string OidcEncryptionKeyPem { get; }
    string WebsiteLoginUrl { get; }
    string OidcIssuer { get; }
}
