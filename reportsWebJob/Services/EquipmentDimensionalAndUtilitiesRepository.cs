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
    class EquipmentDimensionalAndUtilitiesRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);
            string filename = GetFilename(item);
            List<EquipmentDimensionalAndUtilitiesitem> data = GetData(item);

            string excelfile = BuildExcel(item, filename, data, 94);
            UpdateReportStatus(item, 94);

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                if (UploadToCloud(item, excelfile, "xlsx"))
                {
                    item.file_name = filename;
                    CompleteReport(item);
                }

                fileRepository.DeleteFile(excelfile);
            }
        }

        private string BuildExcel(project_report item, string filename, List<EquipmentDimensionalAndUtilitiesitem> data, Decimal totalPercentage)
        {
            string reportsDirectory = GetReportsDirectory();
            string excelpath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelpath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "equipmentDimensionalAndUtilitiesTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {
                client client = _db.clients.Where(x => x.id == item.project.client_id && x.domain_id == item.project_domain_id).FirstOrDefault();
                facility facility = _db.facilities.Where(x => x.id == item.project.facility_id && x.domain_id == item.project_domain_id).FirstOrDefault();

                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Dimensional and Utilities"];

                if (item.remove_logo == true)
                    worksheet.HeaderFooter.FirstHeader.RightAlignedText = null;
                
                int row = 1;
                worksheet.Cells["A1:AG1"].Copy(worksheet.Cells[string.Format("A{0}:AG{0}", row)]);
                row++;
                worksheet.Cells[row, 5].Value = item.project.project_description;
                row++;

                worksheet.Cells[row, 5].Value = client.name;
                row++;
                worksheet.Cells[row, 5].Value = facility.name;

                row = row + 4;


                foreach (var inventory in data)
                {
                    worksheet.Cells["A8:AG8"].Copy(worksheet.Cells[string.Format("A{0}:AG{0}", row)]);
                    worksheet.Cells[row, 1].Value = inventory.asset_code;
                    worksheet.Cells[row, 2].Value = inventory.placement;
                    worksheet.Cells[row, 3].Value = inventory.resp.TrimEnd();
                    worksheet.Cells[row, 4].Value = inventory.manufacturer_description;
                    worksheet.Cells[row, 5].Value = inventory.asset_description;
                    worksheet.Cells[row, 6].Value = inventory.model_number;
                    worksheet.Cells[row, 7].Value = inventory.model_name;
                    worksheet.Cells[row, 8].Value = inventory.btus;
                    worksheet.Cells[row, 9].Value = inventory.data_desc;
                    worksheet.Cells[row, 10].Value = GetFieldValue(inventory.data_option);
                    worksheet.Cells[row, 11].Value = GetFieldValue(inventory.electrical_option);
                    worksheet.Cells[row, 12].Value = inventory.electrical;
                    worksheet.Cells[row, 13].Value = inventory.volts;
                    worksheet.Cells[row, 14].Value = inventory.phases;
                    worksheet.Cells[row, 15].Value = inventory.hertz;
                    worksheet.Cells[row, 16].Value = inventory.amps;
                    worksheet.Cells[row, 17].Value = inventory.volt_amps;
                    worksheet.Cells[row, 18].Value = inventory.watts;
                    worksheet.Cells[row, 19].Value = inventory.height;
                    worksheet.Cells[row, 20].Value = inventory.width;
                    worksheet.Cells[row, 21].Value = inventory.depth;
                    worksheet.Cells[row, 22].Value = inventory.clearance_top;
                    worksheet.Cells[row, 23].Value = inventory.clearance_back;
                    worksheet.Cells[row, 24].Value = inventory.clearance_front;
                    worksheet.Cells[row, 25].Value = inventory.clearance_bottom;
                    worksheet.Cells[row, 26].Value = inventory.clearance_left;
                    worksheet.Cells[row, 27].Value = inventory.clearance_right;
                    worksheet.Cells[row, 28].Value = inventory.weight;
                    worksheet.Cells[row, 29].Value = GetFieldValue(inventory.plumbing_option);
                    worksheet.Cells[row, 30].Value = inventory.plumbing;
                    worksheet.Cells[row, 31].Value = inventory.plu_cold_water == "1" ? "Yes" : "No";
                    worksheet.Cells[row, 32].Value = inventory.plu_hot_water == "1" ? "Yes" : "No";
                    worksheet.Cells[row, 33].Value = inventory.plu_drain == "1" ? "Yes" : "No";
                    row++;
                }
                
                xlPackage.Save();
                UpdateReportStatus(item, totalPercentage);                
            }

            return excelpath;

        }

        private string GetFieldValue(int? field)
        {
            var fieldValue = "";

            switch (field)
            {
                case 1:
                    fieldValue = "Yes";
                    break;

                case 2:
                    fieldValue = "Optional";
                    break;
                default:
                    break;
            }

            return fieldValue;
        }

        private List<EquipmentDimensionalAndUtilitiesitem> GetData(project_report report)
        {
            List<EquipmentDimensionalAndUtilitiesitem> items = new List<EquipmentDimensionalAndUtilitiesitem>();

            StringBuilder select = new StringBuilder("SELECT asset_code, placement, resp, manufacturer_description, asset_description, model_number, model_name, btus, data_desc, data_option, ");
            select.Append(" electrical_option, electrical, volts, phases, electrical_option, volts, phases, hertz, amps, volt_amps, watts, height, width, depth, weight, plu_hot_water, plu_drain, ");
            select.Append(" clearance_top, clearance_back, clearance_front, clearance_bottom, clearance_left, clearance_right, plumbing_option, plumbing, plu_cold_water ");
            select.Append(" FROM asset_inventory a ");
            select.Append(" WHERE ");
            select.Append(GetWhereClause(report, "a", "a").Replace("WHERE", ""));
            select.Append(" GROUP BY  asset_code, placement, resp, manufacturer_description, asset_description, model_number, model_name, btus, data_desc, data_option, ");
            select.Append(" electrical_option, electrical, volts, phases, electrical_option, volts, phases, hertz, amps, volt_amps, watts, height, width, depth, weight, plu_hot_water, plu_drain, ");
            select.Append(" clearance_top, clearance_back, clearance_front, clearance_bottom, clearance_left, clearance_right, plumbing_option, plumbing, plu_cold_water ");
            select.Append(" ORDER BY asset_description, model_number, model_name ");

            return _db.Database.SqlQuery<EquipmentDimensionalAndUtilitiesitem>(select.ToString()).ToList();



        }
    }
}
