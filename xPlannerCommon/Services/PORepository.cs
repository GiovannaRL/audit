using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace xPlannerCommon.Services
{
    public class PORepository : IDisposable
    {
        private audaxwareEntities _db;
        private CultureInfo culture = CultureInfo.GetCultureInfo("en");
        private bool _disposed = false;
        private string poDirectory;

        public PORepository()
        {
            this._db = new audaxwareEntities();
            this.poDirectory = Path.Combine(Domain.GetRoot(), "pos");
            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                fileRepository.CreateLocalDirectory(this.poDirectory);
            }
        }

        public PORepository(short domain_id)
        {
            this._db = new audaxwareEntities();
            this.poDirectory = Path.Combine(Domain.GetRoot(), "pos");
            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                fileRepository.CreateLocalDirectory(this.poDirectory);
            }
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '" + domain_id + "';";
                cmd.ExecuteNonQuery();
            }
        }

        private string GetFilename(int domain_id, int project_id, int po_id)
        {
            return domain_id.ToString() + project_id.ToString() + po_id.ToString();
        }

        public string CreatePODetailsPage(int domain_id, int project_id, int po_id)
        {

            purchase_order po = this._db.purchase_order.Include("project").Include("project_addresses").Include("vendor.vendor_contact").Include("vendor_contact")
                .Include("inventory_purchase_order").Where(p => p.domain_id == domain_id && p.project_id == project_id
                && p.po_id == po_id).FirstOrDefault();

            if (po != null)
            {
                /* Filename and path */
                string poFilename = this.GetFilename(po.domain_id, po.project_id, po.po_id);
                //+ "DetailsCover";

                using (FileStreamRepository fileRep = new FileStreamRepository())
                {

                    //var po_blob = fileRep.GetBlob(BlobsName.POCover(po.domain_id), poFilename + ".pdf");
                    string pdfPath = Path.Combine(this.poDirectory, poFilename + "DetailsCover.pdf");

                    //if (po_blob.Exists())
                    //{
                    //    po_blob.DownloadToFile(pdfPath, FileMode.Create);
                    //}
                    //else
                    //{
                    po.project_addresses = UpdateAddress(po.project_addresses);

                    project_addresses proj_address = UpdateAddress(this._db.project_addresses.Where(pa => pa.domain_id == domain_id &&
                        pa.project_id == project_id && pa.is_default == true).FirstOrDefault());

                    StringWriter stringWriter = new StringWriter();
                    stringWriter.Write("<html><head><meta charset='UTF-8'>");
                    stringWriter.Write("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "css", "PODetails.css") + "' rel='stylesheet'/>");
                    stringWriter.Write("</head><body style=font-family:Arial; font-size:8pt;>");
                    stringWriter.Write("<table>");

                    /* First row of tables */
                    stringWriter.Write("<tr>");
                    /* Project Information Table */
                    stringWriter.Write("<td>");
                    stringWriter.Write("<h3 class=\"toptable\">Purchase Order</h3>");
                    stringWriter.Write("<table class=\"datatable\" cellspacing=\"0\" cellpadding=\"0\">");

                    stringWriter.Write("<tr class=\"header\">");
                    stringWriter.Write("<th colspan=\"2\" align=left>PROJECT:</th>");
                    stringWriter.Write("<th align=left colspan=\"25\">" + po.project.project_description + "</th>");
                    stringWriter.Write("</tr>");

                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td class=\"label\"><strong>Address:</strong></td>");
                    stringWriter.Write("<td class=\"large-space\"></td>");
                    stringWriter.Write("<td align=left colspan=\"9\" class=\"address-value\">" + proj_address.nickname + "</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("<td colspan=\"5\" width=\"50px\"><strong>Ship To:</strong></td>");
                    stringWriter.Write("<td class=\"large-space\"></td>");
                    stringWriter.Write("<td colspan=\"8\" class=\"address-value\">" + po.project_addresses.nickname + "</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("</tr>");

                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td align=left colspan=\"9\" class=\"address-value\">" + proj_address.address1 + "&nbsp;</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("<td colspan=\"5\"></td>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td colspan=\"8\" class=\"address-value\">" + po.project_addresses.address1 + "&nbsp;</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("</tr>");

                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td align=left colspan=\"9\" class=\"address-value\">" + proj_address.address2 + "&nbsp;</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("<td colspan=\"5\"></td>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td colspan=\"8\" class=\"address-value\">" + po.project_addresses.address2 + "&nbsp;</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("</tr>");

                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td align=left colspan=\"9\" class=\"address-value\">" + FormatLastAddressLine(proj_address) + "</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("<td colspan=\"5\"></td>");
                    stringWriter.Write("<td></td>");
                    stringWriter.Write("<td colspan=\"8\" class=\"address-value\">" + FormatLastAddressLine(po.project_addresses) + "</td>");
                    stringWriter.Write("<td class=\"blank-space\"></td>");
                    stringWriter.Write("</tr>");

                    stringWriter.Write("</table>");
                    stringWriter.Write("</td>");

                    /* Horizontal space between tables */
                    stringWriter.Write("<td class=\"divider-column\"></td>");

                    /* Vendor Table */
                    stringWriter.Write("<td>");
                    stringWriter.Write("<h3 align=right class=\"toptable\">" + po.po_number + "&nbsp;</h3>");
                    stringWriter.Write("<table class=\"datatable smaller\" cellspacing=\"0\" cellpadding=\"0\">");
                    stringWriter.Write("<tr class=\"header\">");
                    stringWriter.Write("<th colspan=\"2\" align=left>VENDOR:</th>");
                    stringWriter.Write("<th align=left colspan=\"13\">" + (po.vendor != null ? po.vendor.name : "No vendor information") + "</th>");
                    stringWriter.Write("</tr>");

                    if (po.vendor_contact.Count > 0)
                    {
                        var item = po.vendor_contact.First();
                        
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td class=\"label\"><strong>Address:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td align=left colspan=\"6\" class=\"address-value\">" + item.address + "</td>"); // value
                        stringWriter.Write("<td class=\"label\"><strong>Phone:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\">" + item.phone + "</td>"); //phone
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td align=left colspan=\"6\" class=\"address-value\"></td>"); // value
                        stringWriter.Write("<td class=\"label\"><strong>Fax:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\">" + item.fax + "</td>"); // fax
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td align=left colspan=\"6\" class=\"address-value\"></td>"); //value
                        stringWriter.Write("<td class=\"label\"><strong>Mobile:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\">" + item.mobile + "</td>"); // mobile
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td class=\"label\"><strong>Contact:</strong></td>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td align=left colspan=\"6\">" + item.name + "</td>"); // contact
                        stringWriter.Write("<td class=\"label\"><strong>Email:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\">" + item.email + "</td>"); // email
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                    }
                    else
                    {
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td class=\"label\"><strong>Address:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td align=left colspan=\"6\" class=\"address-value\"></td>"); // value
                        stringWriter.Write("<td class=\"label\"><strong>Phone:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\"></td>"); //phone
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td align=left colspan=\"6\" class=\"address-value\"></td>"); // value
                        stringWriter.Write("<td class=\"label\"><strong>Fax:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\"></td>"); // fax
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td align=left colspan=\"6\" class=\"address-value\"></td>"); //value
                        stringWriter.Write("<td class=\"label\"><strong>Mobile:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\"></td>"); // mobile
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td class=\"label\"><strong>Contact:</strong></td>");
                        stringWriter.Write("<td></td>");
                        stringWriter.Write("<td align=left colspan=\"6\"></td>"); // contact
                        stringWriter.Write("<td class=\"label\"><strong>Email:</strong></td>");
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("<td colspan=\"4\" class=\"value\"></td>"); // email
                        stringWriter.Write("<td class=\"blank-space\"></td>");
                        stringWriter.Write("</tr>");
                    }
                    
                    stringWriter.Write("</table>");

                    stringWriter.Write("</td>");

                    stringWriter.Write("</tr>");

                    /*Vertical space between tables */
                    stringWriter.Write("<tr><td></td></tr>");

                    /* Second row of tables */
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td>");
                    stringWriter.Write("<table class=\"datatable\" cellspacing=\"0\" cellpadding=\"0\">");
                    stringWriter.Write("<tr class=\"header\">");
                    stringWriter.Write("<th colspan=\"30\" align=left>PO DESCRIPTION: " + po.description + "</th>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td><strong>Status:</strong></td>");
                    stringWriter.Write("<td>" + po.status + "</td>");
                    stringWriter.Write("<td class=\"large-space\"></td>");
                    stringWriter.Write("<td><strong>Requisition Number:</strong></td>");
                    stringWriter.Write("<td>" + po.po_requested_number + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td><strong>Quote Requested:</strong></td>");
                    stringWriter.Write("<td>" + (po.quote_requested_date != null ? ((DateTime)po.quote_requested_date).ToShortDateString() : "") + "</td>");
                    stringWriter.Write("<td class=\"large-space\"></td>");
                    stringWriter.Write("<td><strong>PO Requested:</strong></td>");
                    stringWriter.Write("<td>" + (po.po_requested_date != null ? ((DateTime)po.po_requested_date).ToShortDateString() : "") + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td><strong>Quote Received:</strong></td>");
                    stringWriter.Write("<td>" + (po.quote_received_date != null ? ((DateTime)po.quote_received_date).ToShortDateString() : "") + "</td>");
                    stringWriter.Write("<td class=\"large-space\"></td>");
                    stringWriter.Write("<td><strong>PO Issued:</strong></td>");
                    stringWriter.Write("<td>" + (po.po_received_date != null ? ((DateTime)po.po_received_date).ToShortDateString() : "") + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td><strong>Quote Number:</strong></td>");
                    stringWriter.Write("<td>" + po.quote_number + "</td>");
                    stringWriter.Write("<td class=\"large-space\"></td>");
                    stringWriter.Write("<td><strong>PO Number:</strong></td>");
                    stringWriter.Write("<td>" + po.po_number + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("</table>");
                    stringWriter.Write("</td>");
                    stringWriter.Write("<td class=\"divider-column\"></td>");
                    stringWriter.Write("<td>");
                    stringWriter.Write("<table class=\"datatable smaller\" cellspacing=\"0\" cellpadding=\"0\">");
                    stringWriter.Write("<tr class=\"header\">");
                    stringWriter.Write("<th align=left>COMMENTS/SPECIAL INSTRUCTIONS:</th>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td>" + po.comment + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("</table>");
                    stringWriter.Write("</td>");
                    stringWriter.Write("</tr>");

                    /*Vertical space between tables */
                    stringWriter.Write("<tr><td></td></tr>");

                    Decimal po_total = 0;

                    if (po.inventory_purchase_order != null && po.inventory_purchase_order.Count() > 0)
                    {
                        
                        StringBuilder select = new StringBuilder();
                        select.Append("SELECT a.asset_code, coalesce(pri.jsn_code, '') as jsn_code, pri.asset_description, pri.manufacturer_description AS manufacturer, coalesce(pri.serial_number, '') as serial_number, coalesce(pri.serial_name,'') as serial_name, ");
                        select.Append("COALESCE(ipo.po_unit_amt, 0) AS po_unit_amt, upper(am.eq_unit_desc) AS UOM, ");
                        select.Append("SUM(CASE WHEN lower(am.eq_unit_desc) = 'per sf' THEN ");
                        select.Append("CASE WHEN COALESCE(ipo.po_qty, 0) = 0 THEN 0 ELSE 1 END ELSE COALESCE(ipo.po_qty, 0) END) AS po_qty, ");
                        select.Append("ROUND(SUM(COALESCE(ipo.po_qty, 0) * COALESCE(ipo.po_unit_amt, 0)), 2) AS total_po_amt, ");
                        select.Append("STUFF(( SELECT distinct '; ' + av.model_number FROM assets_vendor as av ");
                        select.Append("WHERE a.domain_id = av.asset_domain_id AND a.asset_id = av.asset_id and a.domain_id = av.vendor_domain_id ");
                        select.Append("FOR XML PATH('')),1 ,1, '')  as vendor_model ");
                        select.Append("FROM inventory_purchase_order AS ipo INNER JOIN assets AS a ON ipo.asset_domain_id = a.domain_id AND ipo.asset_id = a.asset_id ");
                        select.Append("INNER JOIN project_room_inventory AS pri ON ipo.inventory_id = pri.inventory_id ");
                        select.Append("INNER JOIN assets_measurement AS am ON am.eq_unit_measure_id = a.eq_measurement_id ");
                        select.Append("WHERE ipo.po_domain_id = " + domain_id + " AND ipo.project_id = " + project_id + " AND ipo.po_id = " + po_id);
                        select.Append(" GROUP BY a.asset_id, a.domain_id, a.asset_code, coalesce(pri.jsn_code, ''), pri.asset_description, pri.manufacturer_description, coalesce(pri.serial_number, ''), coalesce(pri.serial_name,''), COALESCE(ipo.po_unit_amt, 0), am.eq_unit_desc ");
                        select.Append("ORDER BY a.asset_code;");

                        List<POAssetItem> assets = this._db.Database.SqlQuery<POAssetItem>(select.ToString()).ToList();
                        var showJSN = false;
                        if (assets.Count() > 0 && assets.Where(x => x.jsn_code != "").Count() > 0)
                            showJSN = true;

                        /* Assets table */
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td colspan=\"3\">");
                        stringWriter.Write("<table class=\"asset-table\" cellspacing=\"0\" cellpadding=\"0\">");
                        stringWriter.Write("<tr class=\"main-header\">");
                        stringWriter.Write("<th align=left>PO ASSETS:</th>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("<tr>");
                        stringWriter.Write("<td>");
                        stringWriter.Write("<table class=\"asset-datatable\" cellspacing=\"0\" cellpadding=\"0\">");
                        stringWriter.Write("<tr class=\"secondary-header\">");
                        stringWriter.Write("<th>ASSET CODE</th>");
                        if (showJSN)
                            stringWriter.Write("<th>JSN</th>");

                        stringWriter.Write("<th>DESCRIPTION</th>");
                        stringWriter.Write("<th>VENDOR MODEL</th>");
                        stringWriter.Write("<th>MANUFACTURER/MODEL</th>");
                        stringWriter.Write("<th>UOM</th>");
                        stringWriter.Write("<th>PO QTY</th>");
                        stringWriter.Write("<th colspan=\"2\">UNIT AMOUNT</th>");
                        stringWriter.Write("<th colspan=\"2\">TOTAL AMOUNT</th>");
                        stringWriter.Write("</tr>");


                        foreach (POAssetItem asset in assets)
                        {
                            var model = asset.serial_number;
                            if (asset.serial_name.Length > 0)
                                model += "<br>" + asset.serial_name;
                            
                            stringWriter.Write("<tr>");
                            stringWriter.Write("<td align=center>" + asset.asset_code + "</td>");
                            if (showJSN)
                                stringWriter.Write("<td align=center>" + asset.jsn_code + "</td>");

                            stringWriter.Write("<td align=left>" + asset.asset_description + "</td>");
                            stringWriter.Write("<td align=center>" + asset.vendor_model + "</td>");
                            stringWriter.Write("<td align=left>" + asset.manufacturer + " / " + model +  "</td>");
                            stringWriter.Write("<td align=center>" + asset.UOM + "</td>");
                            stringWriter.Write("<td align=center>" + asset.po_qty + "</td>");
                            stringWriter.Write("<td class=\"money left\">$</td>");
                            stringWriter.Write("<td class=\"money right\">" + asset.po_unit_amt.ToString("C", culture).Substring(1) + "</td>");
                            stringWriter.Write("<td class=\"money left\">$</td>");
                            stringWriter.Write("<td class=\"money right\">" + asset.total_po_amt.ToString("C", culture).Substring(1) + "</td>");
                            stringWriter.Write("</tr>");
                        }

                        po_total = assets.Sum(a => a.total_po_amt);

                        stringWriter.Write("<tr class=\"noborder\">");
                        stringWriter.Write("<td colspan=\"8\" align=right><strong>SUBTOTAL:</strong></td>");
                        stringWriter.Write("<td align=left>$</td>");
                        stringWriter.Write("<td align=right>" + po_total.ToString("C", culture).Substring(1) + "</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("</table>");
                        stringWriter.Write("</td>");
                        stringWriter.Write("</tr>");
                        stringWriter.Write("</table>");
                        stringWriter.Write("</td>");
                        stringWriter.Write("</tr>");


                        /*Vertical space between tables */
                        stringWriter.Write("<tr><td></td></tr>");
                    }

                    /* Last table */
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td colspan=\"3\" align=right>");
                    stringWriter.Write("<table cellspacing=\"0\" cellpadding=\"0\">");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td align=right><strong>FREIGHT:<strong></td>");
                    stringWriter.Write("<td class=\"money left\">$</td>");
                    stringWriter.Write("<td class=\"money right\">" + (po.freight ?? 0).ToString("C", this.culture).Substring(1) + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td align=right><strong>SALES TAX:<strong></td>");
                    stringWriter.Write("<td class=\"money left\">$</td>");
                    stringWriter.Write("<td class=\"money right\">" + (po.tax ?? 0).ToString("C", this.culture).Substring(1) + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td align=right><strong>WARRANTY:<strong></td>");
                    stringWriter.Write("<td class=\"money left\">$</td>");
                    stringWriter.Write("<td class=\"money right\">" + (po.warranty ?? 0).ToString("C", this.culture).Substring(1) + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td align=right><strong>MISC:<strong></td>");
                    stringWriter.Write("<td class=\"money left\">$</td>");
                    stringWriter.Write("<td class=\"money right\">" + (po.misc ?? 0).ToString("C", this.culture).Substring(1) + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td align=right><strong>INSTALLATION COST:<strong></td>");
                    stringWriter.Write("<td class=\"money left\">$</td>");
                    stringWriter.Write("<td class=\"money right\">" + (po.install ?? 0).ToString("C", this.culture).Substring(1) + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td align=right><strong>QUOTE DISCOUNT:<strong></td>");
                    stringWriter.Write("<td class=\"money left\">$</td>");
                    stringWriter.Write("<td class=\"money right\">- " + (po.quote_discount ?? 0).ToString("C", this.culture).Substring(1) + "</td>");
                    stringWriter.Write("</tr>");
                    stringWriter.Write("<tr>");
                    stringWriter.Write("<td align=right><strong>PO TOTAL:<strong></td>");
                    stringWriter.Write("<td class=\"money left po-total\">$</td>");
                    stringWriter.Write("<td class=\"money right po-total\">" + (po_total + (po.misc ?? 0) + (po.warranty ?? 0) + (po.tax ?? 0) + (po.freight ?? 0) + (po.install ?? 0) - (po.quote_discount ?? 0)).ToString("C", culture).Substring(1) + "</td>"); // po total
                    stringWriter.Write("</tr>");
                    stringWriter.Write("</table>");
                    stringWriter.Write("</td>");
                    stringWriter.Write("</tr>");

                    stringWriter.Write("</table>");
                    stringWriter.Write("</body></html>");

                    stringWriter.Close();


                    string htmlPath = fileRep.SaveDocumentTemporarily(stringWriter.ToString(), Path.Combine(this.poDirectory, poFilename + ".html"));
                    WkhtmltopdfRepository.ConvertToPDF(htmlPath, pdfPath);
                    //}
                    return pdfPath;
                }
            }
            return null;
        }

        public bool CreatePODetailsPageAndUpload(int domain_id, int project_id, int po_id)
        {
            string localPath = CreatePODetailsPage(domain_id, project_id, po_id);

            using (FileStreamRepository fileRep = new FileStreamRepository())
            {
                fileRep.UploadToCloud(localPath, BlobContainersName.POCover(domain_id), this.GetFilename(domain_id, project_id, po_id) + ".pdf");
                fileRep.DeleteFile(localPath);

                return true;
            }
        }

        /* Return the path where uploaded po was saved or null if this file have no uploaded po */
        public string GetUploadedPO(short domain_id, int project_id, int po_id)
        {
            purchase_order po = this._db.purchase_order.Where(p => p.domain_id == domain_id && p.project_id == project_id && p.po_id == po_id).FirstOrDefault();

            if (po != null && po.po_file != null && !po.po_file.Equals(""))
            {
                using (FileStreamRepository fileRepository = new FileStreamRepository())
                {
                    var blob_file = fileRepository.GetBlob(BlobContainersName.PO(po.domain_id), po.po_file);

                    if (blob_file.Exists())
                    {
                        string filename = domain_id.ToString() + project_id.ToString() + po_id.ToString() + "Details.pdf";

                        blob_file.DownloadTo(Path.Combine(this.poDirectory, filename));

                        return Path.Combine(this.poDirectory, filename);
                    }
                }
            }

            return null;
        }

        public string GetUploadedQuote(int domain_id, int project_id, int po_id)
        {
            purchase_order po = this._db.purchase_order.Find(po_id, project_id, domain_id);

            if (po != null && po.quote_file != null && !po.quote_file.Equals(""))
            {
                using (FileStreamRepository fileRepository = new FileStreamRepository())
                {
                    var blob_file = fileRepository.GetBlob(BlobContainersName.Quote(po.domain_id), po.quote_file);

                    if (blob_file.Exists())
                    {
                        string filename = domain_id.ToString() + project_id.ToString() + po_id.ToString() + "Quote.pdf";

                        blob_file.DownloadTo(Path.Combine(this.poDirectory, filename));

                        return Path.Combine(this.poDirectory, filename);
                    }
                }
            }

            return null;
        }

        private project_addresses UpdateAddress(project_addresses address)
        {
            if (address == null)
            {
                address = new project_addresses();
            }

            address.address1 = address.address1 ?? "";
            address.address2 = address.address2 ?? "";
            address.nickname = address.nickname ?? "No addresses have been registered";
            address.city = address.city ?? "";
            address.state = address.state ?? "";
            address.zip = address.zip ?? "";

            return address;
        }

        private string FormatLastAddressLine(project_addresses address)
        {
            string formated_address = "";

            if (address != null)
            {
                if (address.city != null)
                {
                    formated_address += address.city;
                }

                if (address.state != null && !address.state.Equals(""))
                {
                    if (!formated_address.Equals(""))
                    {
                        formated_address += ", ";
                    }

                    formated_address += address.state;
                }

                if (address.zip != null && !address.zip.Equals(""))
                {
                    formated_address += " " + address.zip;
                }
            }

            return formated_address;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}