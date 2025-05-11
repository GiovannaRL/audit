namespace xPlannerAPI.Models
{
    public class ReplaceInventory
    {
        public int[] inventories_id { get; set; }
        public int new_asset_domain_id { get; set; }
        public int new_asset_id { get; set; }
        public string cost_col { get; set; }
        public decimal budget { get; set; }
        public string resp { get; set; }
    }
}