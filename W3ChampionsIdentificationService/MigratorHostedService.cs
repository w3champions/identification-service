using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Threading;
using W3ChampionsIdentificationService.Identity.Contracts;
using static System.Formats.Asn1.AsnWriter;

namespace W3ChampionsIdentificationService;

public class MigratorHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    public MigratorHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _serviceProvider.GetService<IMicrosoftIdentityRepository>().CreateIndex();
    }

    // noop
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
