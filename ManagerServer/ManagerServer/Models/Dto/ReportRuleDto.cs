namespace ManagerServer.Models.Dto
{
    public class ReportRuleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> AgentIds { get; set; } = new();
        public List<string> Metrics { get; set; } = new();
        public int IntervalHours { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
