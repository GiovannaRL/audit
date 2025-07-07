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
using Newtonsoft.Json;
using xPlannerCommon.Models.Enums;

namespace reportsWebJob.Services
{
    class IllustrationSheetRepository : GenericReportRepository
    {
        public void Build(project_report item)
        {
            InitiateReport(item);

            string filename = GetFilename(item);

            List<IllustrationSheetItem> data = GetData(item, 20);
            UpdateReportStatus(item, 20);

            string pdfFile = BuildPDF(item, filename, data, 20);
            UpdateReportStatus(item, 96);

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {

                    if (UploadToCloud(item, pdfFile, "pdf"))
                    {
                        item.file_name = filename;
                        CompleteReport(item);
                    }
                    else
                    {
                        CompleteReportError(item);
                    }
                fileRepository.DeleteFile(pdfFile);
            }
        }

        public string BuildPDF(project_report item, string filename, List<IllustrationSheetItem> data, Decimal totalPercentage)
        {
            FileStreamRepository _fileRepository = new FileStreamRepository();
            string reportsDirectory = Path.Combine(Domain.GetRoot(), "reports");
            string pdfPath = Path.Combine(reportsDirectory, filename + ".pdf");
            string htmlPath = pdfPath.Replace(".pdf", ".html");

            StringWriter stringWriter = new StringWriter();
            string rootPath = Domain.GetRoot();
            string photo_dir = Path.Combine(rootPath, "photos");
            var guid = Guid.NewGuid().ToString();
            var n = 0;
            var incrementStatus = 70 / (data.Count() == 0 ? 1 : data.Count());

            stringWriter.Write("<html><head><meta charset='UTF-8'>");
            stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "css", "IllustrationSheet.css") + "' rel='stylesheet' />");
            stringWriter.Write("</head><body style=font-family:Arial; font-size:8pt;>");

            if (data.Count() == 0)
            {
                stringWriter.Write("<p>There is no JSN available with finish data type related.</p>");
            }

            foreach (var sheet in data)
            {
                n++;
                var photo = "<img src='' BorderStyle='None' AlternateText='No Picture' Height='250' Width='250' />";

                //check first if there is an asset photo
                if (!string.IsNullOrEmpty(sheet.blob_file_name))
                {
                    //DOWNLOAD PHOTO
                    var blob_photo = _fileRepository.GetBlob(BlobContainersName.Photo(sheet.domain_id), sheet.blob_file_name);

                    var photo_path = _fileRepository.GetBlobSasUri(blob_photo);

                    if (!Directory.Exists(photo_dir))
                        Directory.CreateDirectory(photo_dir);

                    var full_photo_dir = Path.Combine(photo_dir, guid + sheet.blob_file_name);

                    _fileRepository.DeleteFile(full_photo_dir);

                    if (blob_photo.Exists())
                    {
                        blob_photo.DownloadTo(full_photo_dir);
                        photo = "<img src='" + full_photo_dir + "' BorderStyle='None' AlternateText='No Picture' Height='250' Width='250' />";

                    }
                }
                else if (sheet.photo != null && sheet.photo != "")
                {
                    //DOWNLOAD PHOTO
                    var blob_photo = _fileRepository.GetBlob(BlobContainersName.Photo(sheet.asset_domain_id), sheet.photo);

                    var photo_path = _fileRepository.GetBlobSasUri(blob_photo);

                    if (!Directory.Exists(photo_dir))
                        Directory.CreateDirectory(photo_dir);

                    var full_photo_dir = Path.Combine(photo_dir, guid + sheet.photo);

                    _fileRepository.DeleteFile(full_photo_dir);

                    if (blob_photo.Exists())
                    {
                        blob_photo.DownloadTo(full_photo_dir);
                        photo = "<img src='" + full_photo_dir + "' BorderStyle='None' AlternateText='No Picture' Height='250' Width='250' />";

                    }
                }
            
                stringWriter.Write("<table width=\"100%\" border=\"0px\">");

                /* Report Header */
                stringWriter.Write("<tr><td><table width=\"100%\">");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td>");
                stringWriter.Write("<table width=\"100%\" align=\"center\" class=\"bordertab\" cellpadding=\"5\">");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td rowspan=\"8\">" + photo + "</td>");
                stringWriter.Write("<td class=\"datatab\"><strong>SPECIFICATION</strong></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>Manufacturer:</strong> " + sheet.manufacturer_description + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>Description:</strong> " + sheet.asset_description + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>Name/Model/#:</strong> " + sheet.model_name + " / " + sheet.model_number + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>Size:</strong> " + sheet.weight + " W x " + sheet.depth + " D x " + sheet.height + " H" + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>Weight:</strong> " + sheet.weight + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>Weight Limit:</strong> " + sheet.weight_limit + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>Notes:</strong> " + sheet.asset_comment + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("</table></td></tr>");

                /* OPTIONS? */
                if (sheet.options.Count() > 0)
                {
                    var totalPerLine = 4;
                    var perLine = 0;
                    stringWriter.Write("<tr><td style=\"align:center;width:100%\"><table align=\"center\" cellpadding=\"5\" class=\"noborder\"><tr>");
                    foreach (var option in sheet.options)
                    {
                        perLine++;
                        var option_img = "<img src='' BorderStyle='None' AlternateText='No Picture' Height='200' Width='200' />";
                        if (!string.IsNullOrEmpty(option.blob_file_name))
                        {
                            //DOWNLOAD PHOTO
                            var blob_photo = _fileRepository.GetBlob(BlobContainersName.Photo(option.domain_id??sheet.domain_id), option.blob_file_name);

                            var photo_path = _fileRepository.GetBlobSasUri(blob_photo);

                            if (!Directory.Exists(photo_dir))
                                Directory.CreateDirectory(photo_dir);

                            var full_photo_dir = Path.Combine(photo_dir, guid + option.blob_file_name);

                            _fileRepository.DeleteFile(full_photo_dir);

                            if (blob_photo.Exists())
                            {
                                blob_photo.DownloadTo(full_photo_dir);
                                option_img = "<img src='" + full_photo_dir + "' BorderStyle='None' AlternateText='No Picture' Height='200' Width='200' />";

                            }
                        }

                        var settings = new OptionSettings();
                        if (!string.IsNullOrEmpty(option.settings))
                        {
                            settings = JsonConvert.DeserializeObject<OptionSettings>(option.settings.Replace("\"Finish Type\":", "\"finish_type\":").Replace("\"Content\":", "\"content\":"));
                        }


                        if (perLine > totalPerLine)
                        {
                            stringWriter.Write("</tr></table></td></tr>");
                            stringWriter.Write("<tr><td style=\"align:center;width:100%\"><table align=\"center\" cellpadding=\"5\" class=\"noborder\"><tr>");
                            perLine = 1;
                        }
                        stringWriter.Write("<td style=\"align:center;width:25%;vertical-align:top\">");
                        stringWriter.Write("<table width=\"100%\" align=\"center\" class=\"noborder\">");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td>" + option_img + "</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td><strong>Frame Type:</strong> Finish</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td><strong>Finish Type:</strong> " + settings.finish_type + "</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td><strong>Manufacturer:</strong> " + sheet.manufacturer_description + "</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td><strong>Name/Code/Color:</strong> " + option.display_code + "</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td><strong>Content:</strong> " + settings.content + "</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td><strong>Notes:</strong> " + option.description + "</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("</table>");
                        stringWriter.Write("</td>");
                    
                    }
                    stringWriter.Write("</tr></table></td></tr>");
                }

                /* PLACEMENT? */
                if (sheet.rooms.Count() > 0)
                {
                    stringWriter.Write("<tr><td style=\"padding-top:20px\">");
                    stringWriter.Write("<table width=\"100%\" align=\"center\" class=\"bordertab\" cellpadding=\"5\">");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td class=\"datatab\"><strong>PLACEMENT:</strong> " + sheet.placement + "</td>");
                    stringWriter.Write("<td class=\"datatab\" colspan=\"2\"><strong>DEPARTMENT:</strong> " + sheet.department_description + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td class=\"datatab\">LOCATION NAME</td>");
                    stringWriter.Write("<td class=\"datatab\">Rm#</td>");
                    stringWriter.Write("<td class=\"datatab\">Qty</td>");
                    stringWriter.Write("</tr>");

                    foreach (var room in sheet.rooms)
                    {
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td>" + room.drawing_room_number + " - " + room.drawing_room_name + "</td>");
                        stringWriter.Write("<td>" + room.drawing_room_number + "</td>");
                        stringWriter.Write("<td>" + room.room_quantity + "</td>");
                        stringWriter.Write("</tr>");
                    }
                    stringWriter.Write("</table></td></tr>");
                    stringWriter.Write("<tr><td>");
                    stringWriter.Write("<table width=\"100%\" align=\"center\" class=\"bordertab\" cellpadding=\"5\">");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td><strong>UNIT PRICE</strong></td>");
                    stringWriter.Write("<td align=\"right\">$" + sheet.unit_budget_total + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("</table></td></tr>");
                }

                /* FOOTER? */
                stringWriter.Write("<tr><td style=\"padding-top:20px\">");
                stringWriter.Write("<table width=\"100%\" align=\"center\" class=\"bordertab\" cellpadding=\"5\">");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td class=\"datatab\" width=\"50%\"><strong>JSN#:</strong> " + sheet.jsn_code + "</td>");
                stringWriter.Write("<td class=\"datatab\"><strong>JSN Description:</strong> " + sheet.asset_description + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td style=\"border-right:1pt solid #E5E5E5\">");
                stringWriter.Write("<table width=\"100%\" align=\"center\" class=\"noborder\" cellpadding=\"5\">");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td colspan=\"2\"><strong>SPECIFICATION - FURNITURE</strong></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td><strong>DATE:</strong> " + sheet.locked_date + "</td>");
                stringWriter.Write("<td><strong>IO SUBMITTAL:</strong> " + sheet.locked_comment + "</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("</table>");
                stringWriter.Write("</td>");
                stringWriter.Write("<td style=\"border-right:1pt solid #E5E5E5\">");
                stringWriter.Write("<table width=\"100%\" align=\"center\" class=\"noborder\" cellpadding=\"5\">");
                stringWriter.Write("<tr>");
                stringWriter.Write("<td>" + sheet.facility + " - " + sheet.client + "/<br>" + sheet.project_description + "</td>");
                stringWriter.Write("<td width=\"110px\"><img border='0' height=\"40\" src='" + Path.Combine(Domain.GetRoot(), "images/logo_aw.png") + "'></td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("</table>");
                stringWriter.Write("</td>");
                stringWriter.Write("</tr>");
                stringWriter.Write("</table></td></tr>");

                stringWriter.Write("</table>");
                stringWriter.Write("</td></tr>");
                stringWriter.Write("</table>");
                if (n < data.Count())
                {
                    stringWriter.Write("<div style=\"page-break-before:always\">&nbsp;</div>");
                }

                IncrementReportStatus(item, incrementStatus);
            }

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

        

        private List<IllustrationSheetItem> GetData(project_report report, decimal total_percentage)
        {

            StringBuilder poStatusSelect = new StringBuilder();
            StringBuilder curLocationSelect = new StringBuilder();

            string where;
            where = GetWhereClause(report, "pri", "cc", "id");

            /* Get initial data */
            StringBuilder select = new StringBuilder("SELECT distinct pri.project_id, pri.phase_id, pri.department_id, pri.room_id, pri.domain_id, pri.asset_id, pri.asset_domain_id, pri.inventory_id, pri.asset_description, pri.placement, pri.height, pri.width, pri.depth, pri.jsn_code, pri.unit_budget_total, ");
            select.Append("a.photo, pri.model_number, pri.model_name, a.weight, a.weight_limit, pri.comment as asset_comment, pri.manufacturer_description, d.description as department_description, p.project_description, p.locked_comment, p.locked_date, f.name as facility, c.name as client, ac.description as prefix_description, ");
            select.Append("( ");
            select.Append("select pdoc.blob_file_name from documents_associations da ");
            select.Append("inner join project_documents pdoc on pdoc.id = da.document_id and pdoc.project_domain_id = da.project_domain_id and pdoc.type_id = " + DocumentTypeIdEnum.AssetPhoto);
            select.Append(" WHERE da.inventory_id = pri.inventory_id ");
            select.Append(") as blob_file_name ");
            select.Append("FROM project_room_inventory pri inner join assets a on a.asset_id = pri.asset_id and a.domain_id = pri.asset_domain_id ");
            select.Append("inner join project_department d on d.project_id = pri.project_id and d.phase_id = pri.phase_id and d.department_id = pri.department_id and d.domain_id = pri.domain_id ");
            select.Append("inner join project p on p.project_id = pri.project_id and p.domain_id = pri.domain_id ");
            select.Append("inner join facility f on f.id = p.facility_id and f.domain_id = p.facility_domain_id ");
            select.Append("inner join client c on c.id = p.client_id and c.domain_id = p.client_domain_id ");
            select.Append("inner join inventory_options io on io.inventory_id = pri.inventory_id and io.inventory_domain_id = pri.domain_id ");
            select.Append("inner join assets_options ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id and ao.data_type = 'FI' ");
            select.Append("left join assets_codes ac on ac.prefix = left(a.asset_code,3) and ac.domain_id = a.domain_id ");
            select.Append("LEFT JOIN cost_center cc ON pri.cost_center_id = cc.id ");
            select.Append(" WHERE pri.jsn_code is not null ");
            select.Append(where.Replace("WHERE", "AND"));

            List<IllustrationSheetItem> items = this._db.Database.SqlQuery<IllustrationSheetItem>(select.ToString()).ToList();

            int qty = items.Count();

            if (qty > 0)
            {
                decimal percentageByItem = total_percentage / qty;
                for (var i = 0; i < qty; i++)
                {

                    /* GET ROOMS FOR THIS JSN */
                    select.Clear();
                    select.Append("SELECT * ");
                    select.Append("FROM project_room ");
                    select.Append("WHERE project_id = " + items[i].project_id);
                    select.Append("AND phase_id = " + items[i].phase_id);
                    select.Append("AND department_id = " + items[i].department_id);
                    select.Append("AND room_id = " + items[i].room_id);
                    select.Append("AND domain_id = " + items[i].domain_id);
                    select.Append("AND room_id in(select room_id from project_room_inventory where jsn_code = '" + items[i].jsn_code + "' and project_id = " + items[i].project_id + ")");

                    List<project_room> rooms = this._db.Database.SqlQuery<project_room>(select.ToString()).ToList();
                    items[i].rooms = rooms;

                    /* GET ASSETS OPTIONS FOR THIS INVENTORY */
                    select.Clear();
                    select.Append("SELECT ao.data_type, ao.display_code, ao.description, ao.code, ao.settings, dd.domain_id, dd.blob_file_name ");
                    select.Append("FROM inventory_options io ");
                    select.Append("INNER JOIN assets_options ao ON ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id ");
                    select.Append("LEFT JOIN domain_document dd ON dd.id = io.document_id and dd.domain_id = io.document_domain_id ");
                    select.Append("WHERE ao.data_type = 'FI' AND io.inventory_id = " + items[i].inventory_id);
                    select.Append(" AND io.inventory_domain_id = " + items[i].domain_id);

                    List<InventoryOption> options = this._db.Database.SqlQuery<InventoryOption>(select.ToString()).ToList();
                    items[i].options = options;

                    IncrementReportStatus(report, percentageByItem);

                }
            }

            return items;
        }
    }
}
