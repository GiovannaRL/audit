using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IAssetRepository : IDisposable
    {
        string GetMaxCode(int domainId, string assetCode);
        IEnumerable<asset> GetJacks(int domainId);
        asset DuplicateAsset(int currentAssetId, int newDomainId, bool changeInventories, string addedBy);
        asset DuplicateAsset(asset oldAsset, int newDomainId, bool changeInventories, string addedBy, bool linkDuplicated, bool needApproval = false, bool modifyAWAsset = false);
        asset DuplicateAssetWithAWApproval(asset oldAsset, int newDomainId, bool changeInventories, string addedBy, bool linkDuplicated, bool modifyAWAsset);
        List<ReducedAsset> GetNotDiscontinued(int currentDomainId, int currentAssetId);
        Task CreateCutSheet(asset item);
        bool UpdateAssetSimple(asset item);
        get_related_assets_Result UpdateAudaxWareAsset(asset item);
        bool DeleteRelatedAsset(asset item);
        Task<bool> ImportData(short domainId, string filePath, string userId);
        string GetNextCode(int domainId, string assetCode);
        void DuplicateFiles(asset oldAsset, asset newAsset);
        IEnumerable<ProjectQty> GetProjects(int domainId, int assetDomainId, int assetId);
        void ResetControlColumnToRegenerateCoverSheet(asset modifiedItem);
        void SetRegenerateCutSheets(short domain_id);
    }
}
