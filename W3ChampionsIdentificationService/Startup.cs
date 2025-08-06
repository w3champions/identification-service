using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Net;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Config;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Identity.Repositories;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.Middleware;
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
        Log.Information("Services configured");
    }

    public void Configure(IApplicationBuilder app)
    {
        Log.Information("Configuring application");
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            KnownNetworks = { new IPNetwork(IPAddress.Parse("172.18.0.0"), 16) } // Docker network
        });
        app.UseRouting();
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
