using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using xPlannerCommon.Models;
using reportsWebJob.Services;
using System.Data;
using xPlannerCommon.Services;
using System.Diagnostics;
using System.Windows.Forms;

namespace reportsWebJob
{
    public class Functions
    {
        public static void RenameAssetsFiles([QueueTrigger("rename-assets-files")] int domain_id)
        {
            using (var repository = new AssetsFiles(domain_id))
            {
                repository.RenameAssetsPhoto(domain_id);
                repository.RenameAssetsCutsheet(domain_id);
                repository.RenameAssetsRevit(domain_id);
                repository.RenameAssetsCadBlock(domain_id);
                repository.SaveChanges(domain_id);
            }
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ImportAssets([QueueTrigger("import-assets")] ImportedData item)
        {
            using (var assetRepository = new AssetRepository(item.domain_id))
            {
                assetRepository.ImportAssets(item);
            }
        }

        public static void ImportCategories([QueueTrigger("import-categories")] ImportedData item)
        {
            using (var assetRepository = new CategoryRepository(item.domain_id))
            {
                assetRepository.ImportCategories(item);
            }
        }
        public static void AssetBook([QueueTrigger("asset-book")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new AssetBookRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Asset Book Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void BudgetSummary([QueueTrigger("budget-summary")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new BudgetSummaryRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Budget Summary Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report, e);
                }
            }
        }

        public static void AssetStatus([QueueTrigger("asset-status")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (AssetStatusRepository repository = new AssetStatusRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Asset Status Report {0}", e.Message);
                using (GenericReportRepository repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void Procurement([QueueTrigger("procurement")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new ProcurementRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Procurement Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void AssetByRoom([QueueTrigger("asset-by-room")] string message)
        {

            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new AssetByRoomRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Asset By Room Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void RoomByRoom([QueueTrigger("room-by-room")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new RoomByRoomRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Room By Room Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void RoomByRoomGovernment([QueueTrigger("room-by-room-government")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new RoomByRoomGovernmentRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Room By Room Government Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }


        public static void EquipmentWithCosts([QueueTrigger("equipment-with-costs")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new EquipmentWithCostsRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Equipment with costs Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void ShopDrawing([QueueTrigger("shop-drawing")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new ShopDrawingsRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Shop Drawing Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void CopiedProjectComparison([QueueTrigger("copied-project-inventory-comparison")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new CopiedProjectComparisonRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Copied Project Comparison Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void DeleteReportFiles([QueueTrigger("delete-report-files")] IEnumerable<string> files)
        {
            using (var fileRepository = new FileStreamRepository())
            {
                fileRepository.DeleteBlobs("reports", files);
            }
        }

        public static void JsnRollup([QueueTrigger("jsn-rollup")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new JSNRollupRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error JSN Rollup Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void IllustrationSheet([QueueTrigger("illustration-sheet")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new IllustrationSheetRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Illustration sheet {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static async void GovernmentInventory([QueueTrigger("government-inventory")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;
            try
            {
                using (var repository = new GovernmentInventoryRepository())
                {
                   await repository.Build(report);
                }
            }
            catch (Exception e)
            {
                 Trace.TraceError("Error Government Inventory {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void ComprehensiveInteriorDesign([QueueTrigger("comprehensive-interior-design")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;
            try
            {
                using (var repository = new ComprehensiveInteriorDesignReport())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Comprehensive Interior Design {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }
        public static void DoorList([QueueTrigger("door-list")] string message)
        {

            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);

            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new DoorListRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error Door List Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void RoomEquipmentList([QueueTrigger("room-equipment-list")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);
            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository = new RoomEquimentListRepository())
                {
                    repository.Build(report);
                }
            }
            catch (Exception e)
            {

                Trace.TraceError("Error Room equipment list Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReportError(report);
                }
            }
        }

        public static void EquipmentDimensionalAndUtilities([QueueTrigger("equipment-dimensional-and-utilities")] string message)
        {
            int reportId = int.Parse(message.Trim('"'));
            var report = GetReportData(reportId);
            if (report?.report_location == null || !report.report_location.Any()) return;

            try
            {
                using (var repository  = new EquipmentDimensionalAndUtilitiesRepository())
                {
                    repository.Build(report);

                }

            }
            catch (Exception e)
            {

                Trace.TraceError("Error Equipment dimensional and utilities Report {0}", e.Message);
                using (var repository = new GenericReportRepository())
                {
                    repository.CompleteReport(report);
                }
            }
        }

        private static project_report GetReportData(int reportId)
        {
            var db = new audaxwareEntities();

            using (var cmd = db.Database.Connection.CreateCommand())
            {
                db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '1';";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            var report = db.project_report.Include("project.domain").Include("project_room").Include("report_location")
                .Include("cost_center1").Include("report_type").FirstOrDefault(pr => pr.id == reportId);

            db.Dispose();

            return report;
        }
    }
}