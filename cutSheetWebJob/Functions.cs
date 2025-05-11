using Microsoft.Azure.WebJobs;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using cutSheetWebJob.Services;

namespace cutSheetWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void CreateCutSheet([QueueTrigger("create-cut-sheet")] asset new_asset)
        {
            using (CutSheetRepository repository = new CutSheetRepository(1))//new_asset.domain_id))
            {
                if (repository.BuildFullFromZero(new_asset, BlobContainersName.FullCutsheet(new_asset.domain_id), new_asset.asset_id.ToString() + new_asset.domain_id.ToString() + ".pdf"))
                {
                    using (AssetRepository assetRepository = new AssetRepository(new_asset.domain_id))
                    {
                        assetRepository.UpdateCutSheetValue(new_asset, new_asset.asset_id.ToString() + new_asset.domain_id.ToString() + ".pdf");
                    }
                }
            }
        }
    }
}