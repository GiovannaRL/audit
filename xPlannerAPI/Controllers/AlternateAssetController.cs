using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogAssets)]
    public class AlternateAssetController : AudaxWareController
    {
        /* id1 = domain_id
         * id2 = subcategory_domain_id
         * id3 = subcategory_id
         * id4 = current asset_id */
        [ActionName("All")]
        public IEnumerable<ReducedAsset> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null)
        {
            using (var assetRepository = new TableRepository<asset>())
            {
                var allSubcategory = assetRepository.GetAll(new [] { "domain_id", "subcategory_domain_id", "subcategory_id" },
                    new [] { id1, id2 ?? 0, id3 ?? 0 }, null).Where(a => a.discontinued != true && a.asset_id != id4)
                    .OrderBy(a => a.asset_code)
                    .Select(a => new ReducedAsset { asset_code = a.asset_code, asset_id = a.asset_id, domain_id = a.domain_id });

                var current = assetRepository.Get(new [] { "domain_id", "asset_id" }, new [] { id1, id4.GetValueOrDefault() }, null);
                if (current != null)
                {
                    var prefix = current.asset_code.Substring(0, 3);
                    allSubcategory = allSubcategory.Where(a => a.asset_code.Substring(0, 3).Equals(prefix));
                }

                return allSubcategory;
            }
        }
    }
}
