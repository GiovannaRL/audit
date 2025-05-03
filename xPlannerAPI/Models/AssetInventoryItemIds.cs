namespace xPlannerAPI.Models
{
    public class AssetInventoryItemIds
    {
        public short domain_id { get; set; }
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
        public int room_id { get; set; }
        public string inventory_ids { get; set; }
    }
}