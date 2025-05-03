namespace xPlannerCommon.Models
{
    public class InventoryPictureInfo: PictureInfo
    {
        public bool isAssetPhoto { get; set; }
        public bool isTagPhoto { get; set; }
        public bool isArtwork { get; set; }
        public int rotate { get; set; }
    }
}
