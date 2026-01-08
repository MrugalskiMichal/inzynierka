using Microsoft.Extensions.Hosting;

public class AlertBackgroundService : BackgroundService
{
    private readonly AlertEvaluatorService _alertEvaluator;
    private readonly ILogger<AlertBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public AlertBackgroundService(
        AlertEvaluatorService alertEvaluator,
        ILogger<AlertBackgroundService> logger,
        IConfiguration config)
    {
        _alertEvaluator = alertEvaluator;
        _logger = logger;

        // Interwał w sekundach z appsettings.json
        int seconds = int.Parse(config["Alerts:CheckIntervalSeconds"] ?? "30");
        _interval = TimeSpan.FromSeconds(seconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _alertEvaluator.EvaluateAlertsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while evaluating alerts");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("AlertBackgroundService stopped");
    }
}
