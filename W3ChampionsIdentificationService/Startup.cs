using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "w3c-idp-session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = false;
        });

        // Project convention: OidcSigningKeyPem is "" in local dev (env var unset) and set in prod.
        // When set, load it for signing; when empty, fall back to OpenIddict's ephemeral
        // development certificate so `dotnet run` boots (no startup crash — matches how the
        // service tolerates missing config today).
        var appConfig = services.BuildServiceProvider().GetRequiredService<IAppConfig>();
        var hasOidcKey = !string.IsNullOrEmpty(appConfig.OidcSigningKeyPem);
        var oidcRsa = RSA.Create();
        if (hasOidcKey)
            oidcRsa.ImportFromPem(appConfig.OidcSigningKeyPem);

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
                server
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetIssuer(new Uri(appConfig.OidcIssuer));

                server
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                if (hasOidcKey)
                    server.AddSigningKey(new RsaSecurityKey(oidcRsa));
                else
                    server.AddDevelopmentSigningCertificate();

                // OpenIddict also requires an ENCRYPTION credential (authorization codes are
                // encrypted by default). A development cert works for local dev.
                // TODO(prod): supply a stable encryption key for production.
                server.AddDevelopmentEncryptionCertificate();

                server.UseAspNetCore(aspnet =>
                {
                    aspnet.EnableAuthorizationEndpointPassthrough();
                    aspnet.EnableTokenEndpointPassthrough();
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
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors(builder =>
            builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials());

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseHttpException();
        Log.Information("Application configured");
    }
}
