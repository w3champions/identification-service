using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using System;
using System.Net;
using System.Security.Cryptography;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Config;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Identity.Repositories;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.Middleware;
using W3ChampionsIdentificationService.Migrations;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.RolesAndPermissions.Repositories;
using W3ChampionsIdentificationService.Twitch;
using W3ChampionsIdentificationService.W3CAuthentication;
using W3ChampionsIdentificationService.DataProtection;
using W3ChampionsIdentificationService.Oidc;
using W3ChampionsIdentificationService.W3CAuthentication.Contracts;
using W3ChampionsIdentificationService.WebApi.ActionFilters;
using Serilog;

namespace W3ChampionsIdentificationService;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        Log.Information("Configuring services");
        services.AddControllers();

        services.AddSingleton<IAppConfig, AppConfig>();

        services.AddSingleton((x) =>
        {
            var appConfig = x.GetService<IAppConfig>();
            return new MongoClient(appConfig.MongoConnectionString);
        });

        services.AddTransient<IPermissionsRepository, PermissionsRepository>();
        services.AddTransient<IRolesRepository, RolesRepository>();
        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<IMicrosoftIdentityRepository, MicrosoftIdentityRepository>();
        services.AddTransient<IMigrationsRepository, MigrationsRepository>();

        services.AddTransient<IPermissionsCommandHandler, PermissionsCommandHandler>();
        services.AddTransient<IRolesCommandHandler, RolesCommandHandler>();
        services.AddTransient<IUsersCommandHandler, UsersCommandHandler>();

        services.AddTransient<RolesAndPermissionsValidator, RolesAndPermissionsValidator>();

        services.AddTransient<IBlizzardAuthenticationService, BlizzardAuthenticationService>();
        services.AddTransient<ITwitchAuthenticationService, TwitchAuthenticationService>();
        services.AddTransient<IMicrosoftAuthenticationService, MicrosoftAuthenticationService>();
        services.AddTransient<IW3CAuthenticationService, W3CAuthenticationService>();

        services.AddTransient<HasPermissionsPermissionFilter>();

        services.AddHostedService<MigratorHostedService>();

        // Persist + share the DataProtection key ring in MongoDB. The IdP session cookie below
        // is encrypted with DataProtection, whose default key ring is EPHEMERAL and per-instance:
        // a cookie set by one replica (or before a restart) cannot be decrypted by another, which
        // would loop the user back through login mid-handoff. Backing the key ring with the
        // already-present MongoDB makes it durable + shared with no new infrastructure.
        // SetApplicationName MUST be a stable constant so every replica/restart derives the same
        // protection purpose. (Distinct from OIDC_ENCRYPTION_KEY_PEM, which protects OIDC codes.)
        var dataProtectionConfig = new AppConfig();
        var dataProtectionKeysCollection =
            new MongoClient(dataProtectionConfig.MongoConnectionString)
                .GetDatabase(dataProtectionConfig.DatabaseName)
                .GetCollection<BsonDocument>("DataProtectionKeys");
        services.AddDataProtection()
            .SetApplicationName("w3c-identification-service");
        services.Configure<KeyManagementOptions>(options =>
            options.XmlRepository = new MongoXmlRepository(dataProtectionKeysCollection));

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            // __Host- prefix: browsers enforce that such cookies are Secure, have Path="/",
            // and carry no Domain — i.e. strictly host-only. The settings below satisfy that
            // (Secure=Always, default Path="/", no Cookie.Domain), hardening against cookie
            // fixation/subdomain injection. The cookie is read/written via the auth scheme,
            // so the raw name change is transparent to the rest of the code.
            options.Cookie.Name = "__Host-w3c-idp-session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = false;
        });

        // Project convention: OIDC key PEMs are "" in local dev (env vars unset) and set in prod.
        // When set, they are loaded as the signing/encryption credentials; when empty, OpenIddict's
        // ephemeral development certificates are used so `dotnet run` boots (no startup crash —
        // matches how the service tolerates missing config today).
        // AppConfig is a stateless env-var reader with a parameterless ctor, so instantiate it
        // directly rather than building a throwaway service provider (avoids ASP0000 + a duplicate
        // DI container).
        var appConfig = new AppConfig();
        var hasOidcKey = !string.IsNullOrEmpty(appConfig.OidcSigningKeyPem);
        var hasOidcEncKey = !string.IsNullOrEmpty(appConfig.OidcEncryptionKeyPem);

        if (!Uri.TryCreate(appConfig.OidcIssuer, UriKind.Absolute, out var issuerUri))
            throw new InvalidOperationException($"OIDC_ISSUER is not a valid absolute URI: '{appConfig.OidcIssuer}'");

        // Warn when running with a production signing key but the issuer is not among the
        // handoff return-URL allowlist origins. In that case the SSO handoff will 400
        // (HandoffReturnUrlValidator rejects the authorize return URL) mid-flow.
        // Local dev intentionally uses a localhost issuer and never exercises the handoff,
        // so this is a prod-only warning (gated on hasOidcKey = the prod signal).
        if (hasOidcKey && !HandoffReturnUrlValidator.IsAllowed(appConfig.OidcIssuer))
            Log.Warning(
                "OIDC_ISSUER '{Issuer}' is not in the handoff return-URL allowlist [{Allowed}]; " +
                "the SSO handoff will reject the authorize return URL with HTTP 400. " +
                "Set OIDC_ISSUER to one of the allowed origins.",
                appConfig.OidcIssuer, string.Join(", ", HandoffReturnUrlValidator.AllowedOrigins));

        services.AddOpenIddict()
            .AddCore(core =>
            {
                core.UseMongoDb(options =>
                {
                    options.UseDatabase(
                        new MongoClient(appConfig.MongoConnectionString)
                            .GetDatabase(appConfig.DatabaseName));
                });
            })
            .AddServer(server =>
            {
                // These /connect/* endpoints are the NEW standards-compliant OIDC surface.
                // They are distinct from the legacy bespoke /api/oauth/* endpoints in
                // AuthorizationController (e.g. OIDC "/connect/userinfo" vs legacy "/api/oauth/user-info").
                server
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetIssuer(issuerUri);

                server
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                // Register the optional scopes the IdP supports so OpenIddict recognizes them
                // (unregistered scopes are rejected) and discovery advertises them. `openid`
                // is implicit and must not be registered here.
                server.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile);

                // Signing credential: prod uses the configured RSA key; local dev (no key) uses an
                // ephemeral development certificate so the service boots without the secret.
                if (hasOidcKey)
                {
                    var signingRsa = RSA.Create();
                    signingRsa.ImportFromPem(appConfig.OidcSigningKeyPem);
                    server.AddSigningKey(new RsaSecurityKey(signingRsa));
                }
                else
                {
                    server.AddDevelopmentSigningCertificate();
                }

                // Encryption credential (OpenIddict encrypts authorization codes by default).
                if (hasOidcEncKey)
                {
                    var encryptionRsa = RSA.Create();
                    encryptionRsa.ImportFromPem(appConfig.OidcEncryptionKeyPem);
                    server.AddEncryptionKey(new RsaSecurityKey(encryptionRsa));
                }
                else if (hasOidcKey)
                {
                    // Production (signing key set) MUST have a stable encryption key, else auth codes are
                    // encrypted with an ephemeral dev cert that breaks across restarts/replicas. Fail fast.
                    throw new InvalidOperationException(
                        "OIDC_ENCRYPTION_KEY_PEM must be set when OIDC_SIGNING_KEY_PEM is set (production). " +
                        "Generate one with: openssl genrsa 2048");
                }
                else
                {
                    // Local dev (no signing key): ephemeral dev encryption cert is fine.
                    server.AddDevelopmentEncryptionCertificate();
                }

                server.UseAspNetCore(aspnet =>
                {
                    aspnet.EnableAuthorizationEndpointPassthrough();
                    // Token endpoint is NOT in passthrough — OpenIddict handles /connect/token
                    // internally. Passthrough is only needed when a custom controller produces
                    // the token response; we have no such controller, so letting OpenIddict's
                    // built-in handler process the authorization-code exchange is both correct
                    // and sufficient (it validates the code, re-emits the claims+destinations
                    // stored by OidcAuthorizeController.SignIn, and mints access/id tokens).
                    aspnet.EnableUserInfoEndpointPassthrough();
                });
            })
            .AddValidation(validation =>
            {
                validation.UseLocalServer();
                validation.UseAspNetCore();
            });

        Log.Information("Services configured");
    }

    public void Configure(IApplicationBuilder app)
    {
        Log.Information("Configuring application");
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            KnownNetworks = { new IPNetwork(IPAddress.Parse("172.18.0.0"), 16) }, // Docker network
            KnownProxies = { IPAddress.Parse("212.60.5.180") } // Russia gateway
        });
        app.UseRouting();
        // CORS must run BEFORE authentication: for endpoints OpenIddict completes inside
        // UseAuthentication() (notably /connect/token, handled in-middleware since token
        // passthrough is disabled), the response is produced before any later middleware — so
        // CORS has to be in front of UseAuthentication to attach Access-Control-Allow-Origin,
        // otherwise browser-based OIDC clients are blocked. Policy itself is unchanged; only moved.
        app.UseCors(builder =>
            builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials());
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseHttpException();
        Log.Information("Application configured");
    }
}
