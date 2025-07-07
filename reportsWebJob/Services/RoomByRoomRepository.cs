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
    class RoomByRoomRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<RoomByRoomItem> data = GetData(item);

            string excelFile = BuildExcel(item, filename, data, 94); // change to 47 when Build pdf is working
            //string pdfFile = BuildPDF(item, filename, data);UpdateReportStatus(item, 94);
            UpdateReportStatus(item, 94);

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {

                if (UploadToCloud(item, excelFile, "xlsx"))
                {
                    //if (UploadToCloud(item, pdfFile, "pdf"))
                    //{
                    item.file_name = filename;
                    CompleteReport(item);
                    //}
                }


                fileRepository.DeleteFile(excelFile);
                //fileRepository.DeleteFile(pdfFile);
            }
        }

        public string BuildExcel(project_report item, string filename, List<RoomByRoomItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "roomByRoomTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            var allProjectData = data.Select(dt => new
            {
                resp = dt.resp,
                asset_code = dt.asset_code,
                cad_id = dt.cad_id,
                jsn_code = dt.jsn_code,
                asset_description = dt.asset_description,
                budget_qty = data.Where(dt1 => ((!string.IsNullOrEmpty(dt1.jsn_code) && dt1.jsn_code.Equals(dt.jsn_code)) || (string.IsNullOrEmpty(dt1.jsn_code) && dt1.asset_code.Equals(dt.asset_code))) && dt1.resp.Equals(dt.resp) && dt1.tag == dt.tag).Sum(dt1 => dt1.budget_qty),
                lease_qty = data.Where(dt1 => ((!string.IsNullOrEmpty(dt1.jsn_code) && dt1.jsn_code.Equals(dt.jsn_code)) || (string.IsNullOrEmpty(dt1.jsn_code) && dt1.asset_code.Equals(dt.asset_code))) && dt1.resp.Equals(dt.resp) && dt1.tag == dt.tag).Sum(dt1 => dt1.lease_qty),
                dnp_qty = data.Where(dt1 => ((!string.IsNullOrEmpty(dt1.jsn_code) && dt1.jsn_code.Equals(dt.jsn_code)) || (string.IsNullOrEmpty(dt1.jsn_code) && dt1.asset_code.Equals(dt.asset_code))) && dt1.resp.Equals(dt.resp) && dt1.tag == dt.tag).Sum(dt1 => dt1.dnp_qty), 
                po_qty = data.Where(dt1 => ((!string.IsNullOrEmpty(dt1.jsn_code) && dt1.jsn_code.Equals(dt.jsn_code)) || (string.IsNullOrEmpty(dt1.jsn_code) && dt1.asset_code.Equals(dt.asset_code))) && dt1.resp.Equals(dt.resp) && dt1.tag == dt.tag).Sum(dt1 => dt1.po_qty),
                tag = dt.tag,
                manufacturer_description = dt.manufacturer_description,
                model_name = dt.model_name,
                model_number = dt.model_number,
                water_option = dt.water_option,
                plumbing_option = dt.plumbing_option,
                data_option = dt.data_option,
                electrical_option = dt.electrical_option,
                mobile_option = dt.mobile_option,
                blocking_option = dt.blocking_option,
                medgas_option = dt.medgas_option,
                supports_option = dt.supports_option
            }).Distinct().ToList();

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Room By Room"];

                /* Report Header */
                worksheet.Cells[46, 1].Value = item.project.project_description;
                worksheet.Cells[48, 3].Value = item.name;
                worksheet.Cells[49, 3].Value = item.description;
                worksheet.Cells[50, 3].Value = GetCostCenterInfo(item);
                IncrementReportStatus(item, 1);

                int row = 56;
                Decimal percentageByItem;
                StringBuilder asset_description = new StringBuilder();

                if (allProjectData.Count() > 0)
                {
                    percentageByItem = ((decimal)0.2 * (totalPercentage - 2)) / allProjectData.Count();
                    foreach (var dt in allProjectData)
                    {
                        worksheet.Cells["A56:Q56"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                        worksheet.Cells[row, 1].Value = dt.resp;
                        worksheet.Cells[row, 2].Value = dt.asset_code;
                        worksheet.Cells[row, 3].Value = dt.cad_id;
                        worksheet.Cells[row, 4].Value = dt.jsn_code;
                        asset_description.Clear();
                        asset_description.Append(dt.asset_description);
                        if (!string.IsNullOrEmpty(dt.tag))
                        {
                            asset_description.Append(" (");
                            asset_description.Append(dt.tag);
                            asset_description.Append(") ");
                        }
                        asset_description.Append("\n");
                        asset_description.Append(dt.manufacturer_description);
                        if (!string.IsNullOrEmpty(dt.model_name) || !string.IsNullOrEmpty(dt.model_number))
                        {
                            asset_description.Append(" (");
                            if (!string.IsNullOrEmpty(dt.model_number))
                            {
                                asset_description.Append(dt.model_number);
                                if (!string.IsNullOrEmpty(dt.model_name))
                                    asset_description.Append("/");
                            }
                            if (!string.IsNullOrEmpty(dt.model_name))
                                asset_description.Append(dt.model_name);
                            asset_description.Append(")");
                        }

                        worksheet.Cells[row, 5].Value = asset_description.ToString();
                        worksheet.Cells[row, 6].Value = dt.budget_qty;
                        worksheet.Cells[row, 7].Value = dt.lease_qty;
                        worksheet.Cells[row, 8].Value = dt.dnp_qty;
                        worksheet.Cells[row, 9].Value = CalcNetNew(dt.resp, dt.budget_qty, dt.dnp_qty);
                        worksheet.Cells[row, 10].Value = dt.po_qty;
                        worksheet.Cells[row, 11].Value = GetOptionText(dt.electrical_option);
                        worksheet.Cells[row, 12].Value = GetOptionText(dt.data_option);
                        worksheet.Cells[row, 13].Value = GetOptionText(dt.water_option);
                        worksheet.Cells[row, 14].Value = GetOptionText(dt.plumbing_option);
                        worksheet.Cells[row, 15].Value = GetOptionText(dt.medgas_option);
                        worksheet.Cells[row, 16].Value = GetOptionText(dt.blocking_option);
                        worksheet.Cells[row, 17].Value = GetOptionText(dt.supports_option);

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

                var rooms = data.Select(dt => new
                {
                    phase_id = dt.phase_id,
                    department_id = dt.department_id,
                    room_id = dt.room_id,
                    phase_description = dt.phase_description,
                    department_description = dt.department_description,
                    drawing_room_number = dt.drawing_room_number,
                    drawing_room_name = dt.drawing_room_name
                }).Distinct().OrderBy(dt => dt.drawing_room_number).ThenBy(dt => dt.phase_description).ThenBy(dt => dt.department_description);

                if (rooms.Count() > 0)
                {
                    percentageByItem = ((decimal)0.7 * (totalPercentage - 2)) / rooms.Count();
                    foreach (var room in rooms)
                    {
                        worksheet.Cells["A45:Q45"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                        worksheet.Cells[row, 1].Value = room.phase_description + " - " + room.department_description + " - " + room.drawing_room_name;
                        row++;
                        worksheet.Cells["A44:Q44"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                        worksheet.Cells[row, 1].Value = "Room Number " + room.drawing_room_number;
                        row++;
                        worksheet.Cells["A54:Q54"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                        row++;
                        worksheet.Cells["A55:Q55"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                        row++;

                        IEnumerable<RoomByRoomItem> items = data.Where(dt => dt.phase_id == room.phase_id && dt.department_id == room.department_id && dt.room_id == room.room_id);

                        foreach (RoomByRoomItem it in items)
                        {
                            worksheet.Cells["A56:Q56"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                            worksheet.Cells[row, 1].Value = it.resp;
                            worksheet.Cells[row, 2].Value = it.asset_code;
                            worksheet.Cells[row, 3].Value = it.cad_id;
                            worksheet.Cells[row, 4].Value = it.jsn_code;
                            asset_description.Clear();
                            asset_description.Append(it.asset_description);
                            if (it.tag != null && !it.tag.Equals(""))
                            {
                                asset_description.Append(" (");
                                asset_description.Append(it.tag);
                                asset_description.Append(") ");
                            }
                            asset_description.Append("\n");
                            asset_description.Append(it.manufacturer_description);
                            if (!string.IsNullOrEmpty(it.model_name) || !string.IsNullOrEmpty(it.model_number))
                            {
                                asset_description.Append(" (");
                                if (!string.IsNullOrEmpty(it.model_number))
                                {
                                    asset_description.Append(it.model_number);
                                    if (!string.IsNullOrEmpty(it.model_name))
                                        asset_description.Append("/");
                                }
                                if (!string.IsNullOrEmpty(it.model_name))
                                    asset_description.Append(it.model_name);
                                asset_description.Append(")");
                            }
                            worksheet.Cells[row, 5].Value = asset_description.ToString();
                            worksheet.Cells[row, 6].Value = it.budget_qty;
                            worksheet.Cells[row, 7].Value = it.lease_qty;
                            worksheet.Cells[row, 8].Value = it.dnp_qty;
                            worksheet.Cells[row, 9].Value = CalcNetNew(it.resp, it.budget_qty, it.dnp_qty);
                            worksheet.Cells[row, 10].Value = it.po_qty;
                            worksheet.Cells[row, 11].Value = GetOptionText(it.electrical_option);
                            worksheet.Cells[row, 12].Value = GetOptionText(it.data_option);
                            worksheet.Cells[row, 13].Value = GetOptionText(it.water_option);
                            worksheet.Cells[row, 14].Value = GetOptionText(it.plumbing_option);
                            worksheet.Cells[row, 15].Value = GetOptionText(it.medgas_option);
                            worksheet.Cells[row, 16].Value = GetOptionText(it.blocking_option);
                            worksheet.Cells[row, 17].Value = GetOptionText(it.supports_option);

                            row++;
                        }

                        row += 2;
                        IncrementReportStatus(item, percentageByItem);
                    }
                } else
                {
                    IncrementReportStatus(item, (decimal)0.7 * (totalPercentage - 2));
                }

                worksheet.Cells["A45:Q45"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                worksheet.Cells[row, 1].Value = "The following rooms do not have any scheduled asset inventory:";
                row++;

                IEnumerable<report_location> roomsNoAsset = item.report_location.Where(pr => !rooms.Any(r => r.phase_id == pr.phase_id && r.department_id == pr.department_id && r.room_id == pr.room_id));

                foreach (var loc in roomsNoAsset)
                {
                    var room = loc.GetRoom(_db);
                    worksheet.Cells["A43:Q43"].Copy(worksheet.Cells["A" + row + ":Q" + row]);
                    worksheet.Cells[row, 1].Value = String.Format("{0} - {1} - {2}: {3}", room.project_department.project_phase.description,
                        room.project_department.description, room.drawing_room_name, room.drawing_room_number);
                    row++;
                }
                IncrementReportStatus(item, ((decimal)0.1 * (totalPercentage - 2)));

                //HIDE COLUMN JSN CODE IF THERE IS NO JSN ON THE DATABASE
                if (data.Find(x => x.jsn_code != "") == null)
                {
                    worksheet.Column(4).Hidden = true;
                }

                xlPackage.Save();
                UpdateReportStatus(item, totalPercentage);
            }

            return excelPath;
        }

        private List<RoomByRoomItem> GetData(project_report report)
        {
            List<RoomByRoomItem> items = new List<RoomByRoomItem>();

            StringBuilder select = new StringBuilder("SELECT phase_id, department_id, room_id, phase_description, department_description, drawing_room_number, drawing_room_name, ");
            select.Append("COALESCE(iwr.cad_id, '') AS cad_id, coalesce(iwr.jsn_code, '') as jsn_code, a.water_option, a.plumbing_option, a.data_option, a.electrical_option, a.mobile_option, a.blocking_option, a.medgas_option, ");
            select.Append("a.supports_option, COALESCE(iwr.resp, '') AS resp, COALESCE(iwr.asset_code, '') AS asset_code, iwr.asset_description, ");
            select.Append("sum(iwr.budget_qty_sf) AS budget_qty, sum(iwr.lease_qty_sf) AS lease_qty, sum(iwr.dnp_qty_sf) AS dnp_qty, sum(iwr.po_qty_sf) AS po_qty, COALESCE(iwr.tag, '') AS tag, ");
            select.Append("iwr.manufacturer_description, COALESCE(iwr.model_number, '') AS model_number, COALESCE(iwr.model_name, '') AS model_name ");
            select.Append("FROM inventory_w_relo_v iwr INNER JOIN assets a ON a.asset_id = iwr.asset_id AND iwr.asset_domain_id = a.domain_id ");
            select.Append("WHERE iwr.type = 'NEW' AND ");
            select.Append(GetWhereClause(report, "iwr", "iwr").Replace("WHERE", ""));
            select.Append(" GROUP BY phase_id, department_id, room_id, phase_description, department_description, drawing_room_number, drawing_room_name, ");
            select.Append("iwr.cad_id, iwr.jsn_code, iwr.resp, iwr.asset_code, iwr.asset_description, a.water_option, a.plumbing_option, a.data_option, a.electrical_option, ");
            select.Append("a.mobile_option, a.blocking_option, a.medgas_option, a.supports_option, iwr.tag, iwr.manufacturer_description, iwr.model_number, iwr.model_name ");
            select.Append("ORDER BY iwr.jsn_code, iwr.asset_code, iwr.resp");

            return _db.Database.SqlQuery<RoomByRoomItem>(select.ToString()).ToList();
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
