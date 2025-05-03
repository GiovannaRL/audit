using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogCategories)]
    public class SubCategoriesController : TableGenericController<assets_subcategory>
    {
        public SubCategoriesController() : base(new [] { "domain_id", "category_domain_id", "category_id", "subcategory_id" }, new [] { "domain_id", "category_domain_id", "category_id" }, new [] { "assets_category" }, true, true) { }

        protected override bool UpdateReferences(assets_subcategory item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (item.assets_category != null)
            {
                item.category_id = item.assets_category.category_id;
            }
            item.category_domain_id = (short)id2.GetValueOrDefault();
            item.assets_category = null;

            var fieldsToCheck = new [] { "HVAC", "Plumbing", "Gases", "IT", "Electrical", "Support", "Physical",
                "Environmental" };

            var properties = item.GetType().GetProperties();
            foreach (var property in properties)
            {
                var dataValue = property.GetValue(item, null);
                if (fieldsToCheck.Contains(property.Name) && dataValue == null)
                {
                    property.SetValue(item, "E", null);
                }
            }

            return true;
        }

        [ActionName("Item")]
        public override assets_subcategory Add([FromBody] assets_subcategory item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (base.GetAll(id1, id2, id3).FirstOrDefault(sub => sub.description.ToLower().Equals(item.description.ToLower())) == null)
            {
                return base.Add(item, id1, id2, id3, id4, id5);
            }

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Conflict));
        }

        [ActionName("Item")]
        public override assets_subcategory Put(assets_subcategory item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (base.GetAll(id1, id2, id3).FirstOrDefault(sub => sub.subcategory_id != id4 && sub.description.ToLower().Equals(item.description.ToLower())) == null)
            {
                return base.Put(item, id1, id2, id3, id4, id5);
            }

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Conflict));
        }

        [ActionName("All")]
        public IEnumerable<get_all_category_subcategories_Result> GetAll(int id1, int id2, int id3)
        {
            using (var rep = new SubcategoryRepository())
            {
                return rep.GetFromCategories(id1, id2, new [] { id3 });
            }
        }

        [ActionName("Multi")]
        public HttpResponseMessage FromMultiCategories([FromBody]assets_category[] categories, int id1)
        {
            if (categories == null || categories.Length <= 0)
                return Request.CreateResponse(HttpStatusCode.OK);

            IEnumerable<get_all_category_subcategories_Result> subcategories = new List<get_all_category_subcategories_Result>();
            var ids = categories.Select(c => c.domain_id).Distinct();

            using (var rep = new SubcategoryRepository())
            {
                foreach (var categoryDomainId in ids)
                {
                    subcategories = subcategories.Concat(rep.GetFromCategories(id1, categoryDomainId,
                        categories.Where(c => c.domain_id == categoryDomainId).Select(c => c.category_id).ToArray()));
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { subcategories = subcategories });
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<asset>())
            {
                if (!repository.GetAll(new string[] { }, new int[] { }, new [] { "assets_subcategory" })
                    .Any(a => a.subcategory_domain_id == id1 && a.assets_subcategory.category_domain_id == id2
                        && a.assets_subcategory.category_id == id3 && a.subcategory_id == id4))
                {
                    base.Delete(id1, id2, id3, id4, id5);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }

            return Request.CreateResponse(HttpStatusCode.Conflict, "The subcategory could not be deleted. There is assigned asset!");
        }
    }
}