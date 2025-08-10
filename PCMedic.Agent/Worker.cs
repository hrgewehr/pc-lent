using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PCMedic.Agent.Services;
using PCMedic.Shared.Models;

namespace PCMedic.Agent;

public class Worker(ILogger<Worker> logger, SnapshotStore store) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        store.Current = new HealthSnapshot(DateTimeOffset.Now, "OK");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: replace with real collection logic
                store.Current = new HealthSnapshot(DateTimeOffset.Now, "OK");
                logger.LogInformation("Snapshot updated at {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating snapshot");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
