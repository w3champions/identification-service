using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using W3ChampionsIdentificationService.Config;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace W3ChampionsIdentificationService.Oidc.Cli;

public static class OidcClientCli
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
            return Usage();

        var subcommand = args[0];
        using var host = BuildCliHost();
        try
        {
            await host.StartAsync();
            var manager = host.Services.GetRequiredService<IOpenIddictApplicationManager>();
            return subcommand switch
            {
                "register-client" => await RegisterClient(args, manager),
                "list-clients" => await ListClients(manager),
                "delete-client" => await DeleteClient(args, manager),
                _ => Usage()
            };
        }
        catch (Exception ex)
        {
            // Surface transient/operational failures (Mongo unreachable, duplicate-key,
            // descriptor validation, missing-arg) as a clean stderr line + nonzero exit
            // instead of an unhandled stack dump.
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
        finally
        {
            await host.StopAsync();
        }
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

    /// <summary>Parsed + validated register-client arguments. Public for testability.</summary>
    public sealed record RegisterArgs(string ClientId, string RedirectUri, string[] Scopes, bool Update);

    /// <summary>
    /// Parses and validates the register-client argument vector. Throws <see cref="ArgumentException"/>
    /// on a missing value (e.g. a trailing valueless flag) or a missing/invalid required argument;
    /// the top-level handler in <see cref="RunAsync"/> turns that into a clean stderr line + exit 1.
    /// Public for testability.
    /// </summary>
    public static RegisterArgs ParseRegisterArgs(string[] args)
    {
        string clientId = null, redirectUri = null;
        string[] scopes = ["openid", "profile", "email"];
        bool update = false;

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--client-id": clientId = NextValue(args, ref i); break;
                case "--redirect-uri": redirectUri = NextValue(args, ref i); break;
                case "--scopes": scopes = NextValue(args, ref i).Split(','); break;
                case "--update": update = true; break;
            }
        }

        if (string.IsNullOrEmpty(clientId)) throw new ArgumentException("--client-id is required");
        if (string.IsNullOrEmpty(redirectUri)) throw new ArgumentException("--redirect-uri is required");
        if (!IsValidHttpsRedirectUri(redirectUri))
            throw new ArgumentException($"redirect-uri must be an absolute HTTPS URL, got: {redirectUri}");

        return new RegisterArgs(clientId, redirectUri, scopes, update);
    }

    /// <summary>
    /// Returns the value following the flag at <paramref name="i"/>, advancing the index.
    /// Throws <see cref="ArgumentException"/> when the flag is the last token (no value) so the
    /// CLI fails cleanly via the top-level handler instead of an IndexOutOfRangeException dump.
    /// </summary>
    private static string NextValue(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"{args[i]} requires a value");
        return args[++i];
    }

    private static async Task<int> RegisterClient(string[] args, IOpenIddictApplicationManager manager)
    {
        var parsed = ParseRegisterArgs(args);

        var existing = await manager.FindByClientIdAsync(parsed.ClientId);
        if (existing != null && !parsed.Update)
        {
            Console.Error.WriteLine($"ERROR: Client '{parsed.ClientId}' already exists. Re-run with --update to overwrite.");
            return 1;
        }

        var rawSecret = GenerateSecret();

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = parsed.ClientId,
            ClientSecret = rawSecret,       // OpenIddict hashes this via PBKDF2 internally.
            ClientType = ClientTypes.Confidential,
            // Implicit consent: Quackback is a first-party trusted client, so no consent
            // screen is shown (the IdP has no consent UI to render one).
            ConsentType = ConsentTypes.Implicit,
            Permissions =
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
            RedirectUris = { new Uri(parsed.RedirectUri) },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
        };

        if (!parsed.Scopes.Contains("profile", StringComparer.OrdinalIgnoreCase))
            descriptor.Permissions.Remove(Permissions.Scopes.Profile);
        if (!parsed.Scopes.Contains("email", StringComparer.OrdinalIgnoreCase))
            descriptor.Permissions.Remove(Permissions.Scopes.Email);

        if (existing != null && parsed.Update)
        {
            await manager.UpdateAsync(existing, descriptor);
            Console.Error.WriteLine("NOTE: --update rotated the client secret; update the downstream client config.");
        }
        else
        {
            await manager.CreateAsync(descriptor);
        }

        Console.WriteLine($"Client '{parsed.ClientId}' registered successfully.");
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
            if (args[i] == "--client-id") clientId = NextValue(args, ref i);

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
        Console.Error.WriteLine("Usage: register-client --client-id <id> --redirect-uri <uri> [--scopes openid,profile,email] [--update rotates the secret]");
        Console.Error.WriteLine("       list-clients");
        Console.Error.WriteLine("       delete-client --client-id <id>");
        return 1;
    }

    private static IHost BuildCliHost() =>
        Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                // Quiet the default console logging provider + host-lifetime Information logs:
                // they write to stdout — the same stream as the once-only CLIENT_SECRET line —
                // so an operator could mis-copy. Errors are surfaced via Console.Error in RunAsync.
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Warning);
            })
            .ConfigureServices((_, services) =>
            {
                var appConfig = new AppConfig();
                services.AddSingleton<IAppConfig>(appConfig);
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
