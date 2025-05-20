using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using reportsWebJob.Models;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace reportsWebJob.Services
{
    class AssetByRoomRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<AssetByRoomItem> data = GetData(item);

            string excelFile = BuildExcel(item, filename, data, 94); // change to 47 when Build pdf is working
            //UpdateReportStatus(item, 47);
            //string pdfFile = BuildPDF(item, filename, data);UpdateReportStatus(item, 94);
            UpdateReportStatus(item, 94);

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {

                if (UploadToCloud(item, excelFile, "xlsx"))
                {
                    //if (genRepository.UploadToCloud(item, pdfFile, "pdf"))
                    //{
                    item.file_name = filename;
                    CompleteReport(item);
                    //}
                }


                fileRepository.DeleteFile(excelFile);
                //fileRepository.DeleteFile(pdfFile);
            }
        }

        public string BuildExcel(project_report item, string filename, List<AssetByRoomItem> data, Decimal totalPercetage)
        {
            string reportsDirectory = Path.Combine(Domain.GetRoot(), "reports");
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "assetByRoomTemplate.xlsx"));

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Asset By Room"];

                int first_total_row = -1;
                int first_header_row = -1;

                /* Report Header */
                worksheet.Cells[2, 2].Value = item.project.project_description;
                worksheet.Cells[4, 4].Value = item.name;
                worksheet.Cells[5, 4].Value = item.description;
                worksheet.Cells[6, 4].Value = GetCostCenterInfo(item);

                int row = 10;
                if (data.Count() > 0)
                {
                    Decimal percentageByItem = (totalPercetage - 2) / data.Count();
                    foreach (AssetByRoomItem dt in data)
                    {
                        worksheet.Cells["A10:P10"].Copy(worksheet.Cells["A" + row + ":P" + row]);
                        worksheet.Cells[row, 1].Value = $"{dt.asset_information.asset_code} - {dt.asset_information.asset_description}";
                        row++;
                        if (dt.asset_information.serial_number != null && !dt.asset_information.serial_number.Equals(""))
                        {
                            worksheet.Cells["A10:P10"].Copy(worksheet.Cells["A" + row + ":P" + row]);
                            worksheet.Cells[row, 1].Value = $"{dt.asset_information.manufacturer_description}: {dt.asset_information.serial_number}";
                        }
                        row++;
                        if (dt.asset_information.comment != null && !dt.asset_information.comment.Equals(""))
                        {
                            worksheet.Cells["A10:P10"].Copy(worksheet.Cells["A" + row + ":P" + row]);
                            worksheet.Cells[row, 1].Value = dt.asset_information.comment;
                        }
                        row++;
                        if (first_header_row == -1)
                        {
                            worksheet.Cells["A13:P13"].Copy(worksheet.Cells["A" + row + ":P" + row]);
                            first_header_row = row;
                        }
                        else
                        {
                            worksheet.Cells["A" + first_header_row + ":P" + first_header_row].Copy(worksheet.Cells["A" + row + ":P" + row]);
                        }
                        row++;
                        worksheet.Cells["A14:P14"].Copy(worksheet.Cells["A" + row + ":P" + row]);
                        worksheet.Cells[row, 1].Value = dt.asset_information.eq_unit_desc;
                        worksheet.Cells[row, 2].Value = $"{dt.asset_information.height} | {dt.asset_information.width} | {dt.asset_information.depth}";
                        worksheet.Cells[row, 4].Value = $"{ConvertInchToCm(dt.asset_information.height)} | {ConvertInchToCm(dt.asset_information.width)} | {ConvertInchToCm(dt.asset_information.depth)}";
                        worksheet.Cells[row, 6].Value = dt.asset_information.weight;
                        worksheet.Cells[row, 8].Value = ConvertPoundsToKg(dt.asset_information.weight);
                        worksheet.Cells[row, 10].Value = GetDefaultValue(dt.asset_information.electrical_option);
                        worksheet.Cells[row, 11].Value = GetDefaultValue(dt.asset_information.data_option);
                        worksheet.Cells[row, 12].Value = GetDefaultValue(dt.asset_information.water_option);
                        worksheet.Cells[row, 13].Value = GetDefaultValue(dt.asset_information.plumbing_option);
                        worksheet.Cells[row, 14].Value = GetDefaultValue(dt.asset_information.medgas_option);
                        worksheet.Cells[row, 15].Value = GetDefaultValue(dt.asset_information.blocking_option);
                        worksheet.Cells[row, 16].Value = GetDefaultValue(dt.asset_information.supports_option);

                        row++;
                        worksheet.Cells["A15:P15"].Copy(worksheet.Cells["A" + row + ":P" + row]);
                        row++;

                        if (first_total_row == -1)
                        {
                            first_total_row = row + dt.rooms.Count();
                            worksheet.Cells["A17:P17"].Copy(worksheet.Cells["A" + first_total_row + ":P" + first_total_row]);
                        }

                        foreach (AssetByRoomLocation room in dt.rooms)
                        {
                            worksheet.Cells["A16:P16"].Copy(worksheet.Cells["A" + row + ":P" + row]);
                            worksheet.Cells[row, 1].Value = room.resp;
                            worksheet.Cells[row, 3].Value = room.phase_description;
                            worksheet.Cells[row, 6].Value = room.department_description;
                            worksheet.Cells[row, 9].Value = room.drawing_room_number;
                            worksheet.Cells[row, 11].Value = room.drawing_room_name;
                            worksheet.Cells[row, 13].Value = room.total;
                            worksheet.Cells[row, 14].Value = room.lease;
                            worksheet.Cells[row, 15].Value = room.dnp;
                            if (room.resp.Equals("EXOI") || room.resp.Equals("EXCI") || room.resp.Equals("EXVI")
                                || room.resp.Equals("EXEX"))
                                worksheet.Cells[row, 16].Value = 0;
                            else
                                worksheet.Cells[row, 16].Value = room.total - room.dnp;
                            row++;
                        }
                        worksheet.Cells["A" + first_total_row + ":P" + first_total_row].Copy(worksheet.Cells["A" + row + ":P" + row]);
                        worksheet.Cells[row, 13].Value = dt.rooms.Sum(r => r.total);
                        worksheet.Cells[row, 14].Value = dt.rooms.Sum(r => r.lease);
                        worksheet.Cells[row, 15].Value = dt.rooms.Sum(r => r.dnp);
                        worksheet.Cells[row, 16].Value = dt.rooms.Sum(r => (
                            r.resp.Equals("EXOI") || r.resp.Equals("EXCI") || r.resp.Equals("EXVI") || r.resp.Equals("EXEX"))
                            ? 0 : (r.total - r.dnp));

                        row += 2;
                        IncrementReportStatus(item, percentageByItem);
                    }
                }
                else {
                    IncrementReportStatus(item, totalPercetage - 2);
                }

                xlPackage.Save();
                IncrementReportStatus(item, 2);
            }

            return excelPath;
        }

        private List<AssetByRoomItem> GetData(project_report report)
        {
            List<AssetByRoomItem> allInformation = new List<AssetByRoomItem>();

            string useCadId = report.use_cad_id == true ? "COALESCE(cad_id, asset_code) AS asset_code," : "asset_code,";

            string query = $@"
                CREATE TABLE #TempRoomIds (Id INT);
                INSERT INTO #TempRoomIds (Id) SELECT value FROM STRING_SPLIT(@roomIds, ',');

                SELECT DISTINCT 
                    asset_domain_id,
                    asset_id,
                    {useCadId}
                    asset_description,
                    manufacturer_description,
                    COALESCE(serial_name,'') AS serial_name,
                    COALESCE(serial_number,'') AS serial_number,
                    asset_comment AS comment,
                    eq_unit_desc,
                    height,
                    width,
                    depth,
                    weight,
                    electrical_option,
                    data_option,
                    water_option,
                    plumbing_option,
                    medgas_option,
                    blocking_option,
                    supports_option                    
                FROM asset_inventory
                WHERE
                    domain_id = @domainId
                    AND project_id = @projectId
                    AND room_id IN (SELECT Id FROM #TempRoomIds)
                GROUP BY
                    asset_domain_id,
                    asset_id,
                    asset_description,
                    manufacturer_description,
                    serial_name,
                    serial_number,
                    asset_comment,
                    eq_unit_desc,
                    height,
                    width,
                    depth,
                    weight,
                    electrical_option,
                    data_option,
                    water_option,
                    plumbing_option,
                    medgas_option,
                    blocking_option,
                    supports_option,
                    asset_code
                ORDER BY
                    asset_code;

                DROP TABLE #TempRoomIds;
            ";

            var roomIds = report.report_location.Select(r => r.room_id).ToList();
            var roomIdsString = string.Join(",", roomIds);

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@domainId", report.project_domain_id),
                new SqlParameter("@projectId", report.project_id),
                new SqlParameter("@roomIds", roomIdsString)
            };

            List<AssetByRoomInventory> items = _db.Database.SqlQuery<AssetByRoomInventory>(query, parameters.ToArray()).ToList();

            //Query to get rooms by asset
            string subQuery = $@"
                    CREATE TABLE #TempRoomIds (Id INT);
                    INSERT INTO #TempRoomIds (Id) SELECT value FROM STRING_SPLIT(@roomIds, ',');

                    SELECT        
	                    resp,
	                    phase_description,
	                    department_description,
	                    drawing_room_number,
	                    drawing_room_name,
	                    SUM(budget_qty) AS total,
	                    SUM(lease_qty) AS lease,
	                    SUM(dnp_qty) AS dnp

                    FROM
	                    inventory_w_relo_v

                    WHERE
	                    domain_id = @domainId
                        AND project_id = @projectId
                        AND room_id IN (SELECT Id FROM #TempRoomIds)
                        AND asset_domain_id = @asset_domain_id
                        AND asset_id = @asset_id
                        AND asset_description = @asset_description
                        AND ISNULL(serial_name, '') = ISNULL(@serial_name, '')
                        AND ISNULL(serial_number, '') = ISNULL(@serial_number, '')
                    GROUP BY
	                    resp,
	                    phase_description,
	                    department_description,
	                    drawing_room_number,
	                    drawing_room_name
                    ORDER BY 
	                    resp,
	                    phase_description,
	                    department_description,
	                    drawing_room_number,
	                    drawing_room_name

                    DROP TABLE #TempRoomIds;                  

                ";

            foreach (var asset in items)
            {
                
                parameters = null;
                parameters = new List<SqlParameter>
                {
                    new SqlParameter("@domainId", report.project_domain_id),
                    new SqlParameter("@projectId", report.project_id),
                    new SqlParameter("@roomIds", roomIdsString),
                    new SqlParameter("@asset_domain_id", asset.asset_domain_id),
                    new SqlParameter("@asset_id", asset.asset_id),
                    new SqlParameter("@asset_description", (object)asset.asset_description ?? DBNull.Value),
                    new SqlParameter("@serial_name", (object)asset.serial_name ?? DBNull.Value),
                    new SqlParameter("@serial_number", (object)asset.serial_number ?? DBNull.Value)
                };

                List<AssetByRoomLocation> rooms = _db.Database.SqlQuery<AssetByRoomLocation>(subQuery, parameters.ToArray()).ToList();
                allInformation.Add(new AssetByRoomItem(asset, rooms));
            }
            return allInformation;
        }

        public static string GetDefaultValue(int? input) =>
            input == 1 ? "Y" : input == 2 ? "O" : "--";

        public static string ConvertInchToCm(string inches)
        {
            if (string.IsNullOrEmpty(inches))
                return null;

            double aux;
            if (double.TryParse(inches, NumberStyles.Any, CultureInfo.InvariantCulture, out aux))
                return (aux * 2.54).ToString(CultureInfo.InvariantCulture);

            return null;
        }

        public static string ConvertPoundsToKg(string pounds)
        {
            if (string.IsNullOrEmpty(pounds))
                return null;

            double aux;
            if (double.TryParse(pounds, NumberStyles.Any, CultureInfo.InvariantCulture, out aux))
                return (aux * 0.453592).ToString(CultureInfo.InvariantCulture);

            return null;
        }
    }
}
