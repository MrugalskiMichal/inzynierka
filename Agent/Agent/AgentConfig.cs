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
		public bool collectCpuData { get; set; }
		public bool collectRamData { get; set; }
		public bool collectGpuData { get; set; }
		public bool collectDiskData { get; set; }
    }
}

