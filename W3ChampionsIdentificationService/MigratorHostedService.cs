using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Threading;
using W3ChampionsIdentificationService.Identity.Contracts;
using W3ChampionsIdentificationService.Migrations;
using W3ChampionsIdentificationService.RolesAndPermissions.Contracts;

namespace W3ChampionsIdentificationService;

public class MigratorHostedService(IServiceProvider serviceProvider) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _serviceProvider.GetService<IMicrosoftIdentityRepository>().CreateIndex();

        var usersRepo = _serviceProvider.GetService<IUsersRepository>();
        var migrationsRepo = _serviceProvider.GetService<IMigrationsRepository>();

        // Bump the suffix when the migration body's semantics change so the new
        // logic runs once per cluster after the deploy lands.
        await migrationsRepo.RunIfNeeded(
            "IdNormalized_v2_unicode",
            () => usersRepo.MigrateIdNormalized());
        await usersRepo.CreateIndex();
    }

    // noop
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
