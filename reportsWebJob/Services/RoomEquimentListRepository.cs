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
    class RoomEquimentListRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);
            string filename = GetFilename(item);
            List<RoomEquipmentListItem> data = GetData(item);

            string excelfile = BuildExcel(item, filename, data, 94);
            UpdateReportStatus(item, 94);

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                if(UploadToCloud(item, excelfile, "xlsx"))
                {
                    item.file_name = filename;
                    CompleteReport(item);
                }

                fileRepository.DeleteFile(excelfile);
            }
        }

        private string BuildExcel(project_report item, string filename, List<RoomEquipmentListItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "roomEquipmentListTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template)) 
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Room Equipment List"];
                if (item.remove_logo == true)
                    worksheet.HeaderFooter.OddHeader.RightAlignedText = null;

                var room_id = 0;
                int row = 1;

                client client = _db.clients.Where(x => x.id == item.project.client_id && x.domain_id == item.project_domain_id).FirstOrDefault();
                facility facility = _db.facilities.Where(x => x.id == item.project.facility_id && x.domain_id == item.project_domain_id).FirstOrDefault();

                foreach (var inventory in data)
                {
                    if (room_id != inventory.room_id)
                    {
                        if (row > 1)
                        {
                            worksheet.Row(row).PageBreak = true;
                            row = row + 2;
                        }

                        room_id = inventory.room_id;
                        worksheet.Cells["A1:H6"].Copy(worksheet.Cells[string.Format("A{0}:H{0}", row)]);
                        row++;

                        worksheet.Cells[row, 3].Value = inventory.room_name;
                        worksheet.Cells[row, 5].Value = item.project.project_description;
                        row++;

                        worksheet.Cells[row, 3].Value = inventory.room_number;
                        worksheet.Cells[row, 5].Value = client.name;
                        row++;

                        worksheet.Cells[row, 3].Value = inventory.department_description;
                        worksheet.Cells[row, 5].Value = facility.name;
                        row = row + 3;
                    }

                    worksheet.Cells["A7:H7"].Copy(worksheet.Cells[string.Format("A{0}:H{0}", row)]);
                    worksheet.Cells[row, 1].Value = inventory.resp;
                    worksheet.Cells[row, 2].Value = inventory.asset_code;
                    worksheet.Cells[row, 3].Value = inventory.asset_description;
                    worksheet.Cells[row, 4].Value = inventory.manufacturer_description;
                    worksheet.Cells[row, 5].Value = inventory.model_name;
                    worksheet.Cells[row, 6].Value = inventory.model_number;
                    worksheet.Cells[row, 7].Value = inventory.budget_qty;
                    row++;

                }

                xlPackage.Save();
                UpdateReportStatus(item, totalPercentage);

            }

            return excelPath;
        }
       

        private List<RoomEquipmentListItem> GetData(project_report report)
        {
            List<RoomEquipmentListItem> items = new List<RoomEquipmentListItem>();

            StringBuilder select = new StringBuilder("SELECT room_id, room_name, room_number, department_description, d.contact_name, resp, cost_center,  ");
            select.Append("asset_code, manufacturer_description, model_name as model_name, model_number as model_number, asset_description, budget_qty, f.name AS facility  ");
            select.Append("FROM asset_inventory a ");
            select.Append("JOIN project_department d ON a.department_id = d.department_id ");
            select.Append("JOIN project p ON a.project_id = p.project_id ");
            select.Append("JOIN facility f ON p.facility_id = f.id ");
            select.Append("WHERE ");
            select.Append(GetWhereClause(report, "a", "a").Replace("WHERE", ""));
            select.Append("ORDER BY phase_description, department_description, room_number, room_name");

            return _db.Database.SqlQuery<RoomEquipmentListItem>(select.ToString()).ToList();


        }
    }

}
