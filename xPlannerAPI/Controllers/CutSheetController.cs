using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Services;

namespace xPlannerAPI.Controllers
{
    public class CutSheetController : AudaxWareController
    {
        [ActionName("All")]
        public void Put(int id1)
        {
            using (IAssetRepository assetRepository = new AssetRepository())
            {
                assetRepository.SetRegenerateCutSheets(GetLoggedDomainId());
            }
        }
    }
}
