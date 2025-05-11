using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogAssets)]
    public class AssetCodesController : TableGenericController<assets_codes>
    {
        public AssetCodesController() : base(new [] { "domain_id", "prefix" }, new [] { "domain_id" }, true) { }

        protected override bool UpdateReferences(assets_codes item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            item.next_seq = 1;

            return true;
        }
    }
}