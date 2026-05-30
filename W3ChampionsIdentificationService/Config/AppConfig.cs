using System;
using System.Text.RegularExpressions;

namespace W3ChampionsIdentificationService.Config;

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

    public string OidcSigningKeyPem
        => Regex.Unescape(Environment.GetEnvironmentVariable("OIDC_SIGNING_KEY_PEM") ?? "");

    public string OidcEncryptionKeyPem
        => Regex.Unescape(Environment.GetEnvironmentVariable("OIDC_ENCRYPTION_KEY_PEM") ?? "");

    public string WebsiteLoginUrl
        => Environment.GetEnvironmentVariable("WEBSITE_LOGIN_URL") ?? "https://localhost:3000/sso-continue";

    public string OidcIssuer
        => Environment.GetEnvironmentVariable("OIDC_ISSUER") ?? "https://localhost:5050";
}
