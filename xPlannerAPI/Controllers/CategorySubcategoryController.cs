using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerAPI.Services;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogCategories)]
    public class CategorySubcategoryController : TableGenericController<joined_category_subcategory>
    {
        public CategorySubcategoryController() : base(new [] { "subcategory_domain_id", "category_domain_id", "category_id", "subcategory_id" }, new [] { "subcategory_domain_id", "category_domain_id" }, null, true) { }

        [ActionName("All")]
        public override IEnumerable<joined_category_subcategory> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (CategorySubcategoryRepository rep = new CategorySubcategoryRepository())
            {
                return rep.GetAll((short)id1, (short)id2);
            }

        }
    }
}
