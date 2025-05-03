using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace xPlannerAPI.Services
{
    public class ExportImportRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public ExportImportRepository()
        {
            _db = new audaxwareEntities();
        }

        public static string ExportCategories(short domainId, IEnumerable<assets_category> categories)
        {
            var timestamp = DateTime.Now;
            var directoryPath = Path.Combine(Domain.GetRoot(), "ExportFiles", "Categories");
            using (var fileRep = new FileStreamRepository())
            {
                fileRep.CreateLocalDirectory(directoryPath);

                var excelPath = Path.Combine(directoryPath, string.Format("Categories_{0}_{1}_{2}.xlsx", domainId, timestamp.ToShortDateString().Replace("/", ""), timestamp.ToShortTimeString().Replace(":", "")));
                var exportFile = new FileInfo(excelPath);
                var template = new FileInfo(Path.Combine(Domain.GetRoot(), "excel_templates", "categories_export.xlsx"));

                using (var xlPackage = new ExcelPackage(exportFile, template))
                {
                    var worksheet = xlPackage.Workbook.Worksheets["Category"];
                    var worksheet1 = xlPackage.Workbook.Worksheets["Subcategory"];

                    var categoryRow = 2;
                    var subcategoryRow = 2;

                    foreach (var category in categories)
                    {
                        worksheet.Cells[categoryRow, 1].Value = category.category_id;
                        worksheet.Cells[categoryRow, 2].Value = category.description;
                        worksheet.Cells[categoryRow, 3].Value = category.asset_code;
                        categoryRow++;

                        foreach (var subcategory in category.assets_subcategory)
                        {
                            worksheet1.Cells[subcategoryRow, 1].Value = category.category_id;
                            worksheet1.Cells[subcategoryRow, 2].Value = subcategory.subcategory_id;
                            worksheet1.Cells[subcategoryRow, 3].Value = category.description;
                            worksheet1.Cells[subcategoryRow, 4].Value = subcategory.description;
                            worksheet1.Cells[subcategoryRow, 5].Value = subcategory.asset_code;
                            subcategoryRow++;
                        }
                    }
                    xlPackage.Save();
                }
                return excelPath;
            }
        }

        public static async Task<bool> ImportCategories(short domainId, string filePath, string userId)
        {
            var item = new ImportedData
            {
                domain_id = domainId,
                filePath = filePath,
                userId = userId
            };

            var webJobRepository = new WebjobRepository<ImportedData>();

            await webJobRepository.SendMessage("import-categories", item);

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}