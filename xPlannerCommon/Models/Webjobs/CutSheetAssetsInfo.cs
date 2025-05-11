namespace xPlannerCommon.Models.Webjobs
{
    public class CutSheetAssetsInfo
    {
        public short domainId { get; set; }
        public int initialId { get; set; }
        public int finalId { get; set; }

        public bool HasAllInformation() {
            return domainId > 0 && initialId > 0 && finalId >0;
        }
    }
}
