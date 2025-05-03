using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using reportsWebJob.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace reportsWebJob.Services
{
    class RoomByRoomGovernmentRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<RoomByRoomGovernmentItem> data = GetData(item);

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

        private List<RoomByRoomGovernmentItem> GetData(project_report report)
        {
            StringBuilder select = new StringBuilder("SELECT ");
            if (!report.include_budgets)
            {
                select.Append("DISTINCT ");
            }
            select.Append("pri.department_id, pri.room_id, pri.department_description, pri.room_number, pri.room_name, ");
            select.Append("pr.date_added, pri.functional_area, pr.comment, pri.room_code, pri.blueprint, pri.staff, ");
            select.Append("pri.jsn_code, pri.asset_description AS asset_description, ");
            select.Append("COALESCE(pri.resp, '') AS resp, pri.cost_center, pri.ECN, am.eq_unit_desc AS measurement, pri.comment AS asset_comment, ");
            select.Append("COALESCE(pri.jsn_utility1, 'N/A') AS U1, COALESCE(pri.jsn_utility2, 'N/A') AS U2, COALESCE(pri.jsn_utility3,'N/A') AS U3, COALESCE(pri.jsn_utility4, 'N/A') AS U4, COALESCE(pri.jsn_utility5, 'N/A') AS U5, COALESCE(pri.jsn_utility6, 'N/A') AS U6, ");
            select.Append("pri.source_location, SUM(pri.budget_qty) AS planned_qty, p.locked_comment AS submittal, pri.network_option ");
            if (report.include_budgets)
            {
                select.Append(", CAST(pri.unit_budget_adjusted AS DECIMAL(10, 2)) AS unit_cost_adjusted, SUM(pri.total_budget) AS total_cost, (CAST(pri.unit_freight AS DECIMAL(10, 2)) + CAST(pri.unit_install AS DECIMAL(10, 2))) AS extended_cost ");
            }
            select.Append("FROM project_room pr ");
            select.Append("INNER JOIN project p ON p.domain_id = pr.domain_id AND p.project_id = pr.project_id ");
            select.Append("LEFT JOIN asset_inventory pri ON pr.domain_id = pri.domain_id AND pr.project_id = pri.project_id AND pr.phase_id = pri.phase_id AND pr.department_id = pri.department_id AND pr.room_id = pri.room_id ");
            select.Append("LEFT JOIN assets a ON a.domain_id = pri.asset_domain_id AND a.asset_id = pri.asset_id ");
            select.Append("LEFT JOIN assets_measurement am ON a.eq_measurement_id = am.eq_unit_measure_id ");
            select.Append(GetWhereClause(report, "pri", "pri"));
            select.Append(" GROUP BY pri.department_id, pri.room_id, pri.department_description, pri.room_number, pri.room_name, ");
            select.Append("pr.date_added, pri.functional_area, pr.comment, pri.room_code, pri.blueprint, pri.staff, ");
            select.Append("pri.jsn_code, pri.asset_description, ");
            select.Append("COALESCE(pri.resp, ''), pri.cost_center, pri.ECN, am.eq_unit_desc, pri.comment, pri.unit_budget_adjusted, (CAST(pri.unit_freight AS DECIMAL(10, 2)) + CAST(pri.unit_install AS DECIMAL(10, 2))), ");
            select.Append("pri.jsn_utility1, pri.jsn_utility2, pri.jsn_utility3, pri.jsn_utility4, pri.jsn_utility5, pri.jsn_utility6,");
            select.Append("pri.source_location, p.locked_comment, pri.network_option ");
            select.Append(" ORDER BY pri.jsn_code, COALESCE(pri.resp, '')");

            return _db.Database.SqlQuery<RoomByRoomGovernmentItem>(select.ToString()).ToList();
        }

        private double MeasureTextHeight(string text, ExcelFont font, int width)
        {
            if (string.IsNullOrEmpty(text)) return 0.0;
            var bitmap = new Bitmap(1, 1);
            var graphics = Graphics.FromImage(bitmap);

            var pixelWidth = Convert.ToInt32(width * 7.5);  //7.5 pixels per excel column width
            var drawingFont = new Font(font.Name, font.Size);
            var size = graphics.MeasureString(text, drawingFont, pixelWidth);

            //72 DPI and 96 points per inch.  Excel height in points with max of 409 per Excel requirements.
            return Math.Min(Convert.ToDouble(size.Height) * 72 / 96, 409);
        }


        private string checkUtility(string utility)
        {
            if (utility.ToLower() == "n/a" || utility.Length == 0)
                return ".";

            return utility;
        }

        public string BuildExcel(project_report item, string filename, List<RoomByRoomGovernmentItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates",
                item.include_budgets ? "roomByRoomGovernmentBudgetTemplate.xlsx" : "roomByRoomGovernmentTemplate.xlsx"));

            var configData = new
            {
                finalColumn = item.include_budgets ? "S" : "P",
                linesAfterHeader = item.include_budgets ? 0 : 3,
                sourceRoomNoColumn = item.include_budgets ? 10 : 7,
                initialAssetsHeaderRow = 100,
                initialRow = 10
            };

            int assetsHeaderRow = configData.initialAssetsHeaderRow;
            int dataRowFormat = configData.initialAssetsHeaderRow + 1;

            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Room By Room"];

                /* Report Header */
                worksheet.Cells[2, 1].Value = item.project.project_description;
                worksheet.Cells[4, 3].Value = item.name;
                worksheet.Cells[5, 3].Value = item.description;
                worksheet.Cells[6, 3].Value = GetCostCenterInfo(item);
                IncrementReportStatus(item, 1 + (decimal)0.2 * (totalPercentage - 2));

                int row = configData.initialRow;
                Decimal percentageByItem;
                StringBuilder asset_description = new StringBuilder();

                var rooms = data.Select(dt => new
                {
                    submittal = dt.submittal,
                    department_id = dt.department_id,
                    room_id = dt.room_id,
                    department_description = dt.department_description,
                    room_number = dt.room_number,
                    room_name = dt.room_name,
                    staff = dt.staff,
                    date_added = dt.date_added,
                    function_area = dt.functional_area,
                    room_code = dt.room_code,
                    comment = dt.comment,
                    blueprint = dt.blueprint
                }).Distinct().OrderBy(dt => dt.room_number).ThenBy(dt => dt.department_description);

                if (rooms.Count() > 0)
                {
                    percentageByItem = ((decimal)0.7 * (totalPercentage - 2)) / rooms.Count();
                    foreach (var room in rooms)
                    {
                        worksheet.Cells[string.Format("A10:{0}10", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                        worksheet.Cells[row, 2].Value = room.submittal;
                        worksheet.Cells[row, 4].Value = room.date_added.ToShortDateString() ?? "";
                        worksheet.Cells[row, 7].Value = room.department_description;
                        row++;

                        worksheet.Cells[string.Format("A11:{0}11", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                        worksheet.Cells[row, 2].Value = room.room_number;

                        worksheet.Cells[row, 4].Value = room.room_name;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[row, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells[row, 4].Style.WrapText = true;
                        worksheet.Row(row).Height = MeasureTextHeight(room.room_name, worksheet.Cells[row, 4].Style.Font, (int)Math.Ceiling(worksheet.Column(4).Width));

                        worksheet.Cells[row, 7].Value = room.room_code;
                        worksheet.Cells[row, 10].Value = room.blueprint;
                        row++;

                        worksheet.Cells[string.Format("A12:{0}12", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                        worksheet.Cells[row, 2].Value = room.staff;
                        worksheet.Cells[row, 4].Value = room.function_area;
                        worksheet.Cells[row, 7].Value = room.comment;

                        if (item.include_budgets)
                        {
                            row += 2;

                            worksheet.Cells[string.Format("A14:{0}14", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);

                            var costCenters = data.Where(dt => dt.department_id == room.department_id && dt.room_id == room.room_id && !string.IsNullOrEmpty(dt.cost_center))
                                .GroupBy(dt => dt.cost_center).Select(cc => new
                                {
                                    cost_center = cc.Key,
                                    total = cc.Sum(dt => dt.total_cost)
                                });

                            foreach (var costCenter in costCenters)
                            {
                                row++;
                                worksheet.Cells[string.Format("A15:{0}15", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                                worksheet.Cells[row, 1].Value = costCenter.cost_center;
                                worksheet.Cells[row, 4].Value = string.Format("$ {0}", costCenter.total);
                            }

                            row++;
                            worksheet.Cells[string.Format("A15:{0}15", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                            worksheet.Cells[row, 1].Value = "Total";
                            worksheet.Cells[row, 4].Value = string.Format("$ {0}", costCenters.Sum(cc => cc.total));

                            row++;
                            worksheet.Cells[string.Format("A1:{0}1", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                            row++;

                        }

                        row += configData.linesAfterHeader;

                        IEnumerable<RoomByRoomGovernmentItem> items = data.Where(dt => dt.department_id == room.department_id && dt.room_id == room.room_id);

                        worksheet.Cells[string.Format("A{1}:{0}{1}", configData.finalColumn, assetsHeaderRow)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                        assetsHeaderRow = row;
                        row++;

                        foreach (RoomByRoomGovernmentItem it in items)
                        {
                            // Igore items with quantity 0
                            if (it.planned_qty == 0)
                            {
                                continue;
                            }

                            worksheet.Cells[string.Format("A{1}:{0}{1}", configData.finalColumn, dataRowFormat)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                            dataRowFormat = row;

                            worksheet.Cells[row, 1].Value = it.jsn_code;
                            worksheet.Cells[row, 2].Value = it.asset_description;
                            worksheet.Cells[row, 3].Value = it.planned_qty;
                            worksheet.Cells[row, 4].Value = it.measurement;
                            worksheet.Cells[row, 5].Value = it.resp;
                            worksheet.Cells[row, 6].Value = it.cost_center;
                            if (item.include_budgets)
                            {
                                worksheet.Cells[row, 7].Value = string.Format("$ {0}", it.unit_cost_adjusted);
                                worksheet.Cells[row, 8].Value = string.Format("$ {0}", it.extended_cost);
                                worksheet.Cells[row, 9].Value = string.Format("$ {0}", it.total_cost);
                            }
                            worksheet.Cells[row, configData.sourceRoomNoColumn].Value = "";
                            if (!string.IsNullOrEmpty(it.source_location))
                            {
                                var idx = it.source_location.IndexOf("}||");
                                it.source_location = it.source_location.Substring(0, idx + 1);
                                worksheet.Cells[row, configData.sourceRoomNoColumn].Value = JsonConvert.DeserializeObject<SourceLocation>(it.source_location)?.drawing_room_number;
                            }
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 1].Value = it.ECN;
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 2].Value = it.asset_comment;
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 3].Value = checkUtility(it.U1);
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 4].Value = checkUtility(it.U2);
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 5].Value = checkUtility(it.U3);
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 6].Value = checkUtility(it.U4);
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 7].Value = checkUtility(it.U5);
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 8].Value = checkUtility(it.U6);
                            worksheet.Cells[row, configData.sourceRoomNoColumn + 9].Value = it.network_option == 1 ? "X" : "";

                            row++;
                        }

                        // Insert Page break
                        worksheet.Row(row).PageBreak = true;

                        // Copy first line to make sure the lines will be empty 
                        worksheet.Cells[string.Format("A1:{0}1", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                        row++;

                        worksheet.Cells[string.Format("A1:{0}1", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{0}:{1}{0}", row, configData.finalColumn)]);
                        row++;
                        IncrementReportStatus(item, percentageByItem);
                    }
                }
                else
                {
                    IncrementReportStatus(item, (decimal)0.7 * (totalPercentage - 2));

                    worksheet.Cells[string.Format("A1:{0}1", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{1}:{0}{1}", configData.finalColumn, configData.initialAssetsHeaderRow)]);
                    worksheet.Cells[string.Format("A1:{0}1", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{1}:{0}{1}", configData.finalColumn, configData.initialAssetsHeaderRow + 1)]);

                    // Delete the examples rows
                    worksheet.DeleteRow(row, 11);
                    worksheet.Cells[row, 1].Value = "There are no rooms with assets on it";
                }

                // Remove the line that contains the header example
                if (row < configData.initialAssetsHeaderRow)
                {
                    worksheet.Cells[string.Format("A1:{0}1", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{1}:{0}{1}", configData.finalColumn, configData.initialAssetsHeaderRow)]);
                }

                if (row < configData.initialAssetsHeaderRow + 1)
                {
                    worksheet.Cells[string.Format("A1:{0}1", configData.finalColumn)].Copy(worksheet.Cells[string.Format("A{1}:{0}{1}", configData.finalColumn, configData.initialAssetsHeaderRow + 1)]);
                }

                xlPackage.Save();
                UpdateReportStatus(item, totalPercentage);
            }

            return excelPath;
        }
    }
}
