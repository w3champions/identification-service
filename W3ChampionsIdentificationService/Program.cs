using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace W3ChampionsIdentificationService;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning) // Filter out verbose HTTP client logs
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning) // Filter out verbose System.Net.Http logs
            .WriteTo.Console(new JsonFormatter(renderMessage: true), restrictedToMinimumLevel: LogEventLevel.Information) // Write to Console to allow log scraping
            .CreateLogger();

        var cliSubcommands = new[] { "register-client", "list-clients", "delete-client" };
        if (args.Length > 0 && Array.Exists(cliSubcommands, c => c == args[0]))
        {
            var exitCode = await W3ChampionsIdentificationService.Oidc.Cli.OidcClientCli.RunAsync(args);
            Environment.Exit(exitCode);
            return;
        }

        Log.Information("Starting Identification Service");
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}
