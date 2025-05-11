using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using xPlannerCommon.Services;
using xPlannerCommon.Models;

namespace POWebJob
{
    public class Functions
    {
        public static void GenerateAll([QueueTrigger("generate-all-podetails-pages")] int domain_id)
        {
            var db = new audaxwareEntities();

            using (var cmd = db.Database.Connection.CreateCommand())
            {
                db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '1';";
                cmd.ExecuteNonQuery();
            }

            var pos = db.purchase_order.Where(po => po.domain_id == domain_id).ToList();

            foreach (var po in pos)
            {
                using (var repository = new PORepository(po.domain_id))
                {
                    repository.CreatePODetailsPageAndUpload(po.domain_id, po.project_id, po.po_id);
                }
            }
        }

        public static void DeletePOsAndQuotesFiles([QueueTrigger("delete-pos-and-quotes-files")] Dictionary<int, Dictionary<string, List<string>>> dados)
        {
            using (var fileRepository = new FileStreamRepository())
            {
                foreach (var item in dados)
                {
                    fileRepository.DeleteBlobs($"po{item.Key}", item.Value["pos"]);
                    fileRepository.DeleteBlobs($"quote{item.Key}", item.Value["quotes"]);
                }
            }
        }
    }
}
