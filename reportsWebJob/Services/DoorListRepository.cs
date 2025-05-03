using OfficeOpenXml;
using reportsWebJob.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace reportsWebJob.Services
{
    class DoorListRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<DoorListItem> data = GetData(item);

            string excelFile = BuildExcel(item, filename, data, 94); // change to 47 when Build pdf is working
            UpdateReportStatus(item, 94);

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

        public string BuildExcel(project_report item, string filename, List<DoorListItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "doorListTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Door List"];

                var room_id = 0;
                int row = 1;
                foreach (var inventory in data)
                {
                    if (room_id != inventory.room_id)
                    {
                        /* Report Header */
                        if (row > 1)
                        {
                            // Insert Page break
                            worksheet.Row(row).PageBreak = true;
                            row = row + 2;
                        }                       

                        room_id = inventory.room_id;
                        worksheet.Cells["A1:I6"].Copy(worksheet.Cells[string.Format("A{0}", row)]);
                        worksheet.Cells[row, 2].Value = item.project.project_description;
                        row++;
                        worksheet.Cells[row, 2].Value = inventory.department_description;
                        worksheet.Cells[row, 4].Value = inventory.phase_description;
                        row++;
                        worksheet.Cells[row, 2].Value = inventory.room_name;
                        worksheet.Cells[row, 4].Value = inventory.room_number;
                        row = row+3;
                        //IncrementReportStatus(item, 1 + (decimal)0.2 * (totalPercentage - 2));
                    }

                    worksheet.Cells["A6:I6"].Copy(worksheet.Cells[string.Format("A{0}:I{0}", row)]);
                    worksheet.Cells[row, 1].Value = inventory.jsn_code;
                    worksheet.Cells[row, 2].Value = inventory.asset_description;
                    worksheet.Cells[row, 3].Value = inventory.resp;
                    worksheet.Cells[row, 4].Value = inventory.cost_center;
                    worksheet.Cells[row, 5].Value = inventory.budget_qty;
                    worksheet.Cells[row, 6].Value = inventory.ecn;
                    //IncrementReportStatus(item, 1 + (decimal)0.2 * (totalPercentage - 2));


                    row++;
                }

                // Remove ECN Column
                if ((bool)(item.remove_ecn ?? false))
                    worksheet.DeleteColumn(6);

                xlPackage.Save();
                UpdateReportStatus(item, totalPercentage);
            }

            return excelPath;
        }


        private List<DoorListItem> GetData(project_report report)
        {
            List<DoorListItem> items = new List<DoorListItem>();

            StringBuilder select = new StringBuilder("SELECT room_id, room_number, room_name, department_description, phase_description, ");
            select.Append(" jsn_code, asset_description, cost_center, resp, budget_qty, ECN ");
            select.Append("FROM asset_inventory a ");
            select.Append("WHERE coalesce(jsn_code, '') != '' AND a.budget_qty > 0 AND ");
            select.Append(GetWhereClause(report, "a", "a").Replace("WHERE", ""));
            select.Append("ORDER BY phase_description, department_description, room_number, room_name, jsn_code");

            return _db.Database.SqlQuery<DoorListItem>(select.ToString()).ToList();
        }

    }
}
