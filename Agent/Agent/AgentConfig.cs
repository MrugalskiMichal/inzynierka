using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public class AgentConfig
    {
		public required string agentId {  get; set; }
		public required string authToken { get; set; }
		public required string serverAddress { get; set; }
		public int collectionIntervalSeconds { get; set; }
		public required MetricsConfig metrics { get; set; }
    }
    public class MetricsConfig
    {
		public bool collectCpuData { get; set; }
		public bool collectRamData { get; set; }
		public bool collectGpuData { get; set; }
		public bool collectDiskData { get; set; }
    }
}

