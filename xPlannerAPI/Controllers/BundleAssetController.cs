using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using xPlannerCommon.Models;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class BundleAssetController : TableGenericController<bundle_asset>
    {
        public BundleAssetController() : base(new [] { "bundle_domain_id", "bundle_id", "asset_domain_id", "asset_id" }, new [] { "bundle_domain_id", "bundle_id" }, new [] { "asset.manufacturer" }) { }

        [ActionName("All")]
        public IEnumerable<BundleAsset> GetAll(int id1, int id2)
        {
            return base.GetAll(id1, id2).Select(a => new BundleAsset {
                domain_id = a.asset.domain_id,
                asset_id = a.asset.asset_id,
                asset_code = a.asset.asset_code,
                asset_description = a.asset.asset_description,
                model_name = a.asset.model_name,
                model_number = a.asset.model_name,
                manufacturer = a.asset.manufacturer.manufacturer_description
            });
        }
    }
}
