using System;
using System.Collections.Generic;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IInventoryOptionRepository : IDisposable
    {
        IEnumerable<assets_options> UpdateAddMulti(int inventoriesDomainId, List<int> inventoriesIds);
        bool DeleteFromMulti(int inventoiesDomainId, List<int> inventories, int optionDomainId, int optionId);
        List<assets_options> GetAllToAdd(int domainId, int assetId, int? inventoryId, List<int> inventoriesIds);
    }
}
