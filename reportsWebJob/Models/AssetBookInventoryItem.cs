using xPlannerCommon.Models;

namespace reportsWebJob.Models
{
    public class AssetBookInventoryItem : CutSheetInfo
    {

        public AssetBookInventoryItem() : base() { }
        public AssetBookInventoryItem(asset asset) : base(asset) { }
    }
}
