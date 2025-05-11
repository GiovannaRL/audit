using OfficeOpenXml;
using OfflineXPlanner.Database;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Facade;
using OfflineXPlanner.Security;
using OfflineXPlanner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using xPlannerCommon.Models;

namespace OfflineXPlanner.Business
{
    public class InventoryBusiness
    {
        public static bool InsertInventory(int project_id, int department_id, int room_id, Inventory inv)
        {
            IInventoryDAO data = new InventoryDAO();
            return data.InsertInventory(project_id, department_id, room_id, inv);
        }

        public static bool UpdateInventory(int project_id, int department_id, int room_id, Inventory inv)
        {
            IInventoryDAO data = new InventoryDAO();
            return data.UpdateInventory(project_id, department_id, room_id, inv);
        }

        public static bool Delete(int inventoryId)
        {
            IInventoryDAO data = new InventoryDAO();
            return data.DeleteInventory(inventoryId);
        }

        private static void applyInventoryChanges(Inventory inv) {
            if (DatabaseUtil.isDefaultJSNCode(inv.JSN))
            {
                inv.JSN = inv.JSNNomenclature = null;
            }

            if (inv.PlannedQty <= 0)
            {
                inv.PlannedQty = 1;
            }
        }
        
        public static bool Export(int project_id)
        {
            IInventoryDAO inventoryDAO = new InventoryDAO();


            var inventoryData = inventoryDAO.GetInventoriesAsList(project_id);

            inventoryData.ForEach(applyInventoryChanges);
            ProgressBarForm progressBarForm = new ProgressBarForm("Uploading data", inventoryData.Count);
            progressBarForm.Show();

            string excelPath = CreateExcelFile(project_id, inventoryData);


            if (excelPath != null)
            {
                var result = InventoryFacade.AnalyzeData(project_id, File.ReadAllBytes(excelPath), excelPath);
                if (result == null || result.Status != ExportAnalysisResultStatusEnum.Ok)
                {
                    progressBarForm.Close();
                    MessageBox.Show(result == null ? "Error to try upload data" : result.ErrorMessage);
                }
                else
                {
                    progressBarForm.PerformStep();

                    var itemsWithError = ListUtil.isNull(result.Items) ? null : result.Items.Where(i => i.Status == ExportItemStatusEnum.Error);
                    if (ListUtil.isEmptyOrNull(itemsWithError))
                    {
                        for (int i=0; i <  result.Items.Count;  i+=10)
                        {
                            var exportItems = new List<Inventory>();
                            int count = 10;

                            if ((i + count) > result.Items.Count)
                            {
                                count = result.Items.Count - i;
                            }
                            exportItems.AddRange(result.Items.GetRange(i, count));
                            var exportResult = InventoryFacade.ExportData(project_id, exportItems);
                            if (exportResult == null)
                            {
                                progressBarForm.Close();
                                MessageBox.Show("Error to try upload data");
                                return false;
                            }

                            int exportResultSize = exportResult.Items.Count;
                            for (int j = 0; j < exportResultSize; ++j)
                            {
                                progressBarForm.PerformStep();
                                if (exportResult.Items.ElementAt(j) != null)
                                {
                                    if (exportResult.Items.ElementAt(j).Id != null)
                                    {
                                        inventoryData[i + j].inventory_id = (int)exportResult.Items.ElementAt(j).Id;
                                        inventoryDAO.UpdateInventoryID(project_id, inventoryData[i + j].department_id,
                                            inventoryData[i + j].room_id, inventoryData[i + j]);
                                    }
                                }
                            }
                        }

                        progressBarForm.Close();
                        ExportPictures(inventoryData);
                        return true;
                    }
                    else
                    {
                        progressBarForm.Close();
                        ExportDataItemsErrorForm exportDataErrorForm = new ExportDataItemsErrorForm(itemsWithError);
                        exportDataErrorForm.ShowDialog();
                    }
                }
            }

            return false;
        }

        private static string CreateExcelFile(int project_id, List<Inventory> inventoryData)
        {

            if (!ListUtil.isEmptyOrNull(inventoryData))
            {
                string rootDirectory = IOUtil.GetRootDirectory();

                string excelPath = IOUtil.GetExcelFilePath(AudaxwareRestApiInfo.loggedDomain.domain_id);
                FileInfo report = new FileInfo(excelPath);
                FileInfo template = new FileInfo(Path.Combine(rootDirectory, "excel_templates", "export_assets.xlsx"));
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                using (ExcelPackage xlPackage = new ExcelPackage(report, template))
                {
                    ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Inventory"];

                    int row = 2;
                    foreach (var item in inventoryData)
                    {
                        item.Resp = string.IsNullOrEmpty(item.Resp) ? "R" : item.Resp;
                        item.UnitOfMeasure = string.IsNullOrEmpty(item.UnitOfMeasure) ? "EA" : item.UnitOfMeasure;
                        worksheet.Cells[row, 1].Value = item.Code;
                        worksheet.Cells[row, 2].Value = item.Manufacturer;
                        worksheet.Cells[row, 3].Value = item.Description;
                        worksheet.Cells[row, 4].Value = item.ModelNumber;
                        worksheet.Cells[row, 5].Value = item.ModelName;
                        worksheet.Cells[row, 6].Value = item.JSN;
                        worksheet.Cells[row, 7].Value = item.JSNNomenclature;
                        worksheet.Cells[row, 8].Value = Math.Max(1, item.PlannedQty);
                        worksheet.Cells[row, 9].Value = item.Class;
                        worksheet.Cells[row, 10].Value = item.Clin;
                        worksheet.Cells[row, 11].Value = item.Phase;
                        worksheet.Cells[row, 12].Value = item.Department;
                        worksheet.Cells[row, 13].Value = item.RoomNumber;
                        worksheet.Cells[row, 14].Value = item.RoomName;
                        worksheet.Cells[row, 15].Value = item.Resp;
                        worksheet.Cells[row, 16].Value = item.U1;
                        worksheet.Cells[row, 17].Value = item.U2;
                        worksheet.Cells[row, 18].Value = item.U3;
                        worksheet.Cells[row, 19].Value = item.U4;
                        worksheet.Cells[row, 20].Value = item.U5;
                        worksheet.Cells[row, 21].Value = item.U6;
                        worksheet.Cells[row, 22].Value = item.UnitOfMeasure;
                        worksheet.Cells[row, 23].Value = item.inventory_id > 0 ? (object)item.inventory_id : "";
                        worksheet.Cells[row, 24].Value = item.ECN;
                        worksheet.Cells[row, 25].Value = item.Height;
                        worksheet.Cells[row, 26].Value = item.Width;
                        worksheet.Cells[row, 27].Value = item.MountingHeight;
                        worksheet.Cells[row, 28].Value = item.Depth;
                        worksheet.Cells[row, 29].Value = item.Placement;
                        worksheet.Cells[row, 30].Value = item.Comment;
                        worksheet.Cells[row, 31].Value = item.CADID;
                        row++;
                    }

                    xlPackage.Save();
                }

                return excelPath;
            }

            return null;
        }

        private static bool UploadPictures(Inventory item, List<string> paths)
        {
            if (item == null || ListUtil.isEmptyOrNull(paths))
            {
                return true;
            }

            List<FileData> request = new List<FileData>();
            bool isSuccess = true;

            var reportFile = PictureUtil.GetUploadErrorReportFilePath(item.project_id);
            
            foreach (string path in paths)
            {
                try
                {
                    var fileType = 4;
                    //check if tag or photo file
                    if (item.TagPhotoFile == path)
                    {
                        fileType = 7;
                    }
                    else if (item.PhotoFile == path)
                    {
                        fileType = 6;
                    }

                    byte[] bytes = File.ReadAllBytes(path);

                    request.Add(new FileData
                    {
                        fileExtension = "jpg",
                        base64File = Convert.ToBase64String(bytes),
                        fileType = fileType
                    });
                    int retry = 0;

                    while (retry < 3)
                    {
                        if (InventoryFacade.UploadPictures(item.project_id, item.inventory_id, request))
                        {
                            break;
                        }
                        ++retry;
                    }
                    if (retry >= 3)
                    {
                        File.AppendAllText(reportFile, $"Failed to upload file: {path}\n");
                        isSuccess = false;
                    }
                }
                catch (Exception e)
                {
                    File.AppendAllText(reportFile, $"Error to read file: {path}\n");
                    Trace.TraceError($"Error to read file: {path}", e);
                    isSuccess = false;
                }
            }

            return isSuccess;
        }

        private static void ExportPictures(List<Inventory> inventoryData)
        {
            if (ListUtil.isEmptyOrNull(inventoryData)) return;

            ProgressBarForm progressBarForm = new ProgressBarForm("Uploading inventory's pictures", inventoryData.Count);
            progressBarForm.Show();

            bool isSuccess = true;
            var failed = new StringBuilder();
            var reportFile = PictureUtil.GetUploadErrorReportFilePath(inventoryData[0].project_id);
            foreach (var item in inventoryData)
            {
                var paths = PictureUtil.GetPictureFileNames(item.project_id, item.department_id, item.room_id, item.Id);
                if (!UploadPictures(item, paths))
                {
                    isSuccess = false;
                    File.AppendAllText(reportFile, $" Error for the following item: {item.inventory_id}, {item.Department}, {item.RoomNumber}, {item.RoomName}, {item.Description}\n");
                }
                progressBarForm.PerformStep();
            }

            if (!isSuccess)
            {
                MessageBox.Show($"The upload has been completed but some images failed to upload. Please see the report at \'{reportFile}\'", "Upload Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            progressBarForm.Close();
        }
    }
}
