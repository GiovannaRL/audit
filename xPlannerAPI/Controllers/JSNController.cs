using System.Collections.Generic;
using System.Web.Http;
using System.Data.Entity.Core;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using System.Linq;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class JSNController : TableGenericController<jsn>
    {
        public JSNController() : base(new [] { "domain_id", "id" }, new [] { "domain_id" }, true) { }


        [ActionName("Available")]
        public IEnumerable<jsn> GetAvailable(int id1)
        {
            return base.GetAll(id1).Where(x => x.deprecated != true);
        }
        /**
         * id = domain_id
         * id2 = id
         * id3 = asset_id
         */
        [ActionName("JSNUtilities")]
        public IEnumerable<AssetSettingsStructure> GetJSNUtilities(int id1, int id2, int id3, int id4)
        {
            var jsn = base.GetItem(id1, id2);
            var assetRepository = new TableRepository<asset>();
            var item = assetRepository.Get(new [] { "domain_id", "asset_id" }, new [] { id3, id4 }, new [] { "assets_subcategory.assets_category" });
            item.jsn = jsn;

            using (JSNRepository rep = new JSNRepository())
            {
                item = rep.UpdateMetrics(item);
                item = rep.UpdateU1(item);
                item = rep.UpdateU2(item);
                item = rep.UpdateU3(item);
                item = rep.UpdateU4(item);
                item = rep.UpdateU5(item);
                item = rep.UpdateU6(item);
            }

            using (var repository = new AssetSettingsRepository())
            {
                return repository.GetNotDisabledJSN(item);
            }
        }
    }
}