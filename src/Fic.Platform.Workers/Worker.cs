namespace Fic.Platform.Workers;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var useRedis = configuration.GetValue("Features:UseRedis", false);
        var useExternalEvents = configuration.GetValue("Features:UseExternalEvents", false);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Projection worker heartbeat at {time}. redis_enabled={useRedis} external_events_enabled={useExternalEvents}",
                    DateTimeOffset.Now,
                    useRedis,
                    useExternalEvents);
            }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
