using ManagerServer.Models;
using MongoDB.Driver;

public class ReportBackgroundService : BackgroundService
{
    private readonly IServiceProvider _provider;

    public ReportBackgroundService(IServiceProvider provider)
    {
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            var rules = db.GetCollection<ReportRule>("ReportRules");
            var email = scope.ServiceProvider.GetRequiredService<EmailSender>();
            var generator = scope.ServiceProvider.GetRequiredService<ReportGenerator>();

            var allRules = await rules.Find(_ => true).ToListAsync();

            foreach (var rule in allRules)
            {
                var reportData = await BuildReportData(rule, db);
                var pdf = generator.GenerateReport(reportData);

                await email.SendEmail(rule.Email, "System Report", "Attached report", pdf);

                rule.LastSent = DateTime.UtcNow;
                await rules.ReplaceOneAsync(r => r.Id == rule.Id, rule);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task<ReportData> BuildReportData(ReportRule rule, IMongoDatabase db)
    {
        var data = new ReportData();
        var snapshotsCollection = db.GetCollection<MetricsSnapshot>("metrics");

        foreach (var agentId in rule.AgentIds)
        {
            var since = DateTime.UtcNow.AddHours(-rule.IntervalHours);
            var snapshots = await snapshotsCollection
                .Find(x => x.AgentId == agentId && x.Timestamp > since)
                .SortBy(x => x.Timestamp)
                .ToListAsync();

            foreach (var metric in rule.Metrics)
            {
                var values = ExtractMetricValues(metric, snapshots);

                if (values.Count == 0)
                    continue;

                float avg = values.Average();
                float min = values.Min();
                float max = values.Max();
                float last = values.Last();

                data.Sections.Add(new ReportSection
                {
                    Title = $"{agentId} - {(
                        metric switch
                        {
                            "CPU" => "CPU",
                            "RAM" => "RAM",
                            "DISK" => "Disk",
                            "GPU" => "GPU Usage",
                            "GPU_TEMP" => "GPU Temperature",
                            "GPU_MEM" => "GPU Memory",
                            _ => metric
                        }
                    )}",
                    Average = avg,
                    Min = min,
                    Max = max,
                    Last = last
                });
            }
        }

        return data;
    }

    private List<float> ExtractMetricValues(string metric, List<MetricsSnapshot> snapshots)
    {
        switch (metric)
        {
            case "CPU":
                return snapshots
                    .Where(m => m.CpuUsagePercent.HasValue)
                    .Select(m => m.CpuUsagePercent!.Value)
                    .ToList();

            case "RAM":
                return snapshots
                    .Where(m => m.Ram != null && m.Ram.TotalRamMB > 0)
                    .Select(m => (m.Ram!.UsedRamMB / m.Ram.TotalRamMB) * 100f)
                    .ToList();

            case "DISK":
                return snapshots
                    .Where(m => m.Disks != null && m.Disks.Any())
                    .Select(m =>
                    {
                        var diskKey = m.Disks.Keys.FirstOrDefault();
                        return diskKey != null ? (float)m.Disks[diskKey].UsageDiskPercent : 0f;
                    })
                    .ToList();

            case "GPU":
                return snapshots
                    .Where(m => m.Gpu != null && m.Gpu.Any())
                    .Select(m =>
                    {
                        var gpu = m.Gpu.Values.FirstOrDefault();
                        return gpu?.GpuCoreUsage ?? 0f;
                    })
                    .ToList();

            case "GPU_TEMP":
                return snapshots
                    .Where(m => m.Gpu != null && m.Gpu.Any())
                    .Select(m =>
                    {
                        var gpu = m.Gpu.Values.FirstOrDefault();
                        return gpu?.GpuCoreTemperature ?? 0f;
                    })
                    .ToList();

            case "GPU_MEM":
                return snapshots
                    .Where(m => m.Gpu != null && m.Gpu.Any())
                    .Select(m =>
                    {
                        var gpu = m.Gpu.Values.FirstOrDefault();
                        return gpu?.GpuMemoryUsage ?? 0f;
                    })
                    .ToList();

            default:
                return new List<float>();
        }
    }
}
