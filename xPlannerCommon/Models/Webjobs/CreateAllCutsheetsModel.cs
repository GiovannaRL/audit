namespace xPlannerCommon.Models.Webjobs
{
    public class CreateAllCutsheetsModel
    {
        public short userDomainId { get; set; }
        public string userId { get; set; }
        public CutSheetAssetsInfo assetsInfo { get; set; }
    }
}
