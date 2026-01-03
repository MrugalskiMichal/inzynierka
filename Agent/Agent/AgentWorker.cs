using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Agent
{
    public  class AgentWorker : BackgroundService
    {
        private readonly HttpClient _httpClient;
        public AgentWorker(HttpClient httpClient)
        {
            // dodawanie klienta przez Dependency Injection
            _httpClient = httpClient;
        }

        // Main loop
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //wczytanie pliku config
            string configPath = "config.json";
            AgentConfig config = LoadConfigFromFile(configPath);

            Logger.Info("Initializing values...");

            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            cpuCounter.NextValue();
            await Task.Delay(1000, stoppingToken);// po odpaleniu na chwilę skacze cpu do 100% albo do 0%, odczekać sekunde żeby działało

            float? cpuUsage = null;

            RamData? ramData = null;
            float totalRAM = GetTotalMemoryInMB();

            Dictionary<string, GpuData>? gpuData = null;
            Dictionary<string, DiskData>? diskData = null;

            Logger.Info("Values initialized");

            while(!stoppingToken.IsCancellationRequested)
            {
                var startTime = DateTime.UtcNow;


                if (config.metrics.collectCpuData)
                {
                    cpuUsage = GetCPUUsage(cpuCounter);
                }

                if (config.metrics.collectRamData)
                {
                    ramData = GetRamUsage(ramCounter, totalRAM);
                }

                if (config.metrics.collectDiskData)
                {
                    diskData = GetDiskData();
                    if (diskData?.Count == 0)
                    {
                        diskData = null;
                    }
                }

                if (config.metrics.collectGpuData)
                {
                    gpuData = GetGpuData();

                    if (gpuData?.Count == 0)
                    {
                        gpuData = null;
                    }
                }

                var snapshot = new MetricsSnapshot
                {
                    agentId = config.agentId,
                    timestamp = DateTime.UtcNow,
                    cpuUsagePercent = cpuUsage,
                    ram = ramData,
                    disks = diskData,
                    gpu = gpuData
                };

                await SendMetricsToServer(snapshot, config);

                // ile mineło
                var elapsed = DateTime.UtcNow - startTime;
                var delay = TimeSpan.FromSeconds(config.collectionIntervalSeconds) - elapsed;

                if (delay.TotalMilliseconds > 0)
                    await Task.Delay(delay, stoppingToken);
                else
                    Logger.Warning($"Iteration took longer ({elapsed.TotalSeconds:F2}s) than configured interval ({config.collectionIntervalSeconds}s).");
            }

            Logger.Info("Application stopped");
        }

        // Get Values

        static float GetCPUUsage(PerformanceCounter cpuCounter)
        {
            return cpuCounter.NextValue();
        }

        static RamData GetRamUsage(PerformanceCounter ramCounter, float totalRAM)
        {
            float available = ramCounter.NextValue();
            float usedRam = totalRAM - available;

            RamData ramData = new RamData(usedRam, totalRAM);

            return ramData;
        }
        static Dictionary<string, DiskData> GetDiskData()
        {
            Dictionary<string, DiskData> diskData = new Dictionary<string, DiskData>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    long total = drive.TotalSize;
                    long free = drive.TotalFreeSpace;
                    long used = total - free;
                    double usagePercent = (double)used / total * 100;

                    diskData[drive.Name] = new DiskData
                    {
                        usedDiskMB = used,
                        totalDiskMB = total,
                        usageDiskPercent = usagePercent
                    };
                }
            }
            return diskData;
        }
        static Dictionary<string, GpuData> GetGpuData()
        {
            Dictionary<string, GpuData>? gpuData = new Dictionary<string, GpuData>();

            var computer = new Computer
            {
                IsGpuEnabled = true,
            };
            computer.Open();

            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    hardware.Update();

                    float? coreUsage = null;
                    float? coreTemperature = null;
                    float? memoryUsage = null;

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                        {
                            coreUsage = sensor.Value;
                        }
                        else if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Core")
                        {
                            coreTemperature = sensor.Value;
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Memory")
                        {
                            memoryUsage = sensor.Value;
                        }

                    }

                    gpuData[hardware.Name] = new GpuData
                    {
                        gpuCoreUsage = coreUsage,
                        gpuCoreTemperature = coreTemperature,
                        gpuMemoryUsage = memoryUsage
                    };
                }
            }

            return gpuData;
        }

        //On start

        //WMI data
        static float GetTotalMemoryInMB()
        {
            var searcher = new System.Management.ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");

            foreach (var obj in searcher.Get())
            {
                ulong kilobytes = (ulong)obj["TotalVisibleMemorySize"];
                return kilobytes / 1024f;
            }

            return 0;
        }
        // load configuration
        static AgentConfig LoadConfigFromFile(string path)
        {
            Logger.Info("Loading configuration...");

            if (!File.Exists(path))
            {
                Logger.Error("Configuration file not found.");
                return null;
            }

            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true //żeby przyjmowało camelCase i PascalCase
            };
            AgentConfig config = JsonSerializer.Deserialize<AgentConfig>(json, options);

            Logger.Info("Configuration loaded succesfully");

            return config;
        }

        // server communication
        private async Task SendMetricsToServer(MetricsSnapshot metrics, AgentConfig config)
        {
            try
            {
               
                string url = config.serverAddress;
                string token = config.authToken;

                _httpClient.DefaultRequestHeaders.Remove("X-Auth-Token");// usuń stary token
                _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", token);// dodaj nowy

                var response = await _httpClient.PostAsJsonAsync(url, metrics);

                if (response.IsSuccessStatusCode)
                {
                    Logger.Info("Metrics sent successfully.");
                }
                else
                {
                    Logger.Error($"Failed to send metrics. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error while sending metrics: " + ex.Message);
            }
        }

    }
}
