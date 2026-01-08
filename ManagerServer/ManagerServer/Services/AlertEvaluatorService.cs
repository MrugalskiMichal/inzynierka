using ManagerServer.Models;

public class AlertEvaluatorService
{
    private readonly MetricsRepository _metricsRepo;
    private readonly AlertRepository _alertRepo;
    private readonly EmailSender _emailSender;

    public AlertEvaluatorService(
        MetricsRepository metricsRepo,
        AlertRepository alertRepo,
        EmailSender emailSender)
    {
        _metricsRepo = metricsRepo;
        _alertRepo = alertRepo;
        _emailSender = emailSender;
    }

    public async Task EvaluateAlertsAsync()
    {
        Console.WriteLine("=== START EVALUATION ===");
        var rules = await _alertRepo.GetActiveRulesAsync();
        Console.WriteLine($"Liczba aktywnych alertów: {rules.Count}");

        foreach (var rule in rules)
        {
            var metrics = await _metricsRepo.GetMetricsForLastHour(rule.AgentId);

            if (metrics == null || metrics.Count == 0)
            {
                Console.WriteLine($"❌ No metrics for agent {rule.AgentId}");
                continue;
            }
            double? avgValue = ExtractAverageMetric(rule, metrics);
            if (avgValue == null)
                continue;
            Console.WriteLine($"✔ 1h average for {rule.MetricType}: {avgValue}");
            if (CheckCondition(avgValue.Value, rule.Operator, rule.Threshold))
            {
                if (rule.LastTriggered != null &&
                    rule.LastTriggered > DateTime.UtcNow.AddMinutes(-60))
                {
                    Console.WriteLine($"⏳ Alert {rule.MetricType} skipped (cooldown)");
                    continue;
                }

                await TriggerAlert(rule, avgValue.Value);
            }
        }
    }

    private double? ExtractAverageMetric(AlertRule rule, List<MetricsSnapshot> metrics)
    {
        switch (rule.MetricType)
        {
            case "CPU":
                return metrics.Average(m => m.CpuUsagePercent);

            case "RAM":
                return metrics
                    .Where(m => m.Ram != null)
                    .Average(m => m.Ram.RamUsageMB);

            case "DISK":
                if (rule.DiskName == null)
                    return null;

                return metrics
                    .Where(m => m.Disks != null && m.Disks.ContainsKey(rule.DiskName))
                    .Average(m => m.Disks[rule.DiskName].UsageDiskPercent);
            case "GPU":
                return metrics
                    .Where(m => m.Gpu != null && m.Gpu.Any())
                    .Average(m =>
                    {
                        var gpu = m.Gpu.Values.First();
                        return gpu.GpuCoreUsage;
                    });
            case "GPU_TEMP":
                return metrics
                    .Where(m => m.Gpu != null && m.Gpu.Any())
                    .Average(m =>
                    {
                        var gpu = m.Gpu.Values.First();
                        return gpu.GpuCoreTemperature ?? 0;
                    });
            case "GPU_MEM":
                return metrics
                    .Where(m => m.Gpu != null && m.Gpu.Any())
                    .Average(m =>
                    {
                        var gpu = m.Gpu.Values.First();
                        return gpu.GpuMemoryUsage;
                    });
            default:
                return null;
        }
    }
    private bool CheckCondition(double value, string op, double threshold)
    {
        return op switch
        {
            ">" => value > threshold,
            "<" => value < threshold,
            ">=" => value >= threshold,
            "<=" => value <= threshold,
            _ => false
        };
    }

    private async Task TriggerAlert(AlertRule rule, double value)
    {
        // Zapis do historii
        await _alertRepo.AddHistoryAsync(new AlertHistory
        {
            AlertRuleId = rule.Id,
            Timestamp = DateTime.UtcNow,
            TriggerValue = value,
            Details = $"{rule.MetricType} {rule.MetricName}"
        });

        // Wysyłka maila
        await _emailSender.SendAlertEmail(
            rule.Email,
            $"Alert: {rule.MetricType} przekroczył próg",
            $"Wartość: {value}, próg: {rule.Threshold}"
        );

        // Aktualizacja ostatniego wywołania
        rule.LastTriggered = DateTime.UtcNow;
        await _alertRepo.UpdateRuleAsync(rule);
    }
}
