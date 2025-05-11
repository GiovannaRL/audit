using OfficeOpenXml;
using reportsWebJob.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace reportsWebJob.Services
{
    class EquipmentWithCostsRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<EquipmentWithCostsItem> data = GetData(item);

            string excelFile = BuildExcel(item, filename, data, 47); // change to 47 when Build pdf is working
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

        public string BuildExcel(project_report item, string filename, List<EquipmentWithCostsItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "equipmentWithCostsTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

    

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                client client = _db.clients.Where(x => x.id == item.project.client_id && x.domain_id == item.project.domain_id).FirstOrDefault();
                facility facility = _db.facilities.Where(x => x.id == item.project.facility_id && x.domain_id == item.project.domain_id).FirstOrDefault();
                          
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Equipment With Costs"];

                if (item.remove_logo == true)
                {
                    worksheet.HeaderFooter.FirstHeader.RightAlignedText = null;
                    worksheet.HeaderFooter.FirstHeader.CenteredText = worksheet.HeaderFooter.FirstHeader.LeftAlignedText;
                    worksheet.HeaderFooter.FirstHeader.LeftAlignedText = null;
                }

                /* Report Header */
                worksheet.Cells[13, 3].Value = item.project.project_description;
                worksheet.Cells[14, 3].Value = client.name;
                worksheet.Cells[15, 3].Value = facility.name;
                IncrementReportStatus(item, 1);

                int row = 19;
                Decimal percentageByItem;

                if (data.Any())
                {
                    percentageByItem = ((decimal)0.2 * (totalPercentage - 2)) / data.Count();
                    foreach (var dt in data)
                    {
                        worksheet.Cells["A19:I19"].Copy(worksheet.Cells["A" + row + ":I" + row]);
                        worksheet.Cells[row, 1].Value = dt.resp;
                        worksheet.Cells[row, 2].Value = dt.asset_code;
                        worksheet.Cells[row, 3].Value = dt.asset_description;
                        worksheet.Cells[row, 4].Value = dt.manufacturer_description;
                        worksheet.Cells[row, 5].Value = dt.model_name;
                        worksheet.Cells[row, 6].Value = dt.model_number;
                        worksheet.Cells[row, 7].Value = dt.budget_qty;
                        worksheet.Cells[row, 8].Value = dt.unit_budget;
                        worksheet.Cells[row, 9].Value = dt.total_budget;

                        row++;
                        IncrementReportStatus(item, percentageByItem);
                    }
                }
                else
                {
                    IncrementReportStatus(item, (decimal)0.2 * (totalPercentage - 2));
                    // Delete the example row
                    worksheet.DeleteRow(row);
                }

                row += 2;
                xlPackage.Save();
                UpdateReportStatus(item, totalPercentage);
            }

            return excelPath;
        }

        private List<EquipmentWithCostsItem> GetData(project_report report)
        {
            List<EquipmentWithCostsItem> items = new List<EquipmentWithCostsItem>();

            StringBuilder select = new StringBuilder("SELECT resp, asset_code, asset_description, manufacturer_description, ");
            select.Append("serial_name AS model_name, serial_number AS model_number, SUM(budget_qty) AS budget_qty, ");
            select.Append("COALESCE(unit_budget, 0) AS unit_budget, SUM(total_budget) AS total_budget ");
            select.Append("FROM asset_inventory a ");
            select.Append("WHERE ");
            select.Append(GetWhereClause(report, "a", "a").Replace("WHERE", ""));
            select.Append("GROUP BY resp, asset_code, asset_description, manufacturer_description, serial_name, serial_number, unit_budget ");
            select.Append("ORDER BY asset_code, asset_description, unit_budget ");

            return _db.Database.SqlQuery<EquipmentWithCostsItem>(select.ToString()).ToList();
        }

    }
}
