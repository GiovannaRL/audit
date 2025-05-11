using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Services;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfo)]
    public class MigrateCutseetController : AudaxWareController
    {
        [ActionName("generate_cutsheets")]
        public Dictionary<string, string> GetAll()
        {
            var assetRep = new AssetsController();
            var returnData = new Dictionary<string, string>();

            //IFileStreamRepository stream_rep = new FileStreamRepository();
            var cutsheetRepository = new CutSheetRepository(1);

            var assets = assetRep.GetAll().Where(x => x.cut_sheet != null);

            foreach (var item in assets)
            {
                //stream_rep.create_cover(item);
                cutsheetRepository.BuildFullFromZero(item, BlobContainersName.FullCutsheet(item.domain_id), item.asset_id.ToString() + item.domain_id
                    + ".pdf");

                returnData.Add(item.asset_id.ToString() + item.domain_id, item.asset_code);
            }

            return returnData;
        }
    }
}
