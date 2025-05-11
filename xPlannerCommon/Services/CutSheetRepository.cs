using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using xPlannerCommon.Models;
using System.Diagnostics;
using Azure.Storage.Blobs.Models;
using xPlannerCommon.App_Data;

namespace xPlannerCommon.Services
{
    public class CutSheetRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;
        private readonly FileStreamRepository _fileRepository;
        private static Dictionary<int, string> _classes = new Dictionary<int, string>() {
            {0, "N/A"},
            {1, "AW"},
            {2, "CC"},
            {3, "CM"},
            {4, "FF"},
            {5, "ME"},
            {6, "RP"},
            {7, "SW"}
        };

        public CutSheetRepository()
        {
            this._db = new audaxwareEntities();
            this._fileRepository = new FileStreamRepository();
        }

        public CutSheetRepository(short domain_id)
        {
            this._db = new audaxwareEntities();
            this._fileRepository = new FileStreamRepository();
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '" + domain_id + "';";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "EXEC sp_set_session_context 'show_audax', '" + Helper.ShowAudaxWareInfo(domain_id) + "';";
                cmd.ExecuteNonQuery();
            }
        }

        public bool BuildFullFromZero(asset item, string containerToSave, string fileName)
        {
            if (containerToSave == null || fileName == null)
                return false;
            string coverPath = CreateCoverSheet(new CutSheetInfo(item), null, null, null, true);
            string coverShopDrawingPath = coverPath.Replace(".pdf", "ShopD.pdf");
            string savedPath = coverPath;
            string savedShopDrawingPath = coverShopDrawingPath;
            var guid = Guid.NewGuid().ToString();
            string cutSheetPath = DownloadCutSheet(new CutSheetInfo(item), string.Format("{0}-{1}.pdf", fileName.Replace(".pdf", ""), guid));

            if (cutSheetPath != null)
            {
                string books = Path.Combine(Domain.GetRoot(), "cut_books");

                if (!Directory.Exists(books))
                    Directory.CreateDirectory(books);

                savedPath = Path.Combine(books, item.asset_id.ToString() + item.domain_id + guid + ".pdf");
                savedShopDrawingPath = Path.Combine(books, item.asset_id.ToString() + item.domain_id + guid + "ShopD.pdf");

                this._fileRepository.MergeFiles(new string[] { coverPath, cutSheetPath }, savedPath, new string[] { item.asset_code + "_coversheet", item.asset_code + "_cutsheet" });
                this._fileRepository.MergeFiles(new string[] { coverShopDrawingPath, cutSheetPath }, savedShopDrawingPath, new string[] { item.asset_code + "_coversheet", item.asset_code + "_cutsheet" });
                this._fileRepository.DeleteFile(coverPath, true);
                this._fileRepository.DeleteFile(coverShopDrawingPath, true);
                this._fileRepository.DeleteFile(cutSheetPath, true);
            }

            var blobClient = _fileRepository.GetBlob(containerToSave, fileName);
            blobClient.Upload(savedPath, overwrite: true);

            blobClient = _fileRepository.GetBlob(containerToSave, fileName.Replace(".pdf", "ShopDrawing.pdf"));
            blobClient.Upload(savedShopDrawingPath, overwrite: true);

            this._fileRepository.DeleteFile(savedPath, true);
            this._fileRepository.DeleteFile(savedShopDrawingPath, true);

            var cutsheetToRegenerate = _db.cutsheet_to_generate.Find(item.asset_id, item.domain_id);
            if (cutsheetToRegenerate != null)
            {
                _db.cutsheet_to_generate.Remove(cutsheetToRegenerate);
                _db.SaveChanges();
            }

            return true;
        }

        private void MergeFilesToFullAndUploadIt(CutSheetInfo item, project_report report, string localPathToSave, string defaultBlobName) {

            int path_ind = 0;

            List<string> files_path = new List<string>();
            List<string> files_title = new List<string>();

            files_path.Add(DownloadCoverSheet(item, null, defaultBlobName));
            if (files_path.Last() == null)
            {
                files_path[path_ind] = CreateCoverSheet(item, null, defaultBlobName, report, true);
            }
            files_title.Add(String.Format("{0}_coversheet", item.asset_code));
            path_ind++;

            string cutsheetPath = DownloadCutSheet(item, defaultBlobName);
            if (cutsheetPath != null)
            {
                files_path.Add(cutsheetPath);
                files_title.Add(String.Format("{0}_cutsheet", item.asset_code));
            }

            this._fileRepository.MergeFiles(files_path.ToArray(), localPathToSave, files_title.ToArray());

            this._fileRepository.UploadToCloud(localPathToSave, BlobContainersName.FullCutsheet(item.asset_domain_id), defaultBlobName);

        }

        public void BuildFullForInventoryItem(CutSheetInfo item, string localPathToSave, project_report report, bool shopDrawing)
        {
            // Try to download the full cutsheet
            if (item.filename != null) {

                if (shopDrawing && !item.filename.Contains("ShopDrawing")) {
                    item.filename = item.filename.Replace(".pdf", "ShopDrawing.pdf");

                } else if (!shopDrawing && item.filename.Contains("ShopDrawing")) {
                    item.filename = item.filename.Replace("ShopDrawing", "");
                }

                var fullCutSheetblob = this._fileRepository.GetBlob(BlobContainersName.FullCutsheet(item.asset_domain_id), item.filename);
                if (fullCutSheetblob.Exists())
                {
                    fullCutSheetblob.DownloadTo(localPathToSave);
                    // Update the cutsheet name for all items
                    _db.update_inventories_cutsheet_filename(item.inventories_id, item.filename);
                    return;
                }
            }

            // Create the full cutsheet
            string defaultCutsheetAndCoversheetName = item.filename ?? string.Format("{0}.pdf", Guid.NewGuid().ToString());

            MergeFilesToFullAndUploadIt(item, report, localPathToSave, defaultCutsheetAndCoversheetName);
            _db.update_inventories_cutsheet_filename(item.inventories_id, defaultCutsheetAndCoversheetName);         
        }

        public void BuildFull(CutSheetInfo item, string localPathToSave, project_report report = null, bool shopDrawing = false)
        {

            if (!string.IsNullOrEmpty(item.inventories_id) && item.has_overwritten_properties) {
                BuildFullForInventoryItem(item, localPathToSave, report, shopDrawing);
                return;
            }

            string blobFilename = string.Format("{0}{1}.pdf", item.asset_id, item.asset_domain_id);
            string defaultCutSheetAndCoverSheetFilename = blobFilename;
            if (shopDrawing)
            {
                blobFilename = string.Format("{0}{1}ShopDrawing.pdf", item.asset_id, item.asset_domain_id);
            }

            CheckAndRegenerateCutsheet(item.asset_id, item.asset_domain_id, BlobContainersName.FullCutsheet(item.asset_domain_id), blobFilename);

            var fullCutSheetblob = this._fileRepository.GetBlob(BlobContainersName.FullCutsheet(item.asset_domain_id), blobFilename);
            if (fullCutSheetblob.Exists()) {
                fullCutSheetblob.DownloadTo(localPathToSave);
            } else {
                MergeFilesToFullAndUploadIt(item, report, localPathToSave, defaultCutSheetAndCoverSheetFilename);
            }
        }

        public void CheckAndRegenerateCutsheet(int assetId, int domainId, string containerToSave, string blobFileName) {

            var cutsheetToRegenerate = _db.cutsheet_to_generate.Find(assetId, domainId);
            if (cutsheetToRegenerate != null)
            {
                var asset =  _db.assets.Include("manufacturer").FirstOrDefault(x => x.asset_id == assetId && x.domain_id == domainId);
                BuildFullFromZero(asset, containerToSave, blobFileName);
            }
        }

        public string DownloadCoverSheet(CutSheetInfo item, string pathToSave, string blobFilename)
        {
            var blob = _fileRepository.GetBlob(BlobContainersName.Coversheet(item.asset_domain_id), blobFilename);

            if (blob.Exists())
            {
                if (string.IsNullOrEmpty(pathToSave))
                {
                    pathToSave = _fileRepository.CreateLocalDirectory(Path.Combine(Domain.GetRoot(), "coversheets",  item.asset_domain_id.ToString())).FullName;
                }

                blob.DownloadTo(Path.Combine(pathToSave, blobFilename));
                return pathToSave;
            }

            return null;
        }

        public string DownloadShopDCoverSheet(CutSheetInfo item, string pathToSave = null, string filename = null)
        {
            var blob = _fileRepository.GetBlob(BlobContainersName.Coversheet(item.asset_domain_id), filename ?? string.Format("{0}{1}ShopDrawing.pdf", item.asset_id, item.asset_domain_id));

            if (blob.Exists())
            {
                if (String.IsNullOrEmpty(pathToSave))
                {
                    _fileRepository.CreateLocalDirectory(Path.Combine(Domain.GetRoot(), "coversheets"));
                    pathToSave = Path.Combine(Domain.GetRoot(), "coversheets", filename ?? string.Format("{0}{1}ShopD.pdf", item.asset_id, item.asset_domain_id));
                }

                blob.DownloadTo(pathToSave);
                return pathToSave;
            }

            return null;
        }

        public string CreateCoverSheet(CutSheetInfo item, string pathToSave, string filename = null, project_report report = null, bool shopDrawing = false)
        {
            string rootPath = Domain.GetRoot();
            string path = Path.Combine(rootPath, "coversheets");
            string photo_dir = Path.Combine(rootPath, "photos");

            var guid = Guid.NewGuid().ToString();
            string fileName = filename ?? string.Format("{0}{1}{2}.pdf", item.asset_id, item.asset_domain_id, guid);
            string assetCode = item.asset_code + (string.IsNullOrEmpty(item.JSN) ? "" : " / " + item.JSN);

            if (item.manufacturer_description == null)
            {
                item.manufacturer_description = _db.manufacturers.Where(x => x.manufacturer_id == item.manufacturer_id).FirstOrDefault().manufacturer_description;
            };

            string completePath = Path.Combine(path, fileName), pathToSaveShop = pathToSave;
            var asset_options = _db.assets_options.Where(x => x.asset_id == item.asset_id && x.domain_id == item.asset_domain_id && x.project_id == null).OrderBy(x => x.data_type).ThenBy(y => y.description);
            Dictionary<string, string> option_data_types = new Dictionary<string, string>();
            option_data_types.Add("A", "Accessories");
            option_data_types.Add("C", "Colors");
            option_data_types.Add("CO", "Consumables");
            option_data_types.Add("D", "Discounts");
            option_data_types.Add("I", "Installations");
            option_data_types.Add("W", "Warranties");
            option_data_types.Add("FI", "Finish");
            option_data_types.Add("FR", "Frame");
            Dictionary<string, string> category_attribute_types = new Dictionary<string, string>();
            category_attribute_types.Add("F", "Fixed");
            category_attribute_types.Add("MJ", "Major Moveable");
            category_attribute_types.Add("MN", "Minor Moveable");

            AssetSettingsRepository rep = new AssetSettingsRepository(1);
            var attributes = rep.GetSettings(item.asset_domain_id, item.asset_id, true);

            if (item.inventories_id != null && item.has_overwritten_attributes)
            {
                int inventoryId = int.Parse(item.inventories_id.Split(';').FirstOrDefault());
                var inventoryAttributes = rep.GetInventorySettings(report.project_domain_id, inventoryId);

                foreach (var inventory in inventoryAttributes)
                {
                    foreach (var asset in attributes)
                    {
                        if (inventory.asset_field == asset.asset_field)
                            asset.value = inventory.value;
                    }
                }
            }


            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            StringBuilder beforeShopDrawingReference = new StringBuilder();
            beforeShopDrawingReference.Append("<html><head><meta charset='UTF-8'>");
            beforeShopDrawingReference.Append("<link type='text/css' href='" + Path.Combine(Domain.GetRoot(), "css", "CoverSheet.css") + "' rel='stylesheet' />");
            beforeShopDrawingReference.Append("</head><body>");
            beforeShopDrawingReference.Append("<table width='100%'>");

            StringBuilder afterShopDrawingReference = new StringBuilder();
            afterShopDrawingReference.Append("<tr>");
            afterShopDrawingReference.Append("<td><div style='color:#0787D5;font-weight:bold' class='test'>" + item.asset_description + "</div></td>");
            afterShopDrawingReference.Append("<td rowspan='2' align='right' width='260px' valign='top'>");
            afterShopDrawingReference.Append("<div style='font-size: 22px'><strong>" + assetCode + "</strong></div>");
            if (item.cad_id != item.asset_code && item.inventories_id != null)            
                afterShopDrawingReference.Append("<div style='color:#3b3b3b;font-size: 18px'><strong>" + " &nbsp;CAD ID: " + item.cad_id + "</strong></div>");            

            afterShopDrawingReference.Append("</td>");
            afterShopDrawingReference.Append("</tr>");
            afterShopDrawingReference.Append("<tr height='22px'>");
            afterShopDrawingReference.Append("<td>Manufacturer: " + item.manufacturer_description + " &nbsp;Model: " + item.serial_number + " &nbsp;|&nbsp; " + item.serial_name + "</td>");
            afterShopDrawingReference.Append("</tr>");
            afterShopDrawingReference.Append("<tr height='22px'>");
            afterShopDrawingReference.Append("<td>&nbsp;</td>");
            if (item.photo != null && item.photo != "")
            {
                //DOWNLOAD PHOTO
                var blob_photo = _fileRepository.GetBlob(BlobContainersName.Photo(item.photo_domain_id), item.photo);

                var photo_path = _fileRepository.GetBlobSasUri(blob_photo);

                var rotation = item.photo_rotate * 90;         

                if (!Directory.Exists(photo_dir))
                    Directory.CreateDirectory(photo_dir);

                photo_dir = Path.Combine(photo_dir, guid + item.photo);

                _fileRepository.DeleteFile(photo_dir);

                if (blob_photo.Exists())
                {
                    blob_photo.DownloadTo(photo_dir);

                    afterShopDrawingReference.Append("<td rowspan='4' align='right'><img src='" + photo_dir + "' style='-webkit-transform:rotate(" + rotation + "deg)' BorderStyle='None' AlternateText='No Picture' Height='250' Width='250' /></td>");
                }
                else
                {
                    afterShopDrawingReference.Append("<td rowspan='4' align='right'><img src=''  BorderStyle='None' AlternateText='No Picture' Height='250' Width='250' /></td>");
                }
            }
            else
            {
                afterShopDrawingReference.Append("<td rowspan='4' align='right'><img src='' BorderStyle='None' AlternateText='No Picture' Height='250' Width='250' /></td>");
            }

            afterShopDrawingReference.Append("</tr>");
            afterShopDrawingReference.Append("<tr>");
            afterShopDrawingReference.Append("<td valign='top'>" + item.comment + "</td>");
            afterShopDrawingReference.Append("</tr>");
            if (report?.project_id != null)
            {
                afterShopDrawingReference.Append("<tr>");
                var project_qty = _db.project_room_inventory.Where(x => x.asset_id == item.asset_id &&
                    x.asset_domain_id == item.asset_domain_id && x.domain_id == report.project_domain_id && x.project_id == report.project_id &&
                    (report.phase_id == null || (x.phase_id == report.phase_id && (report.department_id == null || (x.department_id ==
                    report.department_id && (report.room_id == null || x.room_id == report.room_id)))))).Sum(x => x.budget_qty);
                //afterShopDrawingReference.Append("<td valign='top'>" + (report.room_id != null ? "Room" : report.department_id != null ? "Department" : report.phase_id != null ? "Phase" : "Project") + " Quantity: " + project_qty + "</td>");
                afterShopDrawingReference.Append("</tr>");
            }

            afterShopDrawingReference.Append("<tr>");
            afterShopDrawingReference.Append("<table width='100%'>");
            afterShopDrawingReference.Append("<td width='45%' valign='top'>");
            afterShopDrawingReference.Append("<table width='100%' class='coversheet_table'>");
            afterShopDrawingReference.Append("<tr class='coversheet_title'>");
            afterShopDrawingReference.Append("<td colspan='2'>Asset Attributes</td>");
            afterShopDrawingReference.Append("</tr>");

            var group_name = "";
            var itemsPerGroup = 0;
            var totalAttributes = 0;
            Dictionary<string, string> option_attributes = new Dictionary<string, string>() { { "", "--" }, { "0", "--" }, { "1", "Yes" }, { "2", "Optional" } };
            Dictionary<string, string> hertz_attributes = new Dictionary<string, string>() { { "", "--" }, { "50", "50" }, { "50/60", "50/60" }, { "60", "60" } };
            Dictionary<string, string> checkbox_attributes = new Dictionary<string, string>() { { "", "--" }, { "false", "--" }, { "true", "Yes" } };
            Dictionary<string, string> mobile_attributes = new Dictionary<string, string>() { { "", "--" }, { "0", "--" }, { "1", "Yes" }, { "2", "Optional" } };

            foreach (var attribute in attributes)
            {
                totalAttributes += 1;
                if (group_name != attribute.group_name)
                {
                    if (itemsPerGroup == 0 && totalAttributes > 1)
                    {
                        afterShopDrawingReference.Append("<tr>");
                        afterShopDrawingReference.Append("<td align='center' colspan='2'>No " + group_name + " required</td>");
                        afterShopDrawingReference.Append("</tr>");
                    }
                    afterShopDrawingReference.Append("<tr class='coversheet_subtitle'><td class='cover_padding' colspan='2'>" + attribute.group_name + "</td></tr>");
                    group_name = attribute.group_name;
                    itemsPerGroup = 0;
                }

                var dataValue = "";
                if (PassValidationRequired(attribute, option_attributes, checkbox_attributes, hertz_attributes, category_attribute_types, mobile_attributes, out dataValue))
                {
                    itemsPerGroup += 1;
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td align='left'>" + attribute.property_name + "</td>");
                    afterShopDrawingReference.Append("<td width='25%' align='right'>" + dataValue + "</td>");
                }


                afterShopDrawingReference.Append("</tr>");


                //CHECK THE LAST ITEM
                if (itemsPerGroup == 0 && attributes.Count() == totalAttributes)
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td align='center' colspan='2'>No " + group_name + " required</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                

            }
            afterShopDrawingReference.Append("</table>");
            afterShopDrawingReference.Append("</td>");
            afterShopDrawingReference.Append("<td width='3%'></td>");
            afterShopDrawingReference.Append("<td style='vertical-align: top'>");
            afterShopDrawingReference.Append("<table width='100%' class='coversheet_table' cellpadding='2'>");
           
            if (asset_options.Count() > 0)
            {
                afterShopDrawingReference.Append("<tr class='coversheet_title'>");
                afterShopDrawingReference.Append("<td colspan='2' class='cover_padding'>Asset Options</td>");
                afterShopDrawingReference.Append("</tr>");
                var type = "";

                foreach (var option in asset_options)
                {
                    if (type != option.data_type)
                    {
                        type = option.data_type;
                        afterShopDrawingReference.Append("<tr class='coversheet_subtitle'><td colspan='2'>" + option_data_types[option.data_type] + "</td></tr>");
                    }
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>" + option.code + "</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + option.description + "</td>");
                    //afterShopDrawingReference.Append("<td width='15%' align='right'>" + option.unit_budget + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }

                afterShopDrawingReference.Append("<tr>");
                afterShopDrawingReference.Append("<td style='padding-top:60px' colspan='2'>&nbsp;</td>");
                afterShopDrawingReference.Append("</tr>");
            }

            if (item.HasAnyGovernmentInformation())
            {
                var class_desc = "";
                if (_classes.ContainsKey(item.@class.GetValueOrDefault()))
                {
                    class_desc = _classes[item.@class.GetValueOrDefault()];
                }

                afterShopDrawingReference.Append("<tr class='coversheet_title'>");
                afterShopDrawingReference.Append("<td colspan='2' class='cover_padding'>Government</td>");
                afterShopDrawingReference.Append("</tr>");
                
                if (!string.IsNullOrEmpty(item.JSN))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>JSN</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.JSN + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (checkJsnUtility(item.jsn_utility1))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>JSN utility 1</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.jsn_utility1 + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (checkJsnUtility(item.jsn_utility2))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>JSN utility 2</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.jsn_utility2 + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (checkJsnUtility(item.jsn_utility3))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>JSN utility 3</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.jsn_utility3 + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (checkJsnUtility(item.jsn_utility4))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>JSN utility 4</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.jsn_utility4 + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (checkJsnUtility(item.jsn_utility5))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>JSN utility 5</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.jsn_utility5 + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (checkJsnUtility(item.jsn_utility6))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>JSN utility 6</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.jsn_utility6 + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (!string.IsNullOrEmpty(item.placement))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>Placement</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + item.placement + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                if (!string.IsNullOrEmpty(class_desc))
                {
                    afterShopDrawingReference.Append("<tr>");
                    afterShopDrawingReference.Append("<td width='25%'>Class</td>");
                    afterShopDrawingReference.Append("<td width='60%'>" + class_desc + "</td>");
                    afterShopDrawingReference.Append("</tr>");
                }
                afterShopDrawingReference.Append("<tr>");
                afterShopDrawingReference.Append("<td width='25%'>Log Cat (responsibility)</td>");
                afterShopDrawingReference.Append("<td width='60%'>" + item.resp + "</td>");
                afterShopDrawingReference.Append("</tr>");
            }
            
            afterShopDrawingReference.Append("</table>");
            afterShopDrawingReference.Append("</td>");
            afterShopDrawingReference.Append("</tr>");
            afterShopDrawingReference.Append("</table>");
            afterShopDrawingReference.Append("</table>");
            afterShopDrawingReference.Append("</body></html>");

            StringWriter stringWriter = new StringWriter();
            stringWriter.Write(beforeShopDrawingReference);
            stringWriter.Write(afterShopDrawingReference);
            stringWriter.Close();


            if (pathToSave == null)
                pathToSave = completePath;

            if (shopDrawing)
            {
                pathToSaveShop = pathToSave.Replace(".pdf", "ShopD.pdf");
            }

            string originPath = this._fileRepository.SaveDocumentTemporarily(stringWriter.ToString(), pathToSave.Replace("pdf", "html"));
            WkhtmltopdfRepository.ConvertToPDF(originPath, pathToSave);

            _fileRepository.UploadToCloud(pathToSave, BlobContainersName.Coversheet(item.asset_domain_id), fileName);

            if (shopDrawing)
            {
                StringWriter stringWriterShop = new StringWriter(beforeShopDrawingReference);
                if (shopDrawing)
                {
                    stringWriterShop.Write("<tr>");
                    stringWriterShop.Write("<td></td>");
                    stringWriterShop.Write("<td align='right' valign='top' width=\"500px\">");
                    stringWriterShop.Write("<div style='font-size: 22px; color: red;'><strong>REFERENCE SHOP DRAWING</strong></div>");
                    stringWriterShop.Write("</td>");
                    stringWriterShop.Write("</tr>");
                }
                stringWriterShop.Write(afterShopDrawingReference);
                stringWriterShop.Close();

                originPath = this._fileRepository.SaveDocumentTemporarily(stringWriterShop.ToString(), pathToSaveShop.Replace("pdf", "html"));
                WkhtmltopdfRepository.ConvertToPDF(originPath, pathToSaveShop);

                _fileRepository.UploadToCloud(pathToSaveShop, BlobContainersName.Coversheet(item.asset_domain_id), fileName.Replace(".pdf", "ShopDrawing.pdf"));
            }

            _fileRepository.DeleteFile(photo_dir, true);

            return pathToSave;
        }

        private bool PassValidationRequired(AssetSettingsStructure attribute, Dictionary<string, string> option_attributes, Dictionary<string, string> checkbox_attributes, Dictionary<string, string> hertz_attributes, Dictionary<string, string> category_attribute_types, Dictionary<string, string> mobile_attributes, out string dataValue) {
            dataValue = "";
            if (attribute.value != null && attribute.value.Trim() != "" && attribute.value != "false" && attribute.value != "--") {
                if ((attribute.property_name.ToLower().Contains("required") || attribute.property_name == "lan") && attribute.property_name.ToLower() != "biomed check required")
                {
                    dataValue = attribute.value != null ? option_attributes[attribute.value] : "--";
                    if (dataValue == "--")
                        return false;
                }
                else if (attribute.editor_type.Equals("checkbox"))
                {
                    dataValue = attribute.value != null ? checkbox_attributes[attribute.value.ToLower()] : "--";
                    if (dataValue == "--")
                        return false;
                }
                else
                {
                    switch (attribute.property_name)
                    {
                        case "Hertz":
                            dataValue = attribute.value != null ? hertz_attributes[attribute.value] : "--";
                            if (dataValue == "--")
                                return false;
                            break;
                        case "Category":
                            dataValue = attribute.value != null ? category_attribute_types[attribute.value] : "--";
                            if (dataValue == "--")
                                return false;
                            break;
                        case "Mobile":
                            dataValue = attribute.value != null ? mobile_attributes[attribute.value] : "--";
                            if (dataValue == "--")
                                return false;
                            break;
                        default:
                            dataValue = attribute.value;
                            break;
                    }
                }
                return true;
            }
            return false;
            
        }

        void NotifyAccessError(CutSheetInfo cutsheetInfo)
        {
            asset item = _db.assets.Find(cutsheetInfo.asset_id, cutsheetInfo.asset_domain_id);

            //add notification
            var notification = new user_notification();
            notification.domain_id = item.domain_id;
            notification.message = "Error trying to access asset file " + item.asset_code + ". Please try again.";
            notification.userId = this._db.AspNetUsers.Where(x => x.Email == item.added_by).FirstOrDefault().ToString();
            this._db.user_notification.Add(notification);
            this._db.SaveChanges();
        }

        private bool checkJsnUtility(string utility)
        {
            if (utility == null || utility.ToLower() == "n/a" || utility.Length == 0)
            {
                return false;
            }

            return true;
        } 


        private string DownloadCutSheet(CutSheetInfo item, string filename)
        {
            if (!string.IsNullOrEmpty(item.cut_sheet))
            {
                string directory_cutsheet = Path.Combine(Domain.GetRoot(), "cutsheets");
                string directory_cutsheet_converted = Path.Combine(Domain.GetRoot(), "cutsheets", "converted");
                string container_cutsheet = BlobContainersName.Cutsheet(item.asset_domain_id);
                string file = filename ?? item.asset_id.ToString() + item.asset_domain_id + ".pdf";
                string container_cutsheet_converted = Path.Combine(directory_cutsheet_converted, file);

                _fileRepository.CreateLocalDirectory(directory_cutsheet);

                //The program to join files is always creating data inside this converted folder and them ignoring the updates.
                if (Directory.Exists(directory_cutsheet_converted) && File.Exists(container_cutsheet_converted))
                    File.Delete(container_cutsheet_converted);


                //DOWNLOAD CUTSHEET
                var blob_cutsheet = _fileRepository.GetBlob(container_cutsheet, item.cut_sheet);

                if (blob_cutsheet.Exists())
                {
                    string path_cutsheet = Path.Combine(directory_cutsheet, file);
                    // Deletes the file in case it already exists
                    this._fileRepository.DeleteFile(path_cutsheet);

                    try
                    {
                        blob_cutsheet.DownloadTo(path_cutsheet);

                        if (!(new FileInfo(path_cutsheet)).Exists)
                        {
                            NotifyAccessError(item);
                            return null;
                        }

                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Error to download cutsheet {0} - Exception: {1}", path_cutsheet, ex);
                        NotifyAccessError(item);
                        return null;
                    }

                    return path_cutsheet;
                }
            }

            return null;
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