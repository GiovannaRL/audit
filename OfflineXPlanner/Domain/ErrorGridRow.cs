using OfflineXPlanner.Domain.Enums;

namespace OfflineXPlanner.Domain
{
    public class ErrorGridRow
    {
        public ExportItemStatusEnum Status { get; set; }
        public string StatusComment { get; set; }
        public string Code { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string ModelNumber { get; set; }
        public string ModelName { get; set; }
        public string JSN { get; set; }
        public string Phase { get; set; }
        public string Department { get; set; }
        public string Room { get; set; }
        public string Resp { get; set; }
        public string U1 { get; set; }
        public string U2 { get; set; }
        public string U3 { get; set; }
        public string U4 { get; set; }
        public string U5 { get; set; }
        public string U6 { get; set; }
    }
}
