namespace OfflineXPlanner.Domain
{
    public class CostCenter
    {
        public string code { get; set; }
        public int project_id { get; set; }
        public string description { get; set; }
        public bool is_default { get; set; }
    }
}
