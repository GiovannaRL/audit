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
    class ShopDrawingsRepository : GenericReportRepository
    {
        private FileStreamRepository fileRepository;

        public ShopDrawingsRepository()
        {
            fileRepository = new FileStreamRepository();
        }

        public void Build(project_report item)
        {
            InitiateReport(item);

            string reports_path = Path.Combine(Domain.GetRoot(), "reports");

            string coverPath = CreateCover(item);

            //List<get_shop_drawing_report_info_Result> data = this._db.get_shop_drawing_report_info(item.project_domain_id,
            //    item.project_id, "shop drawing").ToList();

            StringBuilder select = new StringBuilder("SELECT pdoc.*, ai.asset_code, ai.asset_description, ai.department_description, ");
            select.Append("ai.phase_description, ai.room_number, ai.room_name, p.project_description ");
            select.Append("FROM project_documents pdoc ");
            select.Append("LEFT JOIN asset_inventory ai ON ai.linked_document = pdoc.id ");
            select.Append("LEFT JOIN project p ON pdoc.project_domain_id = p.domain_id AND pdoc.project_id = p.project_id ");
            //select.Append("LEFT JOIN documents_associations da ON pdoc.id = da.document_id ");
            //select.Append("LEFT JOIN document_types dt ON pdoc.type_id = dt.id ");
            //select.Append("LEFT JOIN assets a ON a.domain_id = pri.asset_domain_id AND a.asset_id = pri.asset_id ");
            //select.Append("LEFT JOIN project p ON pdoc.project_domain_id = p.domain_id AND pdoc.project_id = p.project_id ");
            //select.Append("LEFT JOIN project_phase pp ON pdoc.project_domain_id = pp.domain_id AND pdoc.project_id = pp.project_id AND pri.phase_id = pp.phase_id ");
            //select.Append("LEFT JOIN project_department pd ON pdoc.project_domain_id = pd.domain_id AND pdoc.project_id = pd.project_id AND pri.phase_id = pd.phase_id AND pri.department_id = pd.department_id ");
            //select.Append("LEFT JOIN project_room pr ON pdoc.project_domain_id = pr.domain_id AND pdoc.project_id = pr.project_id AND pri.phase_id = pr.phase_id AND pri.department_id = pr.department_id AND pri.room_id = pr.room_id ");
            //select.Append("LEFT JOIN project_room_inventory pri ON pri.inventory_id = da.inventory_id ");
            select.Append(String.Format("WHERE pdoc.project_domain_id = {0} AND pdoc.project_id = {1} AND ai.has_shop_drawing = 1 ", item.project_domain_id, item.project_id));
            if (item.report_location != null && item.report_location.Count() > 0)
            {
                select.Append(GetWhereClause(item, "ai", "ai").Replace("WHERE", " AND "));
            }
            select.Append("ORDER BY pdoc.filename, asset_code, asset_description, phase_description, department_description, room_number, room_name;");
            
            List<ShopDrawing> data = _db.Database.SqlQuery<ShopDrawing>(select.ToString()).ToList();
            UpdateReportStatus(item, 10);

            string docsTocPath = CreateDocsToc(item, data);

            List<string> files_path = new List<string>() { coverPath.Replace("html", "pdf"), docsTocPath.Replace("html", "pdf") };

            WkhtmltopdfRepository.ConvertToPDF(coverPath, files_path[0]);
            WkhtmltopdfRepository.ConvertToPDF(docsTocPath, files_path[1]);
            UpdateReportStatus(item, 30);

            files_path = files_path.Concat(GetDocumentsInfo(item, data)).ToList();

            string temporaryFilePath = Path.Combine(reports_path, item.id + ".pdf");
            UpdateReportStatus(item, 90);
            fileRepository.MergeFiles(files_path.ToArray(), temporaryFilePath);
            string filename = (item.name + "_" + item.report_type.name + "_" + item.id).Replace(" ", "_");

            foreach (string file in files_path)
            {
                fileRepository.DeleteFile(file);
            }

            if (UploadToCloud(item, temporaryFilePath, "pdf"))
            {
                item.file_name = filename;
                CompleteReport(item);
            }
            else
            {
                CompleteReportError(item);
            }
        }

        private string CreateCover(project_report item)
        {
            /* Mount data report */
            StringWriter stringWriter = new StringWriter();
            stringWriter.Write("<html><head><meta charset='UTF-8'></head><body style='font-family:Arial;'>");
            stringWriter.Write("<title>Cover</title><table border='0' 'border-collapse: collapse' width='100%' cellpadding='0' height='300'>");
            stringWriter.Write("<tr><td><p align='right' style='font-size:24'><img border='0' height=\"40\" src='" + Path.Combine(Domain.GetRoot(), "images", "logo_aw.png") + "'><br/><br/><strong>" + item.project.project_description + "</strong><br/>" + item.name + "<br/></td></tr>");
            stringWriter.Write("<tr><td><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><p align='right' style='font-size:24'>" + item.description + "</p></td></tr>");
            stringWriter.Write("<tr><td><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><p align='right' style='font-size:24'>Shop Drawing Binder<br>" + item.last_run.ToString("MM/dd/yyyy") + "</p></td></tr>");
            stringWriter.Write("</body></html>");
            stringWriter.Close();

            return this.fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), Path.Combine(Domain.GetRoot(), "reports"), item.id.ToString() + item.project_domain_id + "Cover.html");
        }

        private string CreateDocsToc(project_report item, IEnumerable<ShopDrawing> docs)
        {
            var distinctDocs = docs.Select(d => new
            {
                d.id,
                d.project_domain_id,
                d.project_id,
                d.project_description,
                d.filename,
                d.type_id,
                d.date_added,
                d.blob_file_name,
                d.version,
                d.status,
                d.file_extension
            }).Distinct();

            StringWriter stringWriter = new StringWriter();
            stringWriter.Write("<html><head><meta charset='UTF-8'></head><body style=font-family:Arial; font-size:8pt;>");
            stringWriter.Write("<div class=bookmark style=visibility:hidden>Table of Documents</div>");
            stringWriter.Write("<h3 style='margin: 5px 0px;'>" + item.project.project_description + "</h3>");
            stringWriter.Write("<p align='left' style='font-size:16;margin: 5px 0px;'>Shop Drawings</p>");
            stringWriter.Write("<table width=100% style='border: 1px solid black;border-collapse:collapse;'>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Name</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Date</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Version</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Status</th>");
            stringWriter.Write("</tr>");

            foreach (var doc in distinctDocs)
            {
                stringWriter.Write("<tr>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=40% class=text>" + doc.filename + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=10% class=text align=center>" + doc.date_added.ToShortDateString() + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=20% class=text>" + doc.version + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=20% class=integer>" + doc.status + "</td>");
                stringWriter.Write("</tr>");
            }

            stringWriter.Write("</table><br></body></html>");
            stringWriter.Close();

            return this.fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), Path.Combine(Domain.GetRoot(), "reports"), item.id.ToString() + item.project_domain_id + "ShopToc.html");
        }

        public List<string> GetDocumentsInfo(project_report item, IEnumerable<ShopDrawing> docs)
        {

            List<string> paths = new List<string>();
            string htmlPath, pdfPath, downloadedPath;

            var distinctDocs = docs.Select(d => new
            {
                d.id,
                d.project_domain_id,
                d.project_id,
                d.project_description,
                d.filename,
                d.type_id,
                d.date_added,
                d.blob_file_name,
                d.version,
                d.status,
                d.file_extension
            }).Distinct();

            int docsQty = docs.Count();
            if (docsQty > 0)
            {
                decimal percentageByItem = ((decimal)60) / docsQty;
                foreach (var doc in distinctDocs)
                {

                    StringWriter stringWriter = new StringWriter();
                    stringWriter.Write("<html><head><meta charset='UTF-8'></head><body style=font-family:Arial; font-size:8pt;>");
                    stringWriter.Write("<div class=bookmark style=visibility:hidden>Table of Documents</div>");
                    stringWriter.Write("<h3 style='margin: 5px 0px;'>" + doc.project_description + "</h3>");
                    stringWriter.Write("<p align='left' style='font-size:16;margin: 5px 0px;'>" + doc.filename + "</p>");
                    stringWriter.Write("<p align='left' style='font-size:16;margin: 5px 0px;'>Version: " + doc.version + "    Date: " + doc.date_added.ToShortDateString() + "</p>");
                    stringWriter.Write("<table width=100% style='border: 1px solid black;border-collapse:collapse;'>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Code</th>");
                    stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Asset Description</th>");
                    stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Phase</th>");
                    stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Department</th>");
                    stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Room Name</th>");
                    stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Room No</th>");
                    stringWriter.Write("</tr>");

                    var associations = docs.Where(d => d.id == doc.id);
                    foreach (var row in associations)
                    {
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=10% class=text align=center>" + row.asset_code + "</td>");
                        stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=30% class=text>" + row.asset_description + "</td>");
                        stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=15% class=text>" + row.phase_description + "</td>");
                        stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=15% class=text>" + row.department_description + "</td>");
                        stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=10% class=text>" + row.room_number + "</td>");
                        stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;padding: 0px 5px;' width=15% class=text>" + row.room_name + "</td>");
                        stringWriter.Write("</tr>");
                    }

                    stringWriter.Write("</table><br></body></html>");
                    stringWriter.Close();

                    htmlPath = this.fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), Path.Combine(Domain.GetRoot(), "reports"), String.Format("{0}{1}_{2}_ShopToc.html", doc.project_domain_id.ToString(), doc.id.ToString(), doc.filename.Replace(" ", "")));
                    pdfPath = htmlPath.Replace("html", "pdf");
                    WkhtmltopdfRepository.ConvertToPDF(htmlPath, pdfPath);
                    paths.Add(pdfPath);
                    downloadedPath = DownloadDocument(doc.project_domain_id, doc.blob_file_name);
                    if (downloadedPath != null)
                    {
                        paths.Add(downloadedPath);
                    }

                    IncrementReportStatus(item, percentageByItem);
                }
            }

            return paths;
        }

        private string DownloadDocument(short domain_id, string blob_file_name)
        {
            if (blob_file_name != "")
            {
                string directory_doc = Path.Combine(Domain.GetRoot(), "projDocuments");
                string container_cutsheet = BlobContainersName.ProjDocuments(domain_id);

                fileRepository.CreateLocalDirectory(directory_doc);

                //Download document
                var blob_doc = fileRepository.GetBlob(container_cutsheet, blob_file_name);

                if (blob_doc.Exists())
                {
                    string path_doc = Path.Combine(directory_doc, domain_id.ToString() + blob_file_name);

                    try
                    {
                        blob_doc.DownloadTo(path_doc);

                        if (!(new System.IO.FileInfo(path_doc)).Exists)
                            return null;
                    }
                    catch (Exception) { return null; }

                    return path_doc;
                }
            }

            return null;
        }
    }
}