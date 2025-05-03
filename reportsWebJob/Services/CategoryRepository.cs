using System;
using OfficeOpenXml;
using System.Data.Entity;
using System.Linq;
using xPlannerCommon.Models;
using System.IO;
using xPlannerCommon.App_Data;
using System.Text;

namespace reportsWebJob.Services
{
    class CategoryRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public CategoryRepository()
        {
            this._db = new audaxwareEntities();
        }

        public CategoryRepository(short domain_id)
        {
            this._db = new audaxwareEntities();
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '1';";
                cmd.ExecuteNonQuery();
            }
        }

        public bool ImportCategories(ImportedData data)
        {
            try
            {

                var attributes = new string[] { "HVAC", "Plumbing", "Gases", "IT", "Electrical", "Support", "Physical", "Environmental" };

                using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(data.filePath)))
                {
                    ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Category"];

                    int numRows = worksheet.Dimension.End.Row;
                    int category_id, subcategory_id;
                    string category_description, subcategory_description;
                    StringBuilder sqlInsertCommand = new StringBuilder();

                    while (numRows > 1)
                    {
                        category_id = Convert.ToInt32(worksheet.Cells[numRows, 1].Value);
                        if (worksheet.Cells[numRows, 2].Value != null)
                        {
                            category_description = worksheet.Cells[numRows, 2].Value.ToString().Replace("'", "''");

                            if (category_id != 0)
                            {
                                assets_category cat = this._db.assets_category.Where(c => c.category_id == category_id).FirstOrDefault();
                                if (cat != null)
                                {
                                    cat.description = category_description;
                                    if (worksheet.Cells[numRows, 3].Value != null && !worksheet.Cells[numRows, 3].Value.ToString().Equals(cat.asset_code) && this._db.assets_codes.Find(worksheet.Cells[numRows, 3].Value.ToString(), 1) != null)
                                    {
                                        cat.asset_code_domain_id = 1;
                                        cat.asset_code = worksheet.Cells[numRows, 3].Value.ToString();
                                    }
                                    this._db.Entry(cat).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                if (worksheet.Cells[numRows, 3].Value != null && this._db.assets_codes.Find(worksheet.Cells[numRows, 3].Value.ToString(), 1) != null)
                                {
                                    sqlInsertCommand.Append(string.Format("INSERT INTO assets_category(domain_id, description, asset_code_domain_id, asset_code) VALUES(1, '{0}', 1, '{1}');", category_description, worksheet.Cells[numRows, 3].Value));
                                }
                                else
                                {
                                    sqlInsertCommand.Append(string.Format("INSERT INTO assets_category(domain_id, description) VALUES(1, '{0}');", category_description));
                                }
                            }
                        }
                        numRows--;
                    }

                    if (sqlInsertCommand.Length > 0)
                    {
                        this._db.Database.ExecuteSqlCommand(sqlInsertCommand.ToString());
                    }
                    this._db.SaveChanges();

                    worksheet = xlPackage.Workbook.Worksheets["Subcategory"];
                    numRows = worksheet.Dimension.End.Row;
                    sqlInsertCommand.Clear();

                    while (numRows > 1)
                    {
                        category_id = Convert.ToInt32(worksheet.Cells[numRows, 1].Value);
                        subcategory_id = Convert.ToInt32(worksheet.Cells[numRows, 2].Value);

                        if (worksheet.Cells[numRows, 4].Value != null)
                        {
                            //category_description = worksheet.Cells[numRows, 3].Value.ToString();
                            subcategory_description = worksheet.Cells[numRows, 4].Value.ToString().Replace("'", "''");

                            if (subcategory_id != 0)
                            {
                                assets_subcategory sub = this._db.assets_subcategory.Where(s => s.category_id == category_id
                                    && s.subcategory_id == subcategory_id).FirstOrDefault();

                                if (sub != null)
                                {
                                    sub.description = subcategory_description;
                                    if (worksheet.Cells[numRows, 5].Value != null && !worksheet.Cells[numRows, 5].Value.ToString().Equals(sub.asset_code) && this._db.assets_codes.Find(worksheet.Cells[numRows, 5].Value.ToString(), 1) != null)
                                    {
                                        sub.asset_code_domain_id = 1;
                                        sub.asset_code = worksheet.Cells[numRows, 5].Value.ToString();
                                    }
                                    this._db.Entry(sub).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                if (worksheet.Cells[numRows, 5].Value != null && this._db.assets_codes.Find(worksheet.Cells[numRows, 5].Value.ToString(), 1) != null)
                                {
                                    sqlInsertCommand.Append(string.Format("INSERT INTO assets_subcategory(category_domain_id, category_id, domain_id, description, asset_code_domain_id, asset_code) VALUES(1, {0}, 1, '{1}', 1, '{2}');", category_id, subcategory_description, worksheet.Cells[numRows, 5].Value.ToString()));
                                }
                                else
                                {
                                    sqlInsertCommand.Append(string.Format("INSERT INTO assets_subcategory(category_domain_id, category_id, domain_id, description) VALUES(1, {0}, 1, '{1}');", category_id, subcategory_description));
                                }
                            }
                        }
                        numRows--;
                    }

                    if (sqlInsertCommand.Length > 0)
                    {
                        this._db.Database.ExecuteSqlCommand(sqlInsertCommand.ToString());
                    }
                    this._db.SaveChanges();
                }

                //delete the file
                if (File.Exists(data.filePath))
                    File.Delete(data.filePath);

                //add notification
                var notification = new user_notification();
                notification.domain_id = data.domain_id;
                notification.message = "Categories imported sucessfully";
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
                notification.message = "Error trying to import categories. " + e.Message;
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
