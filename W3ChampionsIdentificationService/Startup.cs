using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Config;
using W3ChampionsIdentificationService.Identity;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Microsoft;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.RolesAndPermissions.CommandHandlers;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;
using W3ChampionsIdentificationService.Twitch;
using W3ChampionsIdentificationService.W3CAuthentication.Contracts;
using W3ChampionsIdentificationService.WebApi.ActionFilters;

namespace W3ChampionsIdentificationService;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
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
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
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
    }
}
