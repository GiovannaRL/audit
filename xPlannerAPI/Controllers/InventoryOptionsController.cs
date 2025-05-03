using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    public class InventoryOptionsController : TableGenericController<inventory_options>
    {
        public InventoryOptionsController() : base(new [] { "inventory_domain_id", "inventory_id", "domain_id", "option_id" }, new [] { "inventory_domain_id", "inventory_id" }, new [] { "assets_options", "domain_document" }) { }

        protected override bool UpdateReferences(inventory_options item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (item.quantity == 0)
                item.quantity = 1;
            return true;
        }

        [HttpPost]
        [ActionName("AllMulti")]
        public IEnumerable<assets_options> PostMulti([FromBody] EditMultipleData data, int id1)
        {
            using (IInventoryOptionRepository repository = new InventoryOptionRepository())
            {
                return repository.UpdateAddMulti(id1, data.inventories);
            }
        }

        [HttpPost]
        [ActionName("ItemMulti")]
        public void Delete([FromBody] EditMultipleData data, int id1, int id2, int id3)
        {
            using (IInventoryOptionRepository repository = new InventoryOptionRepository())
            {
                repository.DeleteFromMulti(id1, data.inventories, id2, id3);
            }
        }

        [ActionName("AddMulti")]
        public void Post([FromBody] EditMultipleData data, int id1)
        {
            foreach (var inventoryId in data.inventories)
            {
                foreach (var option in data.edited_data.inventory_options)
                {
                    base.Add(option, id1, inventoryId);
                }
            }
        }
    }
}
