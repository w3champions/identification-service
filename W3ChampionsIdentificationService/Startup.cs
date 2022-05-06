using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using W3ChampionsIdentificationService.Blizzard;
using W3ChampionsIdentificationService.RolesAndPermissions;
using W3ChampionsIdentificationService.Twitch;
using W3ChampionsIdentificationService.W3CAuthentication;
using W3ChampionsIdentificationService.W3CAuthentication.Contracts;
using W3ChampionsIdentificationService.WebApi.ActionFilters;

namespace W3ChampionsIdentificationService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? "mongodb://localhost:27017"; // "mongodb://157.90.1.251:3710"; 
            var mongoClient = new MongoClient(mongoConnectionString.Replace("'", ""));
            services.AddSingleton(mongoClient);

            services.AddTransient<IRolesRepository, RolesRepository>();
            services.AddTransient<IBlizzardAuthenticationService, BlizzardAuthenticationService>();
            services.AddTransient<ITwitchAuthenticationService, TwitchAuthenticationService>();
            services.AddTransient<IW3CAuthenticationService, W3CAuthenticationService>();

            services.AddTransient<CheckIfSuperAdminFilter>();
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
}