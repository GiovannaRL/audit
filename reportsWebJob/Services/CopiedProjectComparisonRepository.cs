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
    class CopiedProjectComparisonRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<ProjectComparisonList> data = GetData(item);

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

        public string BuildExcel(project_report item, string filename, List<ProjectComparisonList> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "copiedProjectInventoryComparisonTemplate.xlsx"));

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Copied Project Comparison"];

                /* Report Header */
                IncrementReportStatus(item, 1);

                int row = 4;
                if (data.Any())
                {
                    Decimal percentageByItem = ((decimal)0.2 * (totalPercentage - 2)) / data.Count();

                    foreach (var inventory in data)
                    {

                        var code = (inventory.jsn_code ?? "").Split('.');
                        worksheet.Cells["A4:I4"].Copy(worksheet.Cells["A" + row + ":I" + row]);
                        worksheet.Cells[row, 1].Value = inventory.drawing_room_number;
                        worksheet.Cells[row, 2].Value = inventory.drawing_room_name;
                        worksheet.Cells[row, 3].Value = inventory.jsn_code;
                        worksheet.Cells[row, 4].Value = inventory.asset_description;
                        worksheet.Cells[row, 5].Value = inventory.resp;
                        worksheet.Cells[row, 6].Value = inventory.pfd_resp;
                        worksheet.Cells[row, 7].Value = inventory.budget_qty;
                        worksheet.Cells[row, 8].Value = inventory.pfd_budget_qty;
                        worksheet.Cells[row, 9].Value = inventory.comment;
                        row++;

                        IncrementReportStatus(item, percentageByItem);

                    }
                }
                xlPackage.Save();
            }

            return excelPath;
        }

        private List<ProjectComparisonList> GetData(project_report report)
        {
            List<ProjectComparisonList> items = new List<ProjectComparisonList>();


            StringBuilder select = new StringBuilder(";WITH CTE AS( ");
            select.Append("SELECT pri.copy_link, pr.drawing_room_number, pr.drawing_room_name, pri.budget_qty, pri.resp, pri.jsn_code, pri.asset_description, pri.comment ");
            select.Append("FROM project_room_inventory pri ");
            select.Append("INNER JOIN project_room pr on pr.project_id = pri.project_id and pr.phase_id = pri.phase_id and pr.department_id = pri.department_id and pr.room_id = pri.room_id and pr.domain_id = pri.domain_id ");
            select.Append("WHERE pri.project_id = " + report.project_id + " and pri.domain_id = " + report.project_domain_id + ") ");
            select.Append(" SELECT ");
            select.Append("	CTE.drawing_room_number + (case when COALESCE(CTE.drawing_room_number, '') = COALESCE(TB2.drawing_room_number,'') or COALESCE(CTE.drawing_room_number, '') = '' or COALESCE(TB2.drawing_room_number,'') = '' then '' else '*' end) as drawing_room_number,  ");
            select.Append("	CTE.drawing_room_name + (case when COALESCE(CTE.drawing_room_name, '') = COALESCE(TB2.drawing_room_name,'') or COALESCE(CTE.drawing_room_name, '') = '' or COALESCE(TB2.drawing_room_name,'') = '' then '' else '*' end) as drawing_room_name, ");
            select.Append("	CTE.budget_qty, CTE.resp, CTE.jsn_code, CTE.asset_description, TB2.pfd_resp, TB2.pfd_budget_qty, CTE.comment ");
            select.Append("FROM CTE CROSS APPLY fn_get_project_comparison_column(" + report.compare_with_project_id + ", CTE.copy_link) AS TB2 ");
            select.Append("UNION ALL ");
            select.Append("SELECT pr.drawing_room_number, pr.drawing_room_name, null as budget_qty, '' as resp, pri.jsn_code, pri.asset_description, pri.resp as pfd_resp, pri.budget_qty as pfd_budget_qty, pri.comment ");
            select.Append("FROM project_room_inventory pri ");
            select.Append("INNER JOIN project_room pr on pr.project_id = pri.project_id and pr.phase_id = pri.phase_id and pr.department_id = pri.department_id and pr.room_id = pri.room_id and pr.domain_id = pri.domain_id ");
            select.Append("WHERE pri.project_id = " + report.compare_with_project_id + " and pri.domain_id = " + report.project_domain_id);
            select.Append(" AND pri.copy_link not in(select copy_link from project_room_inventory ");
            select.Append(" WHERE project_id = " + report.project_id + " and domain_id = " + report.project_domain_id + ") ");
            

            return _db.Database.SqlQuery<ProjectComparisonList>(select.ToString()).ToList();
        }

        private string GetOptionText(int? option_value)
        {
            switch (option_value)
            {
                case 0:
                    return "--";
                case 1:
                    return "Y";
                case 2:
                    return "O";
            }

            return null;
        }
    }
}
