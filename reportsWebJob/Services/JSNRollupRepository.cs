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
    class JSNRollupRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<JSNRollupItem> data = GetData(item);

            string excelFile = BuildExcel(item, filename, data, 94);
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

        private List<JSNRollupItem> GetData(project_report report)
        {
            StringBuilder select = new StringBuilder("SELECT ");
            select.Append("pri.jsn_code as jsn, pri.asset_description as jsn_description, pri.resp, cc.code as cost_center, ");
            select.Append("SUM(COALESCE(pri.budget_qty, 0)) AS planned_qty, ");
            select.Append("CASE WHEN r.isNew = 1 THEN COALESCE(pri.unit_budget_adjusted, 0) ELSE 0 END AS unit_cost, ");
            select.Append("CASE WHEN r.isNew = 1 THEN COALESCE(pri.unit_freight, 0) + COALESCE(pri.unit_install, 0) ELSE 0 END extended_cost,");
            select.Append("CASE WHEN r.isNew = 1 THEN COALESCE(pri.unit_tax_calc, 0) ELSE 0 END AS tax_cost, ");
            select.Append("CASE WHEN r.isNew = 1 THEN COALESCE(pri.unit_budget_total, 0) * SUM(COALESCE(pri.budget_qty, 0)) ELSE 0 END AS total_cost ");
            select.Append("FROM project_room_inventory pri ");
            select.Append("LEFT JOIN cost_center cc ON pri.cost_center_id = cc.id ");
            select.Append("LEFT JOIN responsability r ON pri.resp = r.name ");
            select.Append("WHERE pri.jsn_code IS NOT NULL AND ");
            select.Append(GetWhereClause(report, "pri", "cc", "id").Replace("WHERE", ""));
            select.Append(" GROUP BY pri.jsn_code, pri.asset_description, pri.resp, r.isNew, cc.code, ");
            select.Append("COALESCE(pri.unit_budget, 0), ");
            select.Append("COALESCE(pri.unit_freight, 0) + COALESCE(pri.unit_install, 0), ");
            select.Append("COALESCE(pri.unit_budget_adjusted, 0), ");
            select.Append("COALESCE(pri.unit_budget_total, 0), ");
            select.Append("COALESCE(pri.unit_tax_calc, 0) ");
            select.Append("ORDER BY pri.jsn_code, COALESCE(pri.resp, '')");
            return _db.Database.SqlQuery<JSNRollupItem>(select.ToString()).ToList();
        }

        public string BuildExcel(project_report item, string filename, List<JSNRollupItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "jsnRollupTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["JSN Rollup"];

                /* Report Header */
                worksheet.Cells[3, 1].Value = item.project.project_description;
                worksheet.Cells[6, 8].Value = item.project.domain.name;
                worksheet.Cells[8, 3].Value = item.name;
                worksheet.Cells[9, 3].Value = item.description;
                worksheet.Cells[10, 3].Value = GetCostCenterInfo(item);
                IncrementReportStatus(item, 1 + (decimal)0.2 * (totalPercentage - 2));

                int row = 14;

                if (data == null || !data.Any())
                {
                    worksheet.Cells["A15:J15"].Copy(worksheet.Cells["A14:J14"]);
                    worksheet.Cells[row, 5].Value = 0;
                    worksheet.Cells[row, 6].Value = "$0.00";
                    worksheet.Cells[row, 7].Value = "$0.00";
                    worksheet.Cells[row, 8].Value = "$0.00";
                    worksheet.Cells[row, 9].Value = "$0.00";
                    worksheet.Cells["A16:J16"].Copy(worksheet.Cells["A15:J15"]);

                    xlPackage.Save();
                    UpdateReportStatus(item, totalPercentage);
                    return excelPath;
                }

                worksheet.Cells["A15:J15"].Copy(worksheet.Cells["A1:J1"]);

                decimal percentageByItem = ((decimal)0.7 * (totalPercentage - 2)) / data.Count();
                foreach (JSNRollupItem dt in data)
                {
                    worksheet.Cells["A14:J14"].Copy(worksheet.Cells[string.Format("A{0}:J{0}", row)]);
                    worksheet.Cells[row, 1].Value = dt.jsn;
                    worksheet.Cells[row, 2].Value = dt.jsn_description;
                    worksheet.Cells[row, 3].Value = dt.cost_center;
                    worksheet.Cells[row, 4].Value = dt.resp;
                    worksheet.Cells[row, 5].Value = dt.planned_qty;
                    worksheet.Cells[row, 6].Value = string.Format("{0:C}", dt.unit_cost.GetValueOrDefault());
                    worksheet.Cells[row, 7].Value = string.Format("{0:C}", dt.extended_cost.GetValueOrDefault());
                    worksheet.Cells[row, 8].Value = string.Format("{0:C}", dt.tax_cost.GetValueOrDefault());
                    worksheet.Cells[row, 9].Value = string.Format("{0:C}", dt.adjusted_unit_cost.GetValueOrDefault());
                    worksheet.Cells[row, 10].Value = string.Format("{0:C}", dt.total_cost.GetValueOrDefault());

                    row++;

                    IncrementReportStatus(item, percentageByItem);
                }

                worksheet.Cells["A1:J1"].Copy(worksheet.Cells[string.Format("A{0}:J{0}", row)]);
                worksheet.Cells["A7:J7"].Copy(worksheet.Cells["A1:J1"]);
                worksheet.Cells[row, 6].Value = string.Format("{0:C}", data.Sum(dt => dt.planned_qty).GetValueOrDefault());
                worksheet.Cells[row, 7].Value = string.Format("{0:C}", data.Sum(dt => dt.unit_cost).GetValueOrDefault());
                worksheet.Cells[row, 8].Value = string.Format("{0:C}", data.Sum(dt => dt.extended_cost).GetValueOrDefault());
                worksheet.Cells[row, 9].Value = string.Format("{0:C}", data.Sum(dt => dt.adjusted_unit_cost).GetValueOrDefault());
                worksheet.Cells[row, 10].Value = string.Format("{0:C}", data.Sum(dt => dt.total_cost).GetValueOrDefault());

                // Add totals

                xlPackage.Save();
                UpdateReportStatus(item, totalPercentage);

                return excelPath;
            }

        }
    }
}
