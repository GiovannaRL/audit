using System;
using System.IO;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;
using OfficeOpenXml;
using System.Data.Entity;
using System.Linq;

namespace reportsWebJob.Services
{
    public class AssetRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public AssetRepository()
        {
            this._db = new audaxwareEntities();
        }

        public AssetRepository(short domain_id)
        {
            this._db = new audaxwareEntities();
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '1';";
                cmd.ExecuteNonQuery();
            }
        }

        public bool ImportAssets(ImportedData data)
        {
            try
            {

            
            using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(data.filePath)))
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];

                int numRows = worksheet.Dimension.End.Row;
                var show_audaxware_info = Helper.ShowAudaxWareInfo(data.domain_id);

                while (numRows > 1)
                {
                    if (worksheet.Cells[numRows, 1].Value != null && !worksheet.Cells[numRows, 1].Value.ToString().Equals(""))
                    {
                        var asset_code = worksheet.Cells[numRows, 1].Value.ToString();
                        asset item = this._db.assets.Include("assets_subcategory.assets_category").Where(a => (a.domain_id == data.domain_id /*|| (show_audaxware_info == true && a.domain_id == 1)*/)
                            && a.asset_code.Equals(asset_code)).FirstOrDefault();
                        if (item != null)
                        {
                            item.asset_suffix = worksheet.Cells[numRows, 5].Value != null ? worksheet.Cells[numRows, 5].Value.ToString() : null;
                            item.comment = worksheet.Cells[numRows, 10].Value != null ? worksheet.Cells[numRows, 10].Value.ToString() : null;
                            item.updated_at = DateTime.Now;
                            item.model_name = worksheet.Cells[numRows, 4].Value != null ? worksheet.Cells[numRows, 4].Value.ToString() : null;
                            item.model_number = worksheet.Cells[numRows, 3].Value != null ? worksheet.Cells[numRows, 3].Value.ToString() : null;

                            if (worksheet.Cells[numRows, 6].Value != null)
                            {
                                var asset_manufacturer = worksheet.Cells[numRows, 6].Value.ToString().ToLower();
                                manufacturer mnf = this._db.manufacturers.Where(m => (m.domain_id == data.domain_id || (show_audaxware_info == true && m.domain_id == 1))
                                    && m.manufacturer_description.ToLower().Equals(asset_manufacturer)).OrderByDescending(x => x.domain_id).FirstOrDefault();
                                if (mnf != null)
                                {
                                    item.manufacturer = mnf;
                                    item.manufacturer_domain_id = mnf.domain_id;
                                    item.manufacturer_id = mnf.manufacturer_id;
                                }
                            }
                            var asset_category = worksheet.Cells[numRows, 7].Value.ToString().ToLower();
                            var category = item.assets_subcategory.assets_category.description;
                            var subcategory = item.assets_subcategory.description;

                            assets_category ctg = this._db.assets_category.Where(ac => (ac.domain_id == data.domain_id || (show_audaxware_info == true && ac.domain_id == 1))
                                && ac.description.ToLower().Equals(asset_category)).OrderByDescending(x => x.domain_id).FirstOrDefault();
                            if (ctg != null)
                            {
                                var asset_subcategory = worksheet.Cells[numRows, 8].Value.ToString().ToLower();

                                assets_subcategory sub_ctg = this._db.assets_subcategory.Where(asc => asc.category_domain_id == ctg.domain_id &&
                                    asc.category_id == ctg.category_id && (asc.domain_id == data.domain_id || (show_audaxware_info && asc.domain_id == 1))
                                    && asc.description.ToLower().Equals(asset_subcategory)).OrderByDescending(x => x.domain_id).FirstOrDefault();
                                if (sub_ctg != null)
                                {
                                    item.assets_subcategory = sub_ctg;
                                    item.subcategory_id = sub_ctg.subcategory_id;
                                    item.subcategory_domain_id = sub_ctg.domain_id;

                                    category = worksheet.Cells[numRows, 7].Value.ToString();
                                    subcategory = worksheet.Cells[numRows, 8].Value.ToString();
                                }
                            }

                            item.asset_description = category;

                            if (category != subcategory)
                                item.asset_description += ", " + subcategory;

                            if (item.asset_suffix != null && item.asset_suffix != "")
                                item.asset_description += ", " + item.asset_suffix;

                            this._db.Entry(item).State = EntityState.Modified;
                        }
                    }
                    numRows--;
                }
                this._db.SaveChanges();
            }

            //delete the file
            if (File.Exists(data.filePath))
                File.Delete(data.filePath);

            //add notification
            var notification = new user_notification();
            notification.domain_id = data.domain_id;
            notification.message = "Assets imported sucessfully";
            notification.userId = data.userId;

            this._db.user_notification.Add(notification);
            this._db.SaveChanges();

            return true;
            }
            catch (Exception e)
            {
                //add notification
                var notification = new user_notification();
                notification.domain_id = data.domain_id;
                notification.message = "Error trying to import assets. " + e.Message;
                notification.userId = data.userId;

                this._db.user_notification.Add(notification);
                this._db.SaveChanges();

                return false;
            }
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
