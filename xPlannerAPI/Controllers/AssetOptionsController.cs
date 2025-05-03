using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models.Enums;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogAssets)]
    public class AssetOptionsController : TableGenericController<assets_options>
    {
        public AssetOptionsController() : base(new[] { "domain_id", "asset_id", "asset_option_id" }, new[] { "domain_id", "asset_id" }, new[] { "domain_document" }) { }

        protected override bool UpdateReferences(assets_options item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {

            if (item.min_cost != null && item.max_cost != null)
            {
                item.avg_cost = (item.min_cost + item.max_cost) / 2;
            }

            if (item.project_domain_id == null || item.project_id == null) {
                item.scope = AssetOptionScopeEnum.Catalog;
            }

            return true;
        }

        [ActionName("All")]
        public override IEnumerable<assets_options> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return base.GetAll(id1, id2, id3, id4, id5).Where(op => op.scope != AssetOptionScopeEnum.Custom);
        }

        [ActionName("AllInventory")]
        public List<assets_options> GetAllInventory(int id1, int id2, int? id3, [FromUri]List<int> inventories)
        {
            using (IInventoryOptionRepository repository = new InventoryOptionRepository())
            {
                return repository.GetAllToAdd(id1, id2, id3, inventories);
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<inventory_options>())
            {
                if (!repository.GetAll(new[] { "domain_id", "option_id" }, new[] { id1, id3.GetValueOrDefault() }, null).Any())
                {
                    return base.Delete(id1, id2, id3, id4, id5);
                }
            }

            return Request.CreateResponse(HttpStatusCode.Conflict, "The option could not be deleted. It has assigned asset");
        }

        /**
        * id1 = domain_id
        * id2 = asset_id
        * id3 = asset_option_id
        */
        [ActionName("Item")]
        public assets_options Put([FromBody] GenericOption item, int id1, int id2, int id3)
        {
            item.domain_id = (short)id1;
            item.asset_id = id2;
            item.option_id = id3;
            item.asset_domain_id = item.domain_id;

            using (IOptionRepository optionRepository = new OptionRepository())
            {
                if (item.document_domain_id == null || item.document_id == null || item.picture != null)
                {
                    // Delete old picture
                    optionRepository.DeletePicture(base.GetItem(id1, id2, id3));
                    item.document_id = null;
                    item.document_domain_id = null;
                }

                var option = base.Put(item.ToAssetOption(), id1, id2, id3);
                if (option != null && item.picture != null)
                {

                    optionRepository.AddPicture(new GenericOption
                    {
                        domain_id = item.domain_id,
                        option_id = item.option_id
                    }, item.picture);
                }

                return option;
            }
        }

        /**
        * id1 = domain_id
        * id2 = asset_id
        */
        [ActionName("Item")]
        public assets_options Add([FromBody] GenericOption item, int id1, int id2)
        {
            using (IOptionRepository optionRepository = new OptionRepository())
            {
                item.domain_id = (short)id1;
                item.asset_id = id2;
                item.asset_domain_id = item.domain_id;
                assets_options option = optionRepository.AddOption(item, User.Identity.Name);

                if (option != null)
                {
                    optionRepository.AddPicture(new GenericOption
                    {
                        domain_id = item.domain_id,
                        option_id = option.asset_option_id
                    }, item.picture);
                }

                return option;
            }
        }
    }
}
