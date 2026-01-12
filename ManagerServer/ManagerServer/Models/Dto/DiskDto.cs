namespace ManagerServer.Models.Dto
{
    public class DiskDto
    {
        public long UsedDiskMB { get; set; }
        public long TotalDiskMB { get; set; }
        public double UsageDiskPercent { get; set; }
    }
}
