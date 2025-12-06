using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;
using System.Numerics;
using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Net.Http;
using LibreHardwareMonitor.Hardware;


namespace Agent
{
    internal class Program
    {
        //Generalnie na razie działa to tak, że zapisuje do metrics.json i wyświetla w konsolu co 5 sekund informacje
        //metrics.json i config.json są w ../bin/Debug/

        //LibreHardware Sensors

        static async Task Main(string[] args)
        {
            string configPath = "config.json";

            AgentConfig config = LoadConfigFromFile(configPath);

            await MainLoop(config);
        }
        static async Task MainLoop(AgentConfig config)
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            cpuCounter.NextValue();
            Thread.Sleep(1000);//po odpaleniu na chwilę skacze cpu do 100% albo do 0%, odczekać sekunde żeby działało

            //roboczo, bo na razie uznajemy że wszystko jest na true w configu
            int iteration = 0;

            float? cpuUsage = null;

            RamData? ramData = null;
            float totalRAM = GetTotalMemoryInMB();

            Dictionary<string, GpuData>? gpuData = null;
            Dictionary<string, DiskData>? diskData = null;

            while (true)
            {
                Thread.Sleep(1000 * config.collectionIntervalSeconds);
                iteration++;
                Console.WriteLine($"Agent ID: {config.agentId}; Iteration: {iteration}");

                if (config.metrics.collectCpuData)
                {
                    //Z procesorem musi być ten thread sleep na 1 sekundę jak jest show, bo inczej ten sam błąd co na początku
                    //ShowCpuUsage(cpuCounter);
                    //Thread.Sleep(1000);
                    cpuUsage = GetCPUUsage(cpuCounter);
                    Console.WriteLine("Cpu done");
                }

                if (config.metrics.collectRamData)
                {
                    ramData = GetRamUsage(ramCounter, totalRAM);
                    Console.WriteLine("ram done");
                }


                if (config.metrics.collectDiskData)
                {
                    diskData = GetDiskData();
                    Console.WriteLine("disk done");
                    if (diskData?.Count == 0)
                    {
                        diskData = null;
                    }
                }
                if (config.metrics.collectGpuData)
                {
                    gpuData = GetGpuData();
                    Console.WriteLine("gpu done");

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
                
            }
        }
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
                //Console.WriteLine($"[Hardware] {hardware.HardwareType} - {hardware.Name}\n");
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
                            //Console.WriteLine("Gpu usage loaded");
                            //Console.WriteLine($"- {sensor.SensorType}: {sensor.Name} = {sensor.Value}");
                            coreUsage = sensor.Value;
                        }
                        else if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Core")
                        {
                            //Console.WriteLine("Gpu temperature loaded");
                            //Console.WriteLine($"- {sensor.SensorType}: {sensor.Name} = {sensor.Value}");
                            coreTemperature = sensor.Value;
                        }
                        else if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Memory")
                        {
                            //Console.WriteLine("Gpu memory loaded");
                            //Console.WriteLine($"- {sensor.SensorType}: {sensor.Name} = {sensor.Value}");
                            memoryUsage = sensor.Value;
                        }

                    }

                    gpuData[hardware.Name] = new GpuData
                    {
                        gpuCoreUsage = coreUsage,
                        gpuCoreTemperature = coreTemperature,
                        gpuMemoryUsage = memoryUsage
                    };

                    //Console.WriteLine(gpuData[hardware.Name].gpuCoreUsage);
                    //Console.WriteLine(gpuData[hardware.Name].gpuCoreTemperature);
                    //Console.WriteLine(gpuData[hardware.Name].gpuMemoryUsage);
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

        //json
        static AgentConfig LoadConfigFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Configuration file not found.");
                return null;
            }

            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true //żeby przyjmowało camelCase i PascalCase
            };
            AgentConfig config = JsonSerializer.Deserialize<AgentConfig>(json, options);
            return config;
        }

        //server communication
        static async Task SendMetricsToServer(MetricsSnapshot metrics, AgentConfig config)
        {
            try
            {
                HttpClient client = new HttpClient();
               
                string url = config.serverAddress;
                string token = config.authToken;

                client.DefaultRequestHeaders.Add("X-Auth-Token", token);
               
                var response = await client.PostAsJsonAsync(url, metrics);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Metrics sent successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to send metrics. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while sending metrics: " + ex.Message);
            }
        }

        //display data in console functions

        /*
        static void ShowCpuUsage(PerformanceCounter cpuCounter)
        {
            float value = cpuCounter.NextValue();
            Console.WriteLine($"CPU usage: {value:F1}%");
        }

        static void ShowRamUsage(PerformanceCounter ramCounter, float totalRAM)
        {
            float available = ramCounter.NextValue();
            float used = totalRAM - available;
            float percentUsed = (used / totalRAM) * 100;

            Console.WriteLine($"RAM usage: {used:F0} MB / {totalRAM:F0} MB ({percentUsed:F1}%)");
        }

        static void ShowDiskUsage()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    long total = drive.TotalSize;
                    long free = drive.TotalFreeSpace;
                    long used = total - free;
                    double usagePercent = (double)used / total * 100;

                    Console.WriteLine($"{drive.Name} - {usagePercent:F2}% used");
                }
            }
        }
        */
    }
}
