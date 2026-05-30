using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using W3ChampionsIdentificationService.Config;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace W3ChampionsIdentificationService.Oidc.Cli;

public static class OidcClientCli
{
    public static async Task<int> RunAsync(string[] args)
    {
        var subcommand = args[0];
        using var host = BuildCliHost(args);
        await host.StartAsync();

        var manager = host.Services.GetRequiredService<IOpenIddictApplicationManager>();

        int exitCode = subcommand switch
        {
            "register-client" => await RegisterClient(args, manager),
            "list-clients"    => await ListClients(manager),
            "delete-client"   => await DeleteClient(args, manager),
            _ => Usage()
        };

        await host.StopAsync();
        return exitCode;
    }

    /// <summary>Generates a 256-bit random client secret as base64 (44 chars). Public for testability.</summary>
    public static string GenerateSecret()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>True if the redirect URI is an absolute HTTPS URL. Public for testability.</summary>
    public static bool IsValidHttpsRedirectUri(string uri) =>
        Uri.TryCreate(uri, UriKind.Absolute, out var parsed) &&
        string.Equals(parsed.Scheme, "https", StringComparison.OrdinalIgnoreCase);

    private static async Task<int> RegisterClient(string[] args, IOpenIddictApplicationManager manager)
    {
        string clientId = null, redirectUri = null;
        string[] scopes = ["openid", "profile", "email"];
        bool update = false;

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--client-id":    clientId    = args[++i]; break;
                case "--redirect-uri": redirectUri = args[++i]; break;
                case "--scopes":       scopes      = args[++i].Split(','); break;
                case "--update":       update      = true; break;
            }
        }

        if (string.IsNullOrEmpty(clientId))
        {
            Console.Error.WriteLine("ERROR: --client-id is required");
            return 1;
        }
        if (string.IsNullOrEmpty(redirectUri))
        {
            Console.Error.WriteLine("ERROR: --redirect-uri is required");
            return 1;
        }
        if (!IsValidHttpsRedirectUri(redirectUri))
        {
            Console.Error.WriteLine($"ERROR: redirect-uri must be an absolute HTTPS URL, got: {redirectUri}");
            return 1;
        }

        var existing = await manager.FindByClientIdAsync(clientId);
        if (existing != null && !update)
        {
            Console.Error.WriteLine($"ERROR: Client '{clientId}' already exists. Re-run with --update to overwrite.");
            return 1;
        }

        var rawSecret = GenerateSecret();

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId     = clientId,
            ClientSecret = rawSecret,       // OpenIddict hashes this via PBKDF2 internally.
            ClientType   = ClientTypes.Confidential,
            ConsentType  = ConsentTypes.Implicit,
            Permissions  =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code,
                // Note: OpenIddict 7.5.0 does not have Permissions.Scopes.OpenId —
                // the 'openid' scope is implicitly handled by the framework.
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
            },
            RedirectUris = { new Uri(redirectUri) },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
        };

        if (!scopes.Contains("profile", StringComparer.OrdinalIgnoreCase))
            descriptor.Permissions.Remove(Permissions.Scopes.Profile);
        if (!scopes.Contains("email",   StringComparer.OrdinalIgnoreCase))
            descriptor.Permissions.Remove(Permissions.Scopes.Email);

        if (existing != null && update)
            await manager.DeleteAsync(existing);

        await manager.CreateAsync(descriptor);

        Console.WriteLine($"Client '{clientId}' registered successfully.");
        Console.WriteLine($"CLIENT_SECRET (copy now — will not be shown again): {rawSecret}");
        return 0;
    }

    private static async Task<int> ListClients(IOpenIddictApplicationManager manager)
    {
        var count = 0;
        await foreach (var app in manager.ListAsync())
        {
            var id = await manager.GetClientIdAsync(app);
            Console.WriteLine($"  {id}");
            count++;
        }
        Console.WriteLine($"Total: {count} client(s)");
        return 0;
    }

    private static async Task<int> DeleteClient(string[] args, IOpenIddictApplicationManager manager)
    {
        string clientId = null;
        for (int i = 1; i < args.Length; i++)
            if (args[i] == "--client-id") clientId = args[++i];

        if (string.IsNullOrEmpty(clientId))
        {
            Console.Error.WriteLine("ERROR: --client-id is required");
            return 1;
        }

        var existing = await manager.FindByClientIdAsync(clientId);
        if (existing == null)
        {
            Console.Error.WriteLine($"ERROR: Client '{clientId}' not found");
            return 1;
        }

        await manager.DeleteAsync(existing);
        Console.WriteLine($"Client '{clientId}' deleted.");
        return 0;
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage: register-client --client-id <id> --redirect-uri <uri> [--scopes openid,profile,email] [--update]");
        Console.Error.WriteLine("       list-clients");
        Console.Error.WriteLine("       delete-client --client-id <id>");
        return 1;
    }

    private static IHost BuildCliHost(string[] args) =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                var appConfig = new AppConfig();
                services.AddSingleton<IAppConfig>(appConfig);
                services.AddSingleton(_ => new MongoClient(appConfig.MongoConnectionString));
                services.AddOpenIddict()
                    .AddCore(core =>
                    {
                        core.UseMongoDb(options =>
                            options.UseDatabase(
                                new MongoClient(appConfig.MongoConnectionString)
                                    .GetDatabase(appConfig.DatabaseName)));
                    });
            })
            .Build();
}
