using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using OfficeOpenXml;
using System.IO;
using reportsWebJob.Models;
using xPlannerCommon.Models.Enums;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using xPlannerCommon.Models.Enum;
using System.Diagnostics;

namespace reportsWebJob.Services
{
    class GovernmentInventoryRepository : GenericReportRepository
    { 
        public async Task Build(project_report item)
        {
            try
            {
                InitiateReport(item);

                string filename = GetFilename(item);

                List<GovernmentInventoryItem> data = GetData(item);
                UpdateReportStatus(item, ReportStatusCategory.Generating.PercentageStart);

                string excelFile = await BuildExcel(item, filename, data, 94);

                using (FileStreamRepository fileRepository = new FileStreamRepository())
                {

                    if (UploadToCloud(item, excelFile, "xlsx"))
                    {
                        item.file_name = filename;
                        CompleteReport(item);
                    }


                    fileRepository.DeleteFile(excelFile);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error to generate Government Inventory Report {0}", e.Message);
            }
            
        }

        /*
         * Download the photos in a parallel way 
         * Documentation: https://docs.microsoft.com/pt-br/azure/storage/blobs/storage-blob-scalable-app-download-files
         */
        private async Task<List<GovernmentInventoryItem>> DownloadPhotosInParallel(
            project_report report, List<GovernmentInventoryItem> itemsReport, string directoryToSavePhotos
        ) {

            FileStreamRepository _fileRepository = new FileStreamRepository();
            Dictionary<string, string> downloadedPhotos = new Dictionary<string, string>();

            List<Task> tasks = new List<Task>();
            int max_outstanding = 100;
            int completed_count = 0;

            SemaphoreSlim sem = new SemaphoreSlim(max_outstanding, max_outstanding);

            int idx = 0;
            int totalItems = itemsReport.Count();

            UpdateReportStatus(report, ReportStatusCategory.DownloadingPhotos.PercentageStart);

            while (idx < totalItems) {

                GovernmentInventoryItem item = itemsReport.ElementAt(idx);

                string photoPath = null;
                if (item.photo != null && item.photo_domain_id > 0)
                {
                    if (!downloadedPhotos.ContainsKey(item.photo))
                    {
                        var photoBlob = _fileRepository.GetBlob(BlobContainersName.Photo(item.photo_domain_id), item.photo);
                        if (photoBlob.Exists())
                        {
                            photoPath = Path.Combine(directoryToSavePhotos, item.photo);

                            await sem.WaitAsync();
                            tasks.Add(photoBlob.DownloadToAsync(photoPath).ContinueWith((t) =>
                            {
                                sem.Release();
                                Interlocked.Increment(ref completed_count);
                            }));
                        }
                        else { photoPath = null; }

                        downloadedPhotos.Add(item.photo, photoPath);
                    }
                    else
                    {
                        // If the photo has already been dowloaded, only added the local path
                        photoPath = downloadedPhotos[item.photo];
                    }
                }

                itemsReport.ElementAt(idx).photo = photoPath;
                idx++;
            };

            // Wait for all images to be downloaded
            await Task.WhenAll(tasks);

            UpdateReportStatus(report, ReportStatusCategory.DownloadingPhotos.PercentageEnd);

            // return the data with the photo field modified to contains the path where the photo was saved
            return itemsReport;
        }

        public string CreateDirectoryToDownloadPhotos()
        {
            using (FileStreamRepository _fileRepository = new FileStreamRepository())
            {
                string directoryPath = string.Format("government_inventory_photos_tmp_{0}", Guid.NewGuid().ToString());

                // Create the directory where the photos will be temporarily stored
                _fileRepository.CreateLocalDirectory(directoryPath);

                return directoryPath;
            }
        }

        public async Task<string> BuildExcel(project_report item, string filename, List<GovernmentInventoryItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "governmentInventoryTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Inventory"];

                /* Report Header */
                IncrementReportStatus(item, 1);

                int row = 5;
                string photosDirectoryPath = null;

                if (data.Any())
                {
                    decimal percentageByItem = (totalPercentage - 2 - (item.include_photo.GetValueOrDefault() ? ReportStatusCategory.DownloadingPhotos.PercentageEnd : 0)) / data.Count();

                    if (!item.include_photo.GetValueOrDefault())
                    {
                        worksheet.Column(20).Hidden = true;
                    } else
                    {
                        // Create directory where the photos will be temporarily stored
                        photosDirectoryPath = CreateDirectoryToDownloadPhotos();

                        // Download all the photos
                        var dataWithPhotoPath = await DownloadPhotosInParallel(item, data, photosDirectoryPath);

                        for (int i = row; i < data.Count() + row; i++)
                        {
                            worksheet.Row(i).Height = 40;
                        }
                    }


                    foreach (var inventory in data)
                    {
                        worksheet.Cells["A5:U5"].Copy(worksheet.Cells["A" + row + ":U" + row]);
                        worksheet.Cells[row, 1].Value = inventory.department;
                        worksheet.Cells[row, 2].Value = inventory.drawing_room_number;
                        worksheet.Cells[row, 3].Value = inventory.drawing_room_name;
                        worksheet.Cells[row, 4].Value = inventory.jsn_code;
                        worksheet.Cells[row, 5].Value = inventory.asset_description;
                        worksheet.Cells[row, 6].Value = inventory.budget_qty;
                        worksheet.Cells[row, 7].Value = inventory.manufacturer_description;
                        worksheet.Cells[row, 8].Value = inventory.serial_name;
                        worksheet.Cells[row, 9].Value = inventory.height;
                        worksheet.Cells[row, 10].Value = inventory.width;
                        worksheet.Cells[row, 11].Value = inventory.depth;
                        worksheet.Cells[row, 12].Value = inventory.mounting_height;
                        worksheet.Cells[row, 13].Value = inventory.jsn_utility1.ToLower() == "n/a" || inventory.jsn_utility1 == "" ? "." : inventory.jsn_utility1;
                        worksheet.Cells[row, 14].Value = inventory.jsn_utility2.ToLower() == "n/a" || inventory.jsn_utility2 == "" ? "." : inventory.jsn_utility2;
                        worksheet.Cells[row, 15].Value = inventory.jsn_utility3.ToLower() == "n/a" || inventory.jsn_utility3 == "" ? "." : inventory.jsn_utility3;
                        worksheet.Cells[row, 16].Value = inventory.jsn_utility4.ToLower() == "n/a" || inventory.jsn_utility4 == "" ? "." : inventory.jsn_utility4;
                        worksheet.Cells[row, 17].Value = inventory.jsn_utility5.ToLower() == "n/a" || inventory.jsn_utility5 == "" ? "." : inventory.jsn_utility5;
                        worksheet.Cells[row, 18].Value = inventory.jsn_utility6.ToLower() == "n/a" || inventory.jsn_utility6 == "" ? "." : inventory.jsn_utility6;
                        worksheet.Cells[row, 19].Value = inventory.ECN;
                        worksheet.Cells[row, 21].Value = inventory.comment;

                        // Insert the downloaded photo
                        if (!string.IsNullOrEmpty(inventory.photo) && item.include_photo.GetValueOrDefault()) {

                            // Needs to use 'using' otherwise the file will be locked
                            using (Image logo = Image.FromFile(inventory.photo)) {
                                //Get Thumbnail from image
                                Image thumb = logo.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);
                                thumb.Save(Path.ChangeExtension(inventory.photo, "thumb"));
                                var uniqueName = $"{row}_photo";
                                var picture = worksheet.Drawings.AddPicture(uniqueName, new FileInfo(inventory.photo));
                                picture.SetSize(40, 40);
                                picture.SetPosition(row - 1, 5, 19, 5);
                            }                            
                        }

                        row++;

                        IncrementReportStatus(item, percentageByItem);
                    }
                }
                xlPackage.Save();

                // Delete the directory that store the photos recursively
                using (FileStreamRepository _fileRepository = new FileStreamRepository())
                {                    
                    _fileRepository.DeleteLocalDirectory(photosDirectoryPath, true);
                }
            }

            return excelPath;
        }

        private List<GovernmentInventoryItem> GetData(project_report report)
        {
            List<GovernmentInventoryItem> items = new List<GovernmentInventoryItem>();

            StringBuilder select = new StringBuilder("select CASE WHEN COALESCE(asset_photo_doc.blob_file_name, '') = '' THEN pri.asset_domain_id ELSE pri.domain_id END as photo_domain_id, CASE WHEN COALESCE(asset_photo_doc.blob_file_name, '') = '' THEN a.photo ELSE asset_photo_doc.blob_file_name END as photo, ");
            select.Append("pd.description as department, pr.drawing_room_name, pr.drawing_room_number, pri.jsn_code, pri.asset_description, COALESCE(pri.budget_qty, 0) as budget_qty, pri.manufacturer_description, pri.serial_name, pri.height, ");
            select.Append("pri.width, pri.depth, pri.mounting_height, pri.ECN, pri.comment,  ");
            select.Append("COALESCE(pri.jsn_utility1, 'N/A') AS jsn_utility1, COALESCE(pri.jsn_utility2, 'N/A') AS jsn_utility2, COALESCE(pri.jsn_utility3, 'N/A') AS jsn_utility3, COALESCE(pri.jsn_utility4, 'N/A') AS jsn_utility4, COALESCE(pri.jsn_utility5, 'N/A') AS jsn_utility5, COALESCE(pri.jsn_utility6, 'N/A') AS jsn_utility6  ");
            select.Append("FROM project_room_inventory pri ");
            select.Append("INNER JOIN project_department pd on pd.project_id = pri.project_id and pd.phase_id = pri.phase_id and pd.department_id = pri.department_id and pd.domain_id = pri.domain_id ");
            select.Append("INNER JOIN project_room pr on pr.project_id = pri.project_id and pr.phase_id = pri.phase_id and pr.department_id = pri.department_id and pr.room_id = pri.room_id and pr.domain_id = pri.domain_id ");
            select.Append("INNER JOIN assets a on a.asset_id = pri.asset_id and a.domain_id = pri.asset_domain_id ");
            select.Append("LEFT JOIN cost_center cc ON pri.cost_center_id = cc.id ");
            select.Append("LEFT JOIN ( ");
            select.Append("	SELECT pdoc.blob_file_name, da.inventory_id FROM documents_associations da ");
            select.Append("	INNER JOIN project_documents pdoc on pdoc.id = da.document_id and pdoc.project_domain_id = da.project_domain_id and pdoc.type_id =  " + DocumentTypeIdEnum.AssetPhoto);
            select.Append(") as asset_photo_doc on asset_photo_doc.inventory_id = pri.inventory_id ");
            select.Append(GetWhereClause(report, "pri", "cc", "id"));
            select.Append(" ORDER BY pd.description, pr.drawing_room_number ");


            return _db.Database.SqlQuery<GovernmentInventoryItem>(select.ToString()).ToList();
        }

    }
}
