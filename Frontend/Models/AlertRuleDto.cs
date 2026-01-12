namespace Frontend.Models
{
    public class AlertRuleDto
    {
        public string Id { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public double Threshold { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? DiskName { get; set; }
    }
}
