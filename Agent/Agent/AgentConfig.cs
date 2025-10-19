using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public class AgentConfig
    {
		public string agentId {  get; set; }
		public string authToken { get; set; }
		public string serverAddress { get; set; }
		public int collectionIntervalSeconds { get; set; }
		public MetricsConfig metrics { get; set; }
    }
    public class MetricsConfig
    {
		public bool cpuUsage { get; set; }
		public bool cpuTemperature { get; set; }
		public bool ramUsage { get; set; }
		public bool gpuUsage { get; set; }
		public bool gpuTemperature { get; set; }
		public bool diskUsage { get; set; }
		public bool fanSpeeds {  get; set; }
    }
}

