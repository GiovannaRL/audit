using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using reportsWebJob.Models;

namespace reportsWebJob.Services
{
    class AssetBookRepository : GenericReportRepository
    {
        private FileStreamRepository fileRepository;
        private string reportWhere;

        public AssetBookRepository()
        {
            fileRepository = new FileStreamRepository();
        }

        /* Build the asset book report */
        public void Build(project_report item)
        {
            InitiateReport(item);

            /* Cost Center */
            item.cost_center1 = GetCostCenter(item);

            string reports_path = Path.Combine(Domain.GetRoot(), "reports");

            string coverPath = CreateCover(item);
            UpdateReportStatus(item, 5);
            string tocPath = CreateToc(item);
            UpdateReportStatus(item, 10);

            string[] files_path = new string[] { coverPath.Replace("html", "pdf"), tocPath.Replace("html", "pdf") };
            string[] titles = new string[] { "Cover Page", "Table of Contents" };

            WkhtmltopdfRepository.ConvertToPDF(coverPath, files_path[0]);
            WkhtmltopdfRepository.ConvertToPDF(tocPath, files_path[1]);

            var cutsheets = GetCutSheets(item);
            files_path = files_path.Concat(cutsheets.Select(c => c.path)).ToArray();
            titles = titles.Concat(cutsheets.Select(c => c.title)).ToArray();


            string temporaryFilePath = Path.Combine(reports_path, item.id + ".pdf");
            UpdateReportStatus(item, 90);
            fileRepository.MergeFiles(files_path.ToArray(), temporaryFilePath, titles);
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
            else {
                CompleteReportError(item);
            }
        }


        /* Create the page that will contains the data of the assets */
        private string CreateToc(project_report item)
        {
            const string ROOM_QTY_SELECT = "IIF(pr.room_quantity > 0, pr.room_quantity, 1)";

            const string BUDGET_QTY_SELECT = "CASE WHEN am.eq_unit_desc = 'per sf' THEN CASE WHEN COALESCE(pri.budget_qty, 0) = 0 THEN 0 ELSE 1 END ELSE (COALESCE(pri.budget_qty, 0) * " + ROOM_QTY_SELECT + ") END";
            const string LEASE_QTY_SELECT = "CASE WHEN am.eq_unit_desc = 'per sf' THEN CASE WHEN COALESCE(pri.lease_qty, 0) = 0 THEN 0 ELSE 1 END ELSE (COALESCE(pri.lease_qty, 0) * " + ROOM_QTY_SELECT + ") END";
            const string DNP_QTY_SELECT = "CASE WHEN am.eq_unit_desc = 'per sf' THEN CASE WHEN COALESCE(pri.dnp_qty, 0) = 0 THEN 0 ELSE 1 END ELSE (COALESCE(pri.dnp_qty, 0) *" + ROOM_QTY_SELECT + ") END";
            const string PO_QTY_SELECT = "CASE WHEN am.eq_unit_desc = 'per sf' THEN CASE WHEN COALESCE(ipo.po_qty, 0) = 0 THEN 0 ELSE 1 END ELSE COALESCE(ipo.po_qty, 0) END";

            this.reportWhere = GetWhereClause(item, "pri", "pri");

            string select = "SELECT " +
                "   COALESCE(pri.resp, a.default_resp) AS resp, " +
                    (item.use_cad_id == true ? "COALESCE(NULLIF(TRIM(pri.cad_id),''), a.asset_code)" : "a.asset_code") + " AS asset_code, " +
                "   pri.asset_description, " +
                "   pri.jsn_code AS JSN, " +
                "   SUM(" + BUDGET_QTY_SELECT + ") AS budget_qty, " +
                "   SUM(" + LEASE_QTY_SELECT + ") AS lease_qty, " +
                "   SUM(" + DNP_QTY_SELECT + ") AS dnp_qty, " +
                "   SUM(" + PO_QTY_SELECT + ") AS po_qty, " +
                "   pri.jsn_utility1, pri.jsn_utility2, pri.jsn_utility3, pri.jsn_utility4, pri.jsn_utility5, " +
                "   pri.jsn_utility6, pri.jsn_utility7, " +
                "   COALESCE(asset_photo_doc.blob_file_name, a.photo) AS photo, " +
                "   asset_photo_doc.rotate AS photo_rotate, " +
                "   NULLIF(pri.tag, '') as tag, " +

                "   pri.amps, " +
                "   pri.volts, " +
                "   pri.plug_type " +

                " FROM project_room_inventory pri " +
                "   INNER JOIN project_room pr ON pr.project_id = pri.project_id and pr.phase_id = pri.phase_id and pr.department_id = pri.department_id and pr.room_id = pri.room_id and pr.domain_id = pri.domain_id " +
                "   LEFT JOIN assets a ON pri.asset_domain_id = a.domain_id AND pri.asset_id = a.asset_id " +
                "   LEFT JOIN assets_measurement am ON a.eq_measurement_id = am.eq_unit_measure_id " +
                "   LEFT JOIN inventory_purchase_order ipo ON ipo.inventory_id = pri.inventory_id " +
                "   LEFT JOIN ( " +
                "       SELECT pdoc.blob_file_name AS blob_file_name, pdoc.rotate, da.inventory_id from documents_associations da " +
                "           INNER JOIN project_documents pdoc ON pdoc.id = da.document_id AND pdoc.project_domain_id = da.project_domain_id " +
                "       WHERE pdoc.type_id = 6 " +
                "   ) AS asset_photo_doc ON asset_photo_doc.inventory_id = pri.inventory_id"
                + this.reportWhere + " AND a.approval_pending_domain IS NULL " +
                " GROUP BY " + 
                "   COALESCE(pri.resp, a.default_resp), " +
                    (item.use_cad_id == true ? "COALESCE(NULLIF(TRIM(pri.cad_id),''), a.asset_code)" : "a.asset_code") +
                "   , pri.asset_description, pri.jsn_code, " +
                "   pri.jsn_utility1, pri.jsn_utility2, pri.jsn_utility3, pri.jsn_utility4, pri.jsn_utility5, " +
                "   pri.jsn_utility6, pri.jsn_utility7, " +
                "   COALESCE(asset_photo_doc.blob_file_name, a.photo), " +
                "   asset_photo_doc.rotate, " +
                "   NULLIF(pri.tag, '')," +
                "   pri.amps, " +
                "   pri.volts, " +
                "   pri.plug_type " +



                " ORDER BY " + (item.use_cad_id == true ? "COALESCE(NULLIF(TRIM(pri.cad_id),''), a.asset_code)" : "a.asset_code");

            IEnumerable<AssetBookTocItem> data = _db.Database.SqlQuery<AssetBookTocItem>(select).ToList();

            /* Mount data report */
            StringWriter stringWriter = new StringWriter();
            stringWriter.Write("<html><head><meta charset='UTF-8'></head><body style=font-family:Arial; font-size:8pt;>");
            stringWriter.Write("<div class=bookmark style=visibility:hidden>Table of Contents</div>");
            stringWriter.Write("<h3>New Asset</h3>");
            stringWriter.Write("<table width=100% style='border: 1px solid black;border-collapse:collapse;'>");
            stringWriter.Write("<tr>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Resp</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Code</th>");
            if (item.include_jsn)
                stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>JSN</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Description</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Planned Qty</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>Lease Qty</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>DNP Qty</th>");
            stringWriter.Write("<th style='border: 1px solid black;border-collapse:collapse;'>PO Qty</th>");
            stringWriter.Write("</tr>");

            foreach (var asset in data)
            {
                stringWriter.Write("<tr>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=7% class=text align=center>" + asset.resp + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=7% class=text align=center>" + asset.asset_code + "</td>");
                if (item.include_jsn)
                    stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=7% class=text align=center>" + asset.JSN + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=50% class=text>" + asset.asset_description + (asset.tag != null && !asset.tag.Equals("") ? "(" + asset.tag + ")" : "") + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=9% class=integer align=right>" + asset.budget_qty + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=9% class=integer align=right>" + asset.lease_qty + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=9% class=integer align=right>" + asset.dnp_qty + "</td>");
                stringWriter.Write("<td style='border: 1px solid black;border-collapse:collapse;' width=9% class=integer align=right>" + asset.po_qty + "</td>");
                stringWriter.Write("</tr>");
            }

            stringWriter.Write("</table><br></body></html>");   
            stringWriter.Close();

            return this.fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), Path.Combine(Domain.GetRoot(), "reports"), item.id.ToString() + item.project_domain_id + "Toc.html");
        }

        /* Create the cover page and returns the path where it is saved*/
        private string CreateCover(project_report item)
        {
            var project = _db.projects.Where(x => x.project_id == item.project_id && x.domain_id == item.project_domain_id).FirstOrDefault().project_description;
            
            /* Mount data report */
            StringWriter stringWriter = new StringWriter();
            stringWriter.Write("<html><head><meta charset='UTF-8'></head><body style='font-family:Arial;'>");
            stringWriter.Write("<title>Cover</title><table border='0' 'border-collapse: collapse' width='100%' cellpadding='0' height='300'>");
            stringWriter.Write("<tr><td><p align='right' style='font-size:24'><img border='0' height=\"40\" src='" + Path.Combine(Domain.GetRoot(), "images", "logo_aw.png") + "'><br><br>" + item.name + "</td></tr>");
            stringWriter.Write("<tr><td><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><p align='right' style='font-size:24'>" + item.description + "</td></tr>");
            stringWriter.Write("<tr><td><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><p align='right' style='font-size:24'>" + project + "</br>" + (item.cost_center1 != null ? item.cost_center1.description ?? "All Cost Centers" : "All Cost Centers") + "<br>Medical Equipment Binder<br>" + item.last_run.ToString("MM/dd/yyyy") + "</td></tr>");
            stringWriter.Write("</body></html>");
            stringWriter.Close();

            return this.fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), Path.Combine(Domain.GetRoot(), "reports"), item.id.ToString() + item.project_domain_id + "Cover.html");
        }

        private bool HasShopDrawing(AssetBookInventoryItem assetInfo, project_report report) {
            return _db.asset_inventory.Where(pri => pri.domain_id == report.project_domain_id && pri.project_id == report.project_id
             && (report.phase_id == null || (pri.phase_id == report.phase_id && (report.department_id == null ||
             (pri.department_id == report.department_id && (report.room_id == null || pri.room_id == report.room_id)))))
             && pri.asset_domain_id == assetInfo.asset_domain_id && pri.asset_id == assetInfo.asset_id && 
             pri.has_shop_drawing == true)
             .FirstOrDefault() != null;
        }

        private PDFPageInfo[] GetCutSheets(project_report item)
        {
            List<PDFPageInfo> paths = new List<PDFPageInfo>();

            if (item.cost_center1 != null)
                this.reportWhere += " AND pri.cost_center_id = " + item.cost_center1.id;

            string select = "SELECT " +
                "   COALESCE(NULLIF(TRIM(pri.cad_id),''), a.asset_code) AS cad_id," +
                "   a.domain_id AS asset_domain_id, a.asset_id, " +
                "   pri.manufacturer_description, " +
                    (item.ignore_description_difference == true? "  a.asset_description, " : "   pri.asset_description, ") +
                "   a.asset_code, " +
                "   pri.model_number, " +
                "   pri.model_name, " +
                "   COALESCE(asset_photo_doc.blob_file_name, a.photo) AS photo, " +
                "   CASE WHEN asset_photo_doc.blob_file_name is null THEN pri.asset_domain_id ELSE pri.domain_id END as photo_domain_id, " +
                "   asset_photo_doc.rotate AS photo_rotate, " +
                "   a.comment, " +
                "   COALESCE(pri.height, a.height) AS height, COALESCE(pri.width, a.width) AS width, COALESCE(pri.depth, a.depth) AS depth, " +
                "   COALESCE(pri.placement, a.placement) AS placement, " +
                "   pri.jsn_code AS JSN, " +
                "   COALESCE(pri.jsn_utility1, 'N/A') as jsn_utility1, COALESCE(pri.jsn_utility2, 'N/A') as jsn_utility2, " +
                "   COALESCE(pri.jsn_utility3, 'N/A') as jsn_utility3, COALESCE(pri.jsn_utility4, 'N/A') as jsn_utility4, " +
                "   COALESCE(pri.jsn_utility5, 'N/A') as jsn_utility5, COALESCE(pri.jsn_utility6, 'N/A') as jsn_utility6, " +
                "   COALESCE(pri.jsn_utility7, 'N/A') as jsn_utility7, " +
                "   pri.class, clin, ECN, " +
                "   CASE WHEN " +
                    (item.ignore_description_difference == true ? "" : "       pri.asset_description_ow = 1 OR ") +
                "       pri.placement_ow = 1 OR pri.class_ow = 1 OR pri.jsn_ow = 1 OR " +
                "       pri.manufacturer_description_ow = 1 OR pri.model_number_ow = 1 OR pri.model_name_ow = 1 OR " +
                "       pri.class <> a.class OR asset_photo_doc.blob_file_name IS NOT NULL OR pri.resp <> a.default_resp " +
                "   THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS has_overwritten_properties,  " +

                "   CASE WHEN " +
                "       pri.height_ow = 1 OR pri.width_ow = 1 OR pri.depth_ow = 1 OR pri.mounting_height_ow = 1 OR " +
                "       pri.amps_ow = 1 OR pri.volts_ow = 1 OR pri.plug_type_ow = 1 OR pri.connection_type_ow = 1 OR " +
                "       pri.bluetooth_ow = 1 OR pri.cat6_ow = 1 OR pri.displayport_ow = 1 OR pri.dvi_ow = 1 OR pri.hdmi_ow = 1 OR" +
                "       pri.wireless_ow = 1 OR pri.network_option_ow = 1 OR pri.ports_ow = 1" +
                "   THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS has_overwritten_attributes,  " +

                "   MIN(pri.cut_sheet_filename) AS filename, " + // If none of the files has a cut_sheet it will return null
                " a.cut_sheet, " +
                "   STRING_AGG(CAST(pri.inventory_id AS VARCHAR(MAX)), ';') AS inventories_id" +
                " FROM project_room_inventory pri " +
                "   LEFT JOIN assets a ON pri.asset_domain_id = a.domain_id AND pri.asset_id = a.asset_id " +
                "   LEFT JOIN ( " +
                "       SELECT pdoc.blob_file_name AS blob_file_name, pdoc.rotate , da.inventory_id from documents_associations da " +
                "           INNER JOIN project_documents pdoc ON pdoc.id = da.document_id AND pdoc.project_domain_id = da.project_domain_id " +
                "       WHERE pdoc.type_id = 6 " +
                "   ) AS asset_photo_doc ON asset_photo_doc.inventory_id = pri.inventory_id"
                + this.reportWhere + " AND a.approval_pending_domain IS NULL " +
                " GROUP BY " +
                "   COALESCE(NULLIF(TRIM(pri.cad_id),''), a.asset_code)," + 
                "   a.asset_code," +
                "   a.domain_id, a.asset_id, " +
                    (item.ignore_description_difference == true ? "  a.asset_description, " : "  pri.asset_description, ") +
                "   pri.manufacturer_description, a.asset_code, " +
                "   pri.model_number, pri.model_name, " +
                "   COALESCE(asset_photo_doc.blob_file_name, a.photo), " +
                "   CASE WHEN asset_photo_doc.blob_file_name is null THEN pri.asset_domain_id ELSE pri.domain_id END, " +
                "   asset_photo_doc.rotate, " +
                "   a.comment, " +
                "   COALESCE(pri.height, a.height), COALESCE(pri.width, a.width), COALESCE(pri.depth, a.depth), " +
                "   COALESCE(pri.placement, a.placement), " +
                "   pri.jsn_code, " +
                "   pri.jsn_utility1, pri.jsn_utility2, pri.jsn_utility3, pri.jsn_utility4, pri.jsn_utility5, " +
                "   pri.jsn_utility6, pri.jsn_utility7, " +
                "   pri.class, clin, ECN, " +
                "   CASE WHEN " +
                    (item.ignore_description_difference == true ? "" : "       pri.asset_description_ow = 1 OR ") +
                "       pri.placement_ow = 1 OR pri.class_ow = 1 OR pri.jsn_ow = 1 OR " +
                "       pri.manufacturer_description_ow = 1 OR pri.model_number_ow = 1 OR pri.model_name_ow = 1 OR " +
                "       pri.class <> a.class OR asset_photo_doc.blob_file_name IS NOT NULL OR pri.resp <> a.default_resp " +
                "   THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END ," +
                "   CASE WHEN" +
                "       pri.height_ow = 1 OR pri.width_ow = 1 OR pri.depth_ow = 1 OR pri.mounting_height_ow = 1 OR" +
                "       pri.amps_ow = 1 OR pri.volts_ow = 1 OR pri.plug_type_ow = 1 OR pri.connection_type_ow = 1 OR " +
                "       pri.bluetooth_ow = 1 OR pri.cat6_ow = 1 OR pri.displayport_ow = 1 OR pri.dvi_ow = 1 OR pri.hdmi_ow = 1 OR" +
                "       pri.wireless_ow = 1 OR pri.network_option_ow = 1 OR pri.ports_ow = 1" +
                "   THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END ," +
                " a.cut_sheet " +
                " ORDER BY " + 
                (item.use_cad_id == true ? "COALESCE(NULLIF(TRIM(pri.cad_id),''), a.asset_code)" : "a.asset_code");

            IEnumerable<AssetBookInventoryItem> items = _db.Database.SqlQuery<AssetBookInventoryItem>(select).ToList();
            
            if (items != null && items.Count() > 0)
            {
                decimal percentageByItem = ((decimal)80) / items.Count();
                using (CutSheetRepository cutsheetRepository = new CutSheetRepository(1))
                {
                    string directory = Path.Combine(Domain.GetRoot(), "reports");
                    fileRepository.CreateLocalDirectory(directory);
                    if (item.include_cutsheets == true)
                    {
                        foreach (AssetBookInventoryItem assetItem in items)
                        {
                            string inventoryId = assetItem.inventories_id.Split(';').FirstOrDefault();                           
                            bool shopDrawing = HasShopDrawing(assetItem, item);
                            paths.Add(new PDFPageInfo(Path.Combine(directory, item.id + "_" + assetItem.asset_domain_id.ToString() + inventoryId + (shopDrawing ? "ShopD.pdf" : ".pdf")), item.use_cad_id == true ? assetItem.cad_id : assetItem.asset_code));
                            cutsheetRepository.BuildFull(assetItem, paths.Last().path, item, shopDrawing);
                            IncrementReportStatus(item, percentageByItem);
                        }
                    }
                    else
                    {
                        foreach (AssetBookInventoryItem assetItem in items)
                        {
                            string inventoryId = assetItem.inventories_id.Split(';').FirstOrDefault();
                            bool shopDrawing = HasShopDrawing(assetItem, item);
                            string title = item.use_cad_id == true ? assetItem.cad_id : assetItem.asset_code;
                            string fileName = Path.Combine(directory, item.id + "_" + assetItem.asset_domain_id.ToString() + inventoryId + (shopDrawing ? "ShopDrawing.pdf" : ".pdf"));
                            paths.Add(new PDFPageInfo(fileName, title));
                            if (shopDrawing)
                            {
                                fileName = cutsheetRepository.DownloadShopDCoverSheet(assetItem, null, fileName);
                            } else
                            {
                                fileName = cutsheetRepository.DownloadCoverSheet(assetItem, null, fileName);
                            }

                            if(fileName == null)
                            {
                                cutsheetRepository.CreateCoverSheet(assetItem, paths.Last().path, null, item, shopDrawing);
                            }
                            IncrementReportStatus(item, percentageByItem);
                        }
                    }
                }
            }
            else
            {
                IncrementReportStatus(item, 80);
            }
            return paths.ToArray();
        }
    }
}
