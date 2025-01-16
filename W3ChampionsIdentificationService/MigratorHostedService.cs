using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Threading;
using W3ChampionsIdentificationService.Identity.Contracts;

namespace W3ChampionsIdentificationService;

public class MigratorHostedService(IServiceProvider serviceProvider) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _serviceProvider.GetService<IMicrosoftIdentityRepository>().CreateIndex();
    }

    // noop
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
