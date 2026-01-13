using MongoDB.Driver;
using ManagerServer.Models;

public class MetricsCleanupService : BackgroundService
{
    private readonly IMongoDatabase _db;
    private readonly ILogger<MetricsCleanupService> _logger;

    public MetricsCleanupService(IMongoDatabase db, ILogger<MetricsCleanupService> logger)
    {
        _db = db;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var collection = _db.GetCollection<MetricsSnapshot>("metrics");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var threshold = DateTime.UtcNow.AddDays(-7);

                var filter = Builders<MetricsSnapshot>.Filter.Lt(m => m.Timestamp, threshold);

                var result = await collection.DeleteManyAsync(filter, cancellationToken: stoppingToken);

                _logger.LogInformation($"[Cleanup] Deleted {result.DeletedCount} old metrics (older than 7 days).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while clearing metrics");
            }

            await Task.Delay(TimeSpan.FromHours(8), stoppingToken);
        }
    }
}
