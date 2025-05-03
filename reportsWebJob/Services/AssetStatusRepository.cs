using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using OfficeOpenXml;
using System.IO;
using reportsWebJob.Models;
using System.Globalization;

namespace reportsWebJob.Services
{
    class AssetStatusRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<AssetStatusItem> data = GetData(item, 20);
            UpdateReportStatus(item, 20);

            string excelFile = BuildExcel(item, filename, data, 38);
            UpdateReportStatus(item, 58);
            string pdfFile = BuildPDF(item, filename, data, 38);
            UpdateReportStatus(item, 96);

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {

                if (UploadToCloud(item, excelFile, "xlsx"))
                {
                    if (UploadToCloud(item, pdfFile, "pdf"))
                    {
                        item.file_name = filename;
                        CompleteReport(item);
                    }
                    else {
                        CompleteReportError(item);
                    }
                }
                else {
                    CompleteReportError(item);
                }

                fileRepository.DeleteFile(excelFile);
                fileRepository.DeleteFile(pdfFile);
            }
        }

        public string BuildPDF(project_report item, string filename, List<AssetStatusItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = Path.Combine(Domain.GetRoot(), "reports");
            string pdfPath = Path.Combine(reportsDirectory, filename + ".pdf");
            string htmlPath = pdfPath.Replace(".pdf", ".html");

            StringWriter stringWriter = new StringWriter();


            stringWriter.Write("<html><head><meta charset='UTF-8'>");
            stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "css", "AssetStatus.css") + "' rel='stylesheet' />");
            stringWriter.Write("</head><body style=font-family:Arial; font-size:8pt;>");
            stringWriter.Write("<table width=\"100%\" border=\"0px\">");

            /* Report Header */
            stringWriter.Write("<tr><td><table width=\"100%\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>");
            stringWriter.Write("<table width=\"350px\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td colspan=\"2\"><h2>" + item.name + " - Asset Status Report</h2></td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr><td colspan=\"2\">&nbsp;</td></tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>Description:</td>");
            stringWriter.Write("<td>" + item.description + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>Cost Center:</td>");
            stringWriter.Write("<td>" + GetCostCenterInfo(item) + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td>");
            stringWriter.Write("<td>");
            stringWriter.Write("<img height=\"40\" src='" + Path.Combine(Domain.GetRoot(), "images/logo_aw.png") + "'/>");
            stringWriter.Write("</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table></td></tr>");

            /* Data Header */
            stringWriter.Write("<tr><td>");
            stringWriter.Write("<table width=\"100%\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td width=\"1200px\" valign=\"top\">");
            stringWriter.Write("<table>");
            stringWriter.Write("<tr><td>");
            stringWriter.Write("<table width=\"1200px\" class=\"bordertab\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Resp</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\"><font color=\"white\">Code</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">CAD ID</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Description</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Planned Qty</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Lease Qty</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">DNP Qty</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Net New</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">PO Qty</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Total Budget Amt</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Total PO Amt</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Buyout Delta</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">EQ Status</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">PO Status</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">PO Comment</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">PO No.</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Cost Center</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Vendor</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Manufacturer</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Model No.</font></th>");
            stringWriter.Write("<th bgcolor=\"#7C8692\" align=\"center\"><font color=\"white\">Model Name</font></th>");
            stringWriter.Write("</tr>");

            /* Content */
            if (data.Count() > 0)
            {
                Decimal percentageByItem = (totalPercentage - 5) / data.Count();
                foreach (AssetStatusItem dt in data)
                {
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.resp + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.asset_code + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.cad_id + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.asset_description + (!dt.tag.Equals("") ? (" (" + dt.tag + " ) ") : "") + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.budget_qty + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.lease_qty + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.dnp_qty + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + (dt.resp.Equals("EXOI") || dt.resp.Equals("EXCI") || dt.resp.Equals("EXVI") || dt.resp.Equals("EXEX") ? 0 : (dt.budget_qty - dt.dnp_qty)) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.po_qty + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.total_budget_amt.ToString("C", CultureInfo.GetCultureInfo("en")) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.total_po_amt.ToString("C", CultureInfo.GetCultureInfo("en")) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"center\">" + dt.buyout_delta.ToString("C", CultureInfo.GetCultureInfo("en")) + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.current_location + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.po_status + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.po_comment + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.po_num + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.cost_center + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.vendor + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.manufacturer + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.model_no + "</td>");
                    stringWriter.Write("<td class=\"datatab\" align=\"left\">" + dt.model_name + "</td>");
                    stringWriter.Write("</tr>");

                    IncrementReportStatus(item, percentageByItem);
                }
            } else
            {
                IncrementReportStatus(item, (totalPercentage - 5));
            }

            stringWriter.Write("</table>");
            stringWriter.Write("</td></tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td></tr>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td></tr>");

            stringWriter.Write("</table>");
            stringWriter.Write("</body></html>");
            stringWriter.Close();

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), htmlPath);
                WkhtmltopdfRepository.ConvertToPDF(htmlPath, pdfPath);
            }
            IncrementReportStatus(item, 5);

            return pdfPath;
        }

        public string BuildExcel(project_report item, string filename, List<AssetStatusItem> data, Decimal totalPercentage)
        {
            string reportsDirectory = Path.Combine(Domain.GetRoot(), "reports");
            string excelPath = Path.Combine(reportsDirectory, filename + ".xlsx");
            FileInfo report = new FileInfo(excelPath);
            FileInfo template = new FileInfo(Path.Combine(reportsDirectory, "excel_templates", "assetStatusTemplate.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (ExcelPackage xlPackage = new ExcelPackage(report, template))
            {

                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Asset Status"];

                /* Report Header */
                worksheet.Cells[4, 2].Value = item.name;
                if (item.description != null)
                {
                    worksheet.Cells[5, 2].Value = item.description;
                    worksheet.Cells[6, 2].Value = GetCostCenterInfo(item);
                }
                else
                {
                    worksheet.Cells[5, 2].Value = GetCostCenterInfo(item); // Cost center
                }

                /* Data Header */
                worksheet.Cells[8, 1].Value = "Resp";
                worksheet.Cells[8, 2].Value = "Code";
                worksheet.Cells[8, 3].Value = "CAD ID";
                worksheet.Cells[8, 4].Value = "Description";
                worksheet.Cells[8, 5].Value = "Planned Qty";
                worksheet.Cells[8, 6].Value = "Lease Qty";
                worksheet.Cells[8, 7].Value = "DNP Qty";
                worksheet.Cells[8, 8].Value = "Net New";
                worksheet.Cells[8, 9].Value = "PO Qty";
                worksheet.Cells[8, 10].Value = "Total Budget Amt";
                worksheet.Cells[8, 11].Value = "Total PO Amt";
                worksheet.Cells[8, 12].Value = "Buyout Delta";
                worksheet.Cells[8, 13].Value = "EQ Status";
                worksheet.Cells[8, 14].Value = "PO Status";
                worksheet.Cells[8, 15].Value = "PO Comment";
                worksheet.Cells[8, 16].Value = "PO No.";
                worksheet.Cells[8, 17].Value = "Cost Center";
                worksheet.Cells[8, 18].Value = "Vendor";
                worksheet.Cells[8, 19].Value = "Manufacturer";
                worksheet.Cells[8, 20].Value = "Model No.";
                worksheet.Cells[8, 21].Value = "Model Name";

                /* Data */
                //List<AssetStatusItem> data = GetData(item);
                int row = 9;
                if (data.Count() > 0)
                {
                    Decimal percentageByItem = (totalPercentage - 2) / data.Count();
                    foreach (AssetStatusItem dt in data)
                    {
                        worksheet.Cells[row, 1].Value = dt.resp;
                        worksheet.Cells[row, 2].Value = dt.asset_code;
                        worksheet.Cells[row, 3].Value = dt.cad_id;
                        worksheet.Cells[row, 4].Value = dt.asset_description + (!dt.tag.Equals("") ? (" (" + dt.tag + " ) ") : "");
                        worksheet.Cells[row, 5].Value = dt.budget_qty;
                        worksheet.Cells[row, 6].Value = dt.lease_qty;
                        worksheet.Cells[row, 7].Value = dt.dnp_qty;
                        worksheet.Cells[row, 8].Value = dt.resp.Equals("EXOI") || dt.resp.Equals("EXCI") || dt.resp.Equals("EXVI") || dt.resp.Equals("EXEX") ? 0 : (dt.budget_qty - dt.dnp_qty);
                        worksheet.Cells[row, 9].Value = dt.po_qty;
                        worksheet.Cells[row, 10].Value = dt.total_budget_amt;
                        worksheet.Cells[row, 11].Value = dt.total_po_amt;
                        worksheet.Cells[row, 12].Value = dt.buyout_delta;
                        worksheet.Cells[row, 13].Value = dt.current_location;
                        worksheet.Cells[row, 14].Value = dt.po_status;
                        worksheet.Cells[row, 15].Value = dt.po_comment;
                        worksheet.Cells[row, 16].Value = dt.po_num;
                        worksheet.Cells[row, 17].Value = dt.cost_center;
                        worksheet.Cells[row, 18].Value = dt.vendor;
                        worksheet.Cells[row, 19].Value = dt.manufacturer;
                        worksheet.Cells[row, 20].Value = dt.model_no;
                        worksheet.Cells[row, 21].Value = dt.model_name;

                        IncrementReportStatus(item, percentageByItem);
                        row++;
                    }
                } else
                {
                    IncrementReportStatus(item, (totalPercentage - 2));
                }

                xlPackage.Save();
                IncrementReportStatus(item, 2);
            }

            return excelPath;
        }

        private List<AssetStatusItem> GetData(project_report report, decimal total_percentage)
        {

            StringBuilder poStatusSelect = new StringBuilder();
            StringBuilder curLocationSelect = new StringBuilder();

            string where;
            where = GetWhereClause(report, "a", "cc", "id");
            string wherePri = where.Replace("a.", "pri.").Replace("cc.id", "pri.cost_center_id").Replace("cc.", "pri.");

            /* Get initial data */
            StringBuilder select = new StringBuilder("SELECT a.TYPE, a.resp, a.asset_domain_id, a.asset_id, a.asset_code, a.asset_description, a.manufacturer_description AS manufacturer, a.serial_number AS model_no, a.serial_name AS model_name, SUM(a.budget_qty) AS budget_qty, SUM(a.budget_qty_sf) AS budget_qty_sf, SUM(a.dnp_qty) AS dnp_qty, SUM(a.dnp_qty_sf) AS dnp_qty_sf, SUM(a.lease_qty) AS lease_qty, SUM(a.lease_qty_sf) AS lease_qty, SUM(a.po_qty) AS po_qty, SUM(a.po_qty_sf) AS po_qty_sf, SUM(a.total_budget_amt) AS total_budget_amt, SUM(a.total_po_amt) AS total_po_amt, SUM(a.buyout_delta) AS buyout_delta, a.cad_id, COALESCE(a.tag, '') AS tag, cc.description AS cost_center ");
            select.Append("FROM inventory_w_relo_v AS a  ");
            select.Append("LEFT JOIN cost_center AS cc ON cc.id = a.cost_center_id ");
            select.Append("WHERE a.type = 'NEW' AND ");
            select.Append(where.Replace("WHERE", ""));
            select.Append(" GROUP BY a.type, a.resp, a.asset_domain_id, a.asset_id, a.asset_code, a.asset_description, a.manufacturer_description, a.serial_number, a.serial_name, a.cad_id, COALESCE(a.tag, ''), cc.description ORDER BY a.asset_code, a.type;");
            
            List<AssetStatusItem> items = this._db.Database.SqlQuery<AssetStatusItem>(select.ToString()).ToList();
            
            int qty = items.Count();

            if (qty > 0)
            {
                decimal percentageByItem = total_percentage / qty;
                for (var i = 0; i < qty; i++)
                {

                    /* Get PO Status */
                    select.Clear();
                    select.Append("SELECT CASE WHEN po.status IS NULL THEN 1 WHEN po.status = 'Open' THEN 2 WHEN po.status = 'Quote Requested' THEN 3 WHEN po.status = 'Quote Received' THEN 4 WHEN po.status = 'PO Requested' THEN 5 WHEN po.status = 'PO Issued' THEN 6 END AS sort_order, ");
                    select.Append("CASE WHEN po.status IS NULL THEN 'None' WHEN po.status = 'Open' THEN po.status WHEN po.status = 'Quote Requested' THEN 'Quote Req' WHEN po.status = 'Quote Received' THEN 'Quote Rec' WHEN po.status = 'PO Requested' THEN 'PO Req' WHEN po.status = 'PO Issued' THEN 'PO Issued' END AS status, ");
                    select.Append("SUM(CASE WHEN am.eq_unit_desc = 'per sf' THEN CASE WHEN COALESCE(ipo.po_qty, 0) = 0 THEN 0 ELSE 1 END ELSE COALESCE(ipo.po_qty, 0) END) AS equip_count ");
                    select.Append("FROM project_room_inventory AS pri ");
                    select.Append("INNER JOIN assets AS a ON pri.asset_id = a.asset_id AND pri.asset_domain_id = a.domain_id ");
                    select.Append("LEFT JOIN assets_measurement AS am ON a.eq_measurement_id = am.eq_unit_measure_id ");
                    select.Append("LEFT JOIN inventory_purchase_order AS ipo ON pri.inventory_id = ipo.inventory_id ");
                    select.Append("LEFT JOIN purchase_order AS po ON ipo.po_id = po.po_id AND ipo.po_domain_id = po.domain_id AND ipo.project_id = po.project_id ");
                    select.Append(wherePri);
                    select.Append(" AND pri.asset_id = " + items[i].asset_id + " AND pri.asset_domain_id = " + items[i].asset_domain_id + " AND COALESCE(pri.resp, a.default_resp) = '" + items[i].resp + "' ");
                    select.Append("GROUP BY CASE WHEN po.status IS NULL THEN 1 WHEN po.status = 'Open' THEN 2 WHEN po.status = 'Quote Requested' THEN 3 WHEN po.status = 'Quote Received' THEN 4 WHEN po.status = 'PO Requested' THEN 5 WHEN po.status = 'PO Issued' THEN 6 END, CASE WHEN po.status IS NULL THEN 'None' WHEN po.status = 'Open' THEN po.status WHEN po.status = 'Quote Requested' THEN 'Quote Req' WHEN po.status = 'Quote Received' THEN 'Quote Rec' WHEN po.status = 'PO Requested' THEN 'PO Req' WHEN po.status = 'PO Issued' THEN 'PO Issued' END ");
                    select.Append("ORDER BY sort_order");

                    List<AssetStatusPOStatus> poStatuses = this._db.Database.SqlQuery<AssetStatusPOStatus>(select.ToString()).ToList();

                    items[i].po_status = "";
                    AssetStatusPOStatus lastPOStatus = poStatuses.Last();
                    foreach (AssetStatusPOStatus status in poStatuses)
                    {
                        items[i].po_status += status.status + " (" + status.equip_count + ")";
                        if (status != lastPOStatus)
                        {
                            items[i].po_status += "\n";
                        }
                    }

                    /* Current location */
                    select.Clear();
                    select.Append("SELECT pri.current_location, SUM((COALESCE(pri.budget_qty, 0)*pr.room_quantity)) AS equip_count ");
                    select.Append("FROM project_room_inventory AS pri ");
                    select.Append("INNER JOIN assets AS a ON pri.asset_domain_id = a.domain_id AND pri.asset_id = a.asset_id ");
                    select.Append("INNER JOIN project_room pr ON pr.project_id = pri.project_id and pr.phase_id = pri.phase_id and pr.department_id = pri.department_id and pr.room_id = pri.room_id and pr.domain_id = pri.domain_id ");
                    select.Append(wherePri);
                    select.Append(" AND pri.asset_domain_id = " + items[i].asset_domain_id + " AND pri.asset_id = " + items[i].asset_id);
                    select.Append(" GROUP BY pri.current_location ORDER BY pri.current_location;");

                    List<AssetStatusCurLocation> curLocations = this._db.Database.SqlQuery<AssetStatusCurLocation>(select.ToString()).ToList();

                    items[i].current_location = "";
                    AssetStatusCurLocation lastCurLocation = curLocations.Last();
                    foreach (AssetStatusCurLocation curLocation in curLocations)
                    {
                        items[i].current_location += curLocation.current_location + " (" + curLocation.equip_count + ")";
                        if (curLocation != lastCurLocation)
                        {
                            items[i].current_location += "\n";
                        }
                    }

                    /* PO NUM, PO comment, vendor */
                    select.Clear();
                    select.Append("SELECT COALESCE(po.po_number, '') AS po_number, v.name AS vendor_name, CASE WHEN RTRIM(po.comment) IS NULL THEN '' ELSE RTRIM(po.comment) END AS po_comment ");
                    select.Append("FROM purchase_order AS po ");
                    select.Append("INNER JOIN inventory_purchase_order AS ipo ON po.po_id = ipo.po_id AND po.domain_id = ipo.po_domain_id AND po.project_id = ipo.project_id ");
                    select.Append("INNER JOIN project_room_inventory AS pri ON ipo.inventory_id = pri.inventory_id ");
                    select.Append("INNER JOIN assets AS a ON pri.asset_domain_id = a.domain_id AND pri.asset_id = a.asset_id ");
                    select.Append("INNER JOIN vendor AS v ON po.vendor_domain_id = v.domain_id AND po.vendor_id = v.vendor_id ");
                    select.Append(wherePri);
                    select.Append(" AND pri.asset_domain_id = " + items[i].asset_domain_id + " AND pri.asset_id = " + items[i].asset_id + " AND COALESCE(pri.resp, a.default_resp) = '" + items[i].resp + "' ");
                    select.Append("GROUP BY po.po_number, v.name, CASE WHEN RTRIM(po.comment) IS NULL THEN '' ELSE RTRIM(po.comment) END ORDER BY po.po_number;");

                    List<AssetStatusPOVendor> pos_vendors = this._db.Database.SqlQuery<AssetStatusPOVendor>(select.ToString()).ToList();

                    items[i].po_num = "";
                    items[i].po_comment = "";
                    items[i].vendor = "";
                    foreach (AssetStatusPOVendor po_vendor in pos_vendors)
                    {
                        items[i].po_num += po_vendor.po_number + System.Environment.NewLine;
                        items[i].po_comment += po_vendor.po_comment + System.Environment.NewLine;
                        items[i].vendor += po_vendor.vendor_name + System.Environment.NewLine;
                    }
                    IncrementReportStatus(report, percentageByItem);
                }
            }

            return items;
        }
    }
}
