using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using reportsWebJob.Models;
using System.Globalization;

namespace reportsWebJob.Services
{
    class ProcurementRepository : GenericReportRepository
    {
        private CultureInfo culture = CultureInfo.GetCultureInfo("en");
        private List<ProcurementTOCItem> pos;
        private string filePathNoExtension;

        public void Build(project_report item)
        {
            InitiateReport(item);

            this.filePathNoExtension = Path.Combine(GetReportsDirectory(), GetFilename(item));

            List<string> titles = new List<string>();

            /* Create table of content page */
            string tocPath = CreateTOC(item);
            titles.Add("Table of contents");

            /* After TOC is create update report status to 10% */
            UpdateReportStatus(item, 10);

            /* Create PO details page and get uploaded quote and po */
            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                using (PORepository poRep = new PORepository(1))
                {
                    List<string> paths = new List<string>();

                    string uploadedFilePath;

                    if (pos.Count() > 0)
                    {
                        /* Calculate the percentagem value of the each po, considering that process of create the
                            pos page equals 80% of the project */
                        Decimal percentageByItem = ((decimal)80) / pos.Count();

                        foreach (ProcurementTOCItem po in pos)
                        {
                            if (item.include_po_cover)
                            {
                                paths.Add(poRep.CreatePODetailsPage(item.project_domain_id, item.project_id, po.po_id));
                                titles.Add((po.po_number ?? po.po_description) + " - PO Cover");
                            }

                            if (item.include_po_uploaded)
                            {
                                uploadedFilePath = GetUploadedFile(item, po.po_id, "po");
                                if (uploadedFilePath != null)
                                {
                                    paths.Add(uploadedFilePath);
                                    titles.Add((po.po_number ?? po.po_description) + " - PO Details");
                                }
                            }

                            if (item.include_quote_uploaded)
                            {
                                uploadedFilePath = GetUploadedFile(item, po.po_id, "quote");
                                if (uploadedFilePath != null)
                                {
                                    paths.Add(uploadedFilePath);
                                    titles.Add((po.po_number ?? po.po_description) + " - Quote");
                                }
                            }

                            /* After add the pages of each po increment the report status value */
                            IncrementReportStatus(item, percentageByItem);
                        }
                    } else
                    {
                        IncrementReportStatus(item, 80);
                    }

                    if (paths.Count() > 0)
                    {
                        paths.Insert(0, tocPath);
                        fileRepository.MergeFiles(paths.ToArray(), this.filePathNoExtension + ".pdf", titles.ToArray());
                        fileRepository.DeleteFiles(paths);
                    }
                    else
                    {
                        System.IO.File.Move(tocPath, this.filePathNoExtension + ".pdf");
                    }
                }

                item.file_name = GetFilename(item);
                if (UploadToCloud(item, this.filePathNoExtension + ".pdf", "pdf"))
                    CompleteReport(item);
            }
        }

        private string GetUploadedFile(project_report item, int po_id, string type)
        {
            using (PORepository poRep = new PORepository(item.project_domain_id))
            {
                switch (type.ToLower())
                {
                    case "po":
                        return poRep.GetUploadedPO(item.project_domain_id, item.project_id, po_id);
                    case "quote":
                        return poRep.GetUploadedQuote(item.project_domain_id, item.project_id, po_id);
                    default:
                        return null;
                }
            }
        }

        public string CreateTOC(project_report item)
        {
            string reportWhere = GetWhereClause(item, "pri", null);
            string htmlPath = this.filePathNoExtension + "_TOC.html";
            string pdfPath = this.filePathNoExtension + "_TOC.pdf";

            StringWriter stringWriter = new StringWriter();

            stringWriter.Write("<html><head><meta charset='UTF-8'>");
            stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "css", "Procurement.css") + "' rel='stylesheet' />");
            stringWriter.Write("</head><body style=font-family:Arial; font-size:8pt;>");
            stringWriter.Write("<table width=\"100%\" border=\"0px\">");

            /* Report Header */
            stringWriter.Write("<tr><td><table width=\"1000px\">");
            stringWriter.Write("<tr><td>");
            stringWriter.Write("<table width=\"500px\">");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td colspan=\"2\"><h2>" + item.project.project_description + "</h2></td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td colspan=\"2\"><h2>" + item.name + "</h2></td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr><td colspan=\"2\">&nbsp;</td></tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td>" + item.description + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<td colspan=\"2\">Purchase Status:&nbsp;&nbsp;" + item.po_status + "</td>");
            //stringWriter.Write("<td>" + item.po_status + "</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</td>");
            stringWriter.Write("<td>");
            stringWriter.Write("<img height=\"40\" src='" + Path.Combine(Domain.GetRoot(), "images", "logo_aw.png") + "'/>");
            stringWriter.Write("</td>");
            stringWriter.Write("</tr>");
            stringWriter.Write("</table></td></tr>");

            // table of contents

            stringWriter.Write("<tr><td><table class=\"toc\" width=\"100%\">");
            /* Table header */
            stringWriter.Write("<tr class=\"header\">");
            stringWriter.Write("<th class=\"datatab\" style=\"width:20%\">PO #</th>");
            stringWriter.Write("<th class=\"datatab\" style=\"width:10%\">Vendor</th>");
            stringWriter.Write("<th class=\"datatab\" style=\"width:10%\">Description</th>");
            stringWriter.Write("<th class=\"datatab\" style=\"width:15%\">Quote #</th>");
            stringWriter.Write("<th class=\"datatab\" style=\"width:15%\">Requisition #</th>");
            stringWriter.Write("<th class=\"datatab\" style=\"width:10%\">Status</th>");
            stringWriter.Write("<th class=\"datatab\" style=\"width:10%\">Status Date</th>");
            //stringWriter.Write("<th class=\"datatab\">Aging (Days)</th>");
            stringWriter.Write("<th class=\"datatab\" colspan=\"2\" style=\"width:10%\">Amount</th>");
            //stringWriter.Write("<th class=\"datatab\">Comment</th>");
            //stringWriter.Write("<th class=\"datatab\">Ship To</th>");
            stringWriter.Write("</tr>");

            /* Table Content */
            StringBuilder select = new StringBuilder();
            select.Append("select po.po_id, po.po_number, po.description AS po_description, CASE po.status  WHEN 'PO Requested' THEN po.po_requested_date ");
            select.Append("WHEN 'PO Issued' THEN po.po_received_date WHEN 'Quote Requested' THEN po.quote_requested_date WHEN 'Quote Received' THEN po.quote_received_date ELSE null END AS status_date, ");
            select.Append("CASE po.status  WHEN 'PO Issued' THEN DATEDIFF(DAY, po.po_received_date, po.po_requested_date) WHEN 'PO Requested' THEN DATEDIFF(DAY, GETDATE(), po.po_requested_date) WHEN 'Quote Requested' THEN DATEDIFF(DAY, GETDATE(), po.quote_requested_date) WHEN 'Quote Received' THEN DATEDIFF(DAY, GETDATE(), po.quote_received_date) END AS aging_days, ");
            select.Append("COALESCE(SUM(COALESCE(ipo.po_qty, 0) * COALESCE(ipo.po_unit_amt, 0)) + MIN(COALESCE(po.freight,0)) + MIN(COALESCE(po.warehouse,0)) + MIN(COALESCE(po.tax,0)) + MIN(COALESCE(po.misc,0)) + MIN(COALESCE(po.warranty,0)),0) AS amount, po.status, po.date_added, po.added_by, po.comment, po.quote_number, po.po_requested_number,v.name AS vendor, pa.nickname AS ship_to ");
            select.Append("FROM project_room_inventory AS pri INNER JOIN inventory_purchase_order AS ipo ON ipo.inventory_id = pri.inventory_id ");
            select.Append("INNER JOIN purchase_order AS po ON po.po_id = ipo.po_id AND po.domain_id = ipo.po_domain_id AND po.project_id = ipo.project_id ");
            select.Append("INNER JOIN vendor AS v ON po.vendor_domain_id = v.domain_id AND po.vendor_id = v.vendor_id ");
            select.Append("LEFT JOIN project_addresses AS pa ON po.ship_to = pa.id ");
            select.Append(reportWhere);
            if (!item.po_status.ToLower().Equals("all"))
            {
                select.Append(" AND lower(po.status) = lower('" + item.po_status + "') ");
            }
            select.Append(" GROUP BY po.po_id, po.po_number, po.description, CASE po.status  WHEN 'PO Requested' THEN po.po_requested_date ");
            select.Append("WHEN 'PO Issued' THEN po.po_received_date WHEN 'Quote Requested' THEN po.quote_requested_date WHEN 'Quote Received' THEN po.quote_received_date ELSE null END, ");
            select.Append("CASE po.status  WHEN 'PO Issued' THEN DATEDIFF(DAY, po.po_received_date, po.po_requested_date) WHEN 'PO Requested' THEN DATEDIFF(DAY, GETDATE(), po.po_requested_date) WHEN 'Quote Requested' THEN DATEDIFF(DAY, GETDATE(), po.quote_requested_date) WHEN 'Quote Received' THEN DATEDIFF(DAY, GETDATE(), po.quote_received_date) END, ");
            select.Append("po.status, po.date_added, po.added_by, po.comment, po.quote_number, po.po_requested_number, v.name, pa.nickname ");
            select.Append("ORDER BY status_date desc, po.description;");

            // TODO (unasigned select)
            
            List<ProcurementTOCItem> data = this._db.Database.SqlQuery<ProcurementTOCItem>(select.ToString()).ToList();

            pos = data;

            foreach (ProcurementTOCItem dt in data)
            {
                stringWriter.Write("<tr class=\"content\">");
                stringWriter.Write("<td class=\"datatab\" style=\"width:20%\" nowrap>" + (dt.po_number != null ? dt.po_number.Replace("&", "&<br/>") : "") + " </td>");
                stringWriter.Write("<td class=\"datatab\" align=center style=\"width:10%\">" + dt.vendor + "</td>");
                stringWriter.Write("<td class=\"datatab\" style=\"width:10%\">" + dt.po_description + "</td>");
                stringWriter.Write("<td class=\"datatab\" style=\"width:15%\">" + dt.quote_number + "</td>");
                stringWriter.Write("<td class=\"datatab\" style=\"width:15%\">" + dt.po_requested_number + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=center style=\"width:10%\">" + dt.status + "</td>");
                stringWriter.Write("<td class=\"datatab\" align=center style=\"width:10%\">" + (dt.status_date != null ? ((DateTime)dt.status_date).ToShortDateString() : "") + "</td>");
                //stringWriter.Write("<td class=\"datatab integer\" align=center>" + dt.aging_days + "</td>");
                stringWriter.Write("<td class=\"datatab money left\" style=\"width:3%\">$</td>");
                stringWriter.Write("<td class=\"datatab money right\" style=\"width:7%\">" + dt.amount.ToString("C", this.culture).Substring(1) + "</td>");
                //stringWriter.Write("<td class=\"datatab text\" align=center>" + dt.comment + "</td>");
                //stringWriter.Write("<td class=\"datatab text\" align=center>" + dt.ship_to + "</td>");
                stringWriter.Write("</tr>");
            }

            stringWriter.Write("</table></td></tr>");
            stringWriter.Write("</table>");
            stringWriter.Write("</body></html>");

            using (FileStreamRepository fileRep = new FileStreamRepository())
            {
                fileRep.SaveDocumentTemporarily(stringWriter.ToString(), htmlPath);
                WkhtmltopdfRepository.ConvertToPDF(htmlPath, pdfPath);
            }

            return pdfPath;
        }
    }
}
