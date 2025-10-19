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

namespace Agent
{
    internal class Program
    {
        //Generalnie na razie działa to tak, że zapisuje do metrics.json i wyświetla w konsolu co 5 sekund informacje
        //metrics.json i config.json są w ../bin/Debug/

        static void Main(string[] args)
        {
            string configPath = "config.json";

            AgentConfig config = LoadConfigFromFile(configPath);

            MainLoop(config);
        }

        static void MainLoop(AgentConfig config)
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            cpuCounter.NextValue();
            Thread.Sleep(1000);//po odpaleniu na chwilę skacze cpu do 100% albo do 0%, odczekać sekunde żeby działało

            //roboczo, bo na razie uznajemy że wszystko jest na true w configu
            int iteration = 0;

            float cpuUsage = 0f;
            float UsedRAM = 0f;
            float totalRAM = GetTotalMemoryInMB();

            Dictionary<string, DiskUsage> diskData = null;

            while (true)
            {
                Thread.Sleep(1000 * config.collectionIntervalSeconds);
                iteration++;
                Console.WriteLine($"Agent ID: {config.agentId}; Iteration: {iteration}");

                if (config.metrics.cpuUsage)
                {
                    //Z procesorem musi być ten thread sleep na 1 sekundę jak jest show, bo inczej ten sam błąd co na początku
                    //ShowCpuUsage(cpuCounter);
                    //Thread.Sleep(1000);
                    cpuUsage = GetCPUUsage(cpuCounter);
                }

                if (config.metrics.ramUsage)
                {
                    ShowRamUsage(ramCounter, totalRAM);
                    Vector2 ramData = GetRamUsage(ramCounter, totalRAM);
                    //na razie nie używamy procentowych warotści, potem może dodam
                    UsedRAM = ramData.X;
                }


                if (config.metrics.diskUsage)
                {
                    ShowDiskUsage();
                    diskData = GetDiskData();
                }


                var snapshot = new MetricsSnapshot
                {
                    AgentId = config.agentId,
                    Timestamp = DateTime.UtcNow,
                    CpuUsagePercent = cpuUsage,
                    UsedRamMB = UsedRAM,
                    TotalRamMB = totalRAM,
                    Disks = diskData,
                };

                SaveMetricsToFile(snapshot, "metrics.json");
            }
        }
        static void ShowCpuUsage(PerformanceCounter cpuCounter)
        {
            float value = cpuCounter.NextValue();
            Console.WriteLine($"CPU usage: {value:F1}%");
        }

        static float GetCPUUsage(PerformanceCounter cpuCounter)
        {
            return cpuCounter.NextValue();
        }
        static void ShowRamUsage(PerformanceCounter ramCounter, float totalRAM)
        {
            float available = ramCounter.NextValue();
            float used = totalRAM - available;
            float percentUsed = (used / totalRAM) * 100;

            Console.WriteLine($"RAM usage: {used:F0} MB / {totalRAM:F0} MB ({percentUsed:F1}%)");
        }
        static Vector2 GetRamUsage(PerformanceCounter ramCounter, float totalRAM)
        {
            float available = ramCounter.NextValue();
            float used = totalRAM - available;
            float percentUsed = (used / totalRAM) * 100;

            Vector2 ramData = new Vector2(used, percentUsed);

            return ramData;
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

        static Dictionary<string, DiskUsage> GetDiskData()
        {
            Dictionary<string, DiskUsage> diskData = new Dictionary<string, DiskUsage>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    long total = drive.TotalSize;
                    long free = drive.TotalFreeSpace;
                    long used = total - free;
                    double usagePercent = (double)used / total * 100;

                    diskData[drive.Name] = new DiskUsage
                    {
                        UsedDiskMB = used,
                        TotalDiskMB = total,
                        UsageDiskPercent = usagePercent
                    };
                }
            }
            return diskData;
        }

        //On start/WMI data
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

        static void SaveMetricsToFile(MetricsSnapshot metrics, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true //dla czytelności
            };

            string json = JsonSerializer.Serialize(metrics, options);
            File.WriteAllText(filePath, json);

            Console.WriteLine($"Saved in file: {filePath}");
        }

    }
}
