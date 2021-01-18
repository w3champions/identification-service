using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using W3ChampionsIdentificationService.ActionFilters;
using W3ChampionsIdentificationService.Authorization;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.Twitch;
using W3ChampionsIdentificationService.W3CAuthentication;

namespace W3ChampionsIdentificationService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")  ?? "mongodb://176.28.16.249:3513";
            var mongoClient = new MongoClient(mongoConnectionString.Replace("'", ""));
            services.AddSingleton(mongoClient);

            services.AddTransient<IBlizzardAuthenticationService, BlizzardAuthenticationService>();
            services.AddTransient<ITwitchAuthenticationService, TwitchAuthenticationService>();
            services.AddTransient<IW3CAuthenticationService, W3CAuthenticationService>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<InjectActingPlayerFromAuthCodeFilter>();
            services.AddTransient<CheckIfUserIsAdminFilter>();

            services.AddSignalR();

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
        }
    }
}