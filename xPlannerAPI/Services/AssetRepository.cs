using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Models;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace xPlannerAPI.Services
{
    public class AssetRepository : IAssetRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public AssetRepository()
        {
            _db = new audaxwareEntities();
        }

        public string GetMaxCode(int domainId, string assetCode)
        {
            var assets = _db.assets.Where(a => a.asset_code.Substring(0, 3).ToUpper().Equals(assetCode.ToUpper()) && a.domain_id == domainId && !a.asset_code.EndsWith("C")).Select(x => x.asset_code);
            return assets.Max();
        }

        public IEnumerable<asset> GetJacks(int domainId)
        {
            return _db.assets.Where(a => a.domain_id == domainId && a.approval_pending_domain == null && a.asset_code.Substring(0, 3).Equals("JCK")).ToList();
        }

        public void DuplicateFiles(asset oldAsset, asset newAsset)
        {
            using (var fileRep = new FileStreamRepository())
            {
                if (oldAsset.photo != null)
                {
                    fileRep.copyBlob(BlobContainersName.Photo(oldAsset.domain_id), oldAsset.photo, BlobContainersName.Photo(newAsset.domain_id), newAsset.photo);
                }
                if (oldAsset.cad_block != null)
                {
                    fileRep.copyBlob(BlobContainersName.Cadblock(oldAsset.domain_id), oldAsset.cad_block, BlobContainersName.Cadblock(newAsset.domain_id), newAsset.cad_block);
                }
                if (oldAsset.revit != null)
                {
                    fileRep.copyBlob(BlobContainersName.Revit(oldAsset.domain_id), oldAsset.revit, BlobContainersName.Revit(newAsset.domain_id), newAsset.revit);
                }
                if (oldAsset.cut_sheet != null)
                {
                    fileRep.copyBlob(BlobContainersName.Cutsheet(oldAsset.domain_id), oldAsset.cut_sheet, BlobContainersName.Cutsheet(newAsset.domain_id), newAsset.cut_sheet);
                }
            }
        }

        public asset DuplicateAssetWithAWApproval(asset oldAsset, int newDomainId, bool changeInventories, string addedBy, bool linkDuplicated, bool modifyAWAsset)
        {
            return this.DuplicateAsset(oldAsset, newDomainId, changeInventories, addedBy, linkDuplicated, true, modifyAWAsset);
        }

        public asset DuplicateAsset(int currentAssetId, int newDomainId, bool changeInventories, string addedBy)
        {
            return _db.duplicate_asset_ex((short)newDomainId, currentAssetId, changeInventories, addedBy);
        }

        public asset DuplicateAsset(asset oldAsset, int newDomainId, bool changeInventories, string addedBy, bool linkDuplicated, bool needApproval = false, bool modifyAWAsset = false)
        {
            var newAsset = _db.duplicate_asset_new_code(oldAsset.domain_id, oldAsset.asset_id, (short)newDomainId,
                GetNextCode(newDomainId, oldAsset.asset_code.Substring(0, 3)), changeInventories, addedBy, linkDuplicated, needApproval, modifyAWAsset).FirstOrDefault();
            DuplicateFiles(oldAsset, newAsset);
            return newAsset;
        }

        public string GetNextCode(int domainId, string assetCode)
        {
            var lastAssetCode = GetMaxCode(domainId, assetCode);
            string nextCode;
            if (lastAssetCode != null)
            {
                int aux;
                int.TryParse(lastAssetCode.Substring(3, lastAssetCode.Length - 3), out aux);
                nextCode = (aux + 1).ToString("D" + 5);
            }
            else
            {
                nextCode = domainId == 1 ? "00001" : "10000";
            }
            return assetCode + nextCode;
        }

        public List<ReducedAsset> GetNotDiscontinued(int currentDomainId, int currentAssetId)
        {
            return _db.assets.Where(a => a.discontinued != true && a.domain_id == currentDomainId && a.asset_id != currentAssetId).Select(a => new ReducedAsset { asset_id = a.asset_id, asset_code = a.asset_code }).ToList();
        }

        public async Task CreateCutSheet(asset item)
        {
            var webJobRepository = new WebjobRepository<asset>();

            await webJobRepository.SendMessage("create-cut-sheet", item);
        }

        public bool UpdateAssetSimple(asset item)
        {
            if (item == null)
                return true;

            _db.Entry(item).State = EntityState.Modified;
            return _db.SaveChanges() > 0;
        }

        public bool DeleteRelatedAsset(asset item)
        {
            if (item == null)
                return true;

            try
            {
                _db.delete_asset_and_related_asset(item.asset_id, item.domain_id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public get_related_assets_Result UpdateAudaxWareAsset(asset item)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var ignored_columns = new string[] { "asset_id", "domain_id", "domain", "domain1", "assets_category", "asset_code" };
                    var properties = new string[] { typeof(int).Name, typeof(int?).Name, typeof(string).Name, typeof(decimal).Name, typeof(decimal?).Name, typeof(bool).Name, typeof(bool?).Name, typeof(DateTime).Name, typeof(DateTime?).Name, typeof(short).Name, typeof(short?).Name };
                    get_related_assets_Result related_asset = _db.get_related_assets(item.asset_id, item.domain_id).FirstOrDefault();
                    var asset = _db.assets.Include("assets_options").Include("assets_vendor").Where(x => x.asset_id == related_asset.asset_id && x.domain_id == related_asset.domain_id).FirstOrDefault();
                    Type t = item.GetType();
                    PropertyInfo[] props = t.GetProperties();

                    foreach (PropertyInfo prp in props)
                    {
                        if (!ignored_columns.Contains(prp.Name) && properties.Contains(prp.PropertyType.Name))
                        {
                            prp.SetValue(asset, prp.GetValue(item));
                        }
                    }

                    _db.SaveChanges();


                    _db.assets_options.RemoveRange(asset.assets_options);
                    _db.SaveChanges();

                    var options = _db.assets_options.Where(x => x.asset_id == item.asset_id && x.domain_id == item.domain_id).ToList();
                    foreach (var option in options)
                    {
                        option.asset_id = asset.asset_id;
                        option.asset_domain_id = asset.domain_id;
                        _db.Entry(option).State = EntityState.Modified;
                    }

                    _db.SaveChanges();

                    _db.assets_vendor.RemoveRange(asset.assets_vendor);
                    _db.SaveChanges();

                    var vendors = _db.assets_vendor.Where(x => x.asset_id == item.asset_id && x.asset_domain_id == item.domain_id).ToList();

                    Type typeVendor = (new assets_vendor()).GetType();
                    props = typeVendor.GetProperties();

                    
                    foreach (var vendor in vendors)
                    {
                        var new_vendor = new assets_vendor();
                        new_vendor.asset_id = asset.asset_id;
                        new_vendor.asset_domain_id = asset.domain_id;
                        foreach (PropertyInfo prp in props)
                        {
                            if (prp.Name != "asset_id" && prp.Name != "asset_domain_id")
                            {
                                prp.SetValue(new_vendor, prp.GetValue(vendor));
                            }

                        }
                        _db.assets_vendor.Add(new_vendor);
                        _db.SaveChanges();
                    }

                    transaction.Commit();

                    return related_asset;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Trace.TraceError($"Error on AssetRepository:UpdateAudaxWareAsset. ErrorMessage: {e.Message}. InnerException: {e.InnerException}");
                    return new get_related_assets_Result();
                }
            }
        }

        public async Task<bool> ImportData(short domainId, string filePath, string userId)
        {
            var item = new ImportedData
            {
                domain_id = domainId,
                filePath = filePath,
                userId = userId
            };

            var webJobRepository = new WebjobRepository<ImportedData>();

            await webJobRepository.SendMessage("import-assets", item);

            return true;
        }

        private static bool IsDifferentBlobFiles(BlobDownload file1, BlobDownload file2)
        {
            if (file1 != null && file2 != null)
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(file1.BlobStream.ToArray());
                    var hashFile1 = BitConverter.ToString(hash).Replace("_", "").ToLower();

                    var hash2 = md5.ComputeHash(file2.BlobStream.ToArray());
                    var hashFile2 = BitConverter.ToString(hash2).Replace("_", "").ToLower();

                    return !hashFile1.Equals(hashFile2);
                }
            }

            // Se os dois não são nulls então são diferentes
            return file1 != null || file2 != null;
        }

        private static List<AssetsDifferencesStructure> GetDifferencesList(asset_summarized customized, asset_summarized original)
        {
            var result = new List<AssetsDifferencesStructure>();

            if (customized != null && original != null)
            {
                var notIncludedProperties = new[] { "updated_at", "owner_name" };
                var fileFields = new[] { "cut_sheet", "cad_block", "revit", "photo" };

                foreach (var prop in customized.GetType().GetProperties())
                {
                    if (!notIncludedProperties.Any(a => a.Equals(prop.Name)) && !prop.Name.Contains("_id"))
                    {
                        string customizedValue;
                        string originalValue;
                        if (fileFields.Contains(prop.Name))
                        {
                            customizedValue = prop.GetValue(customized, null) != null ? prop.GetValue(customized, null).ToString() : null;
                            originalValue = prop.GetValue(original, null) != null ? prop.GetValue(original, null).ToString() : null;

                            if (prop.GetValue(customized, null) != null || prop.GetValue(original, null) != null)
                            {
                                if (customizedValue != null && originalValue != null)
                                {
                                    using (var rep = new FileStreamRepository())
                                    {
                                        if (IsDifferentBlobFiles(rep.DownloadBlobFile(
                                                $"{prop.Name.Replace("_", "")}{customized.domain_id}", customizedValue),
                                                rep.DownloadBlobFile($"{prop.Name.Replace("_", "")}1", originalValue)))
                                            result.Add(new AssetsDifferencesStructure(prop.Name, customizedValue, originalValue));
                                    }
                                }
                                else
                                {
                                    result.Add(new AssetsDifferencesStructure(prop.Name, customizedValue, originalValue));
                                }
                            }
                        }
                        else
                        {
                            customizedValue = prop.GetValue(customized, null) != null ? prop.GetValue(customized, null).ToString() : prop.PropertyType == typeof(bool?) ? "False" : "";
                            originalValue = prop.GetValue(original, null) != null ? prop.GetValue(original, null).ToString() : prop.PropertyType == typeof(bool?) ? "False" : "";

                            if (!originalValue.Equals(customizedValue))
                            {
                                result.Add(new AssetsDifferencesStructure(prop.Name, customizedValue, originalValue));
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<AssetsDifferencesStructure> GetCustomizedDifferences(int customizedDomainId, int customizedAssetId)
        {
            using (var repo = new TableRepository<asset>())
            {
                var customizedAsset = repo.Get(new[] { "domain_id", "asset_id" }, new[] { customizedDomainId, customizedAssetId }, new[] { "assets" });
                if (customizedAsset?.assets == null || !customizedAsset.assets.Any())
                    return null;

                using (var repository = new TableRepository<asset_summarized>())
                {
                    var customized = repository.Get(new[] { "domain_id", "asset_id" }, new[] { customizedAsset.domain_id, customizedAsset.asset_id }, null);
                    var original = repository.Get(new[] { "domain_id", "asset_id" }, new[] { customizedAsset.assets.First().domain_id, customizedAsset.assets.First().asset_id }, null);

                    return GetDifferencesList(customized, original);
                }
            }
        }

        public IEnumerable<ProjectQty> GetProjects(int domainId, int assetDomainId, int assetId)
        {

            var projects = _db.project_room_inventory.Include("project_room.project_department.project_phase.project").Where(x => x.asset_id == assetId && x.asset_domain_id == assetDomainId && x.domain_id == domainId && x.project_room.project_department.project_phase.project.status != "R")
                .GroupBy(g => g.project_id)
                .Select(p => new ProjectQty
                {
                    ProjectId = p.FirstOrDefault().project_id,
                    ProjectDescription = p.FirstOrDefault().project_room.project_department.project_phase.project.project_description,
                    Quantity = (int)p.Sum(q => q.budget_qty)
                }).OrderBy(x => x.ProjectDescription).ToList();
            return projects;
        }

        private Expression<Func<project_room_inventory, bool>> IsSameAssetAndOverwritableFieldsAreDifferent(asset originalAsset)
        {
            return inventoryItem => inventoryItem.asset_domain_id == originalAsset.domain_id && inventoryItem.asset_id == originalAsset.asset_id
                && (originalAsset.placement != inventoryItem.placement
                || originalAsset.@class != inventoryItem.@class
                || (inventoryItem.asset_description_ow == true && !inventoryItem.asset_description.Equals(originalAsset.asset_description))
                || inventoryItem.manufacturer_description_ow == true
                || (inventoryItem.model_number_ow == true && !inventoryItem.model_number.Equals(originalAsset.model_number))
                || (inventoryItem.model_name_ow == true && !inventoryItem.model_name.Equals(originalAsset.model_name))
                || (inventoryItem.jsn_ow == true && (inventoryItem.jsn_code != null
                    || inventoryItem.jsn_utility1 != originalAsset.jsn_utility1
                    || inventoryItem.jsn_utility2 != originalAsset.jsn_utility2
                    || inventoryItem.jsn_utility3 != originalAsset.jsn_utility3
                    || inventoryItem.jsn_utility4 != originalAsset.jsn_utility4
                    || inventoryItem.jsn_utility5 != originalAsset.jsn_utility5
                    || inventoryItem.jsn_utility6 != originalAsset.jsn_utility6
                    || inventoryItem.jsn_utility7 != originalAsset.jsn_utility7
                )));
        }

        public void ResetControlColumnToRegenerateCoverSheet(asset modifiedItem)
        {

            asset itemDB = _db.assets.Find(modifiedItem.asset_id, modifiedItem.domain_id);

            if (OverwritableFieldsHadChanged(itemDB, modifiedItem))
            {
                var inventories = _db.project_room_inventory.Where(IsSameAssetAndOverwritableFieldsAreDifferent(modifiedItem)).Select(i => i.inventory_id).ToList();
                if (inventories.Any())
                {
                    int total = inventories.Count();
                    while (total > 0)
                    {
                        var inventoriesStr = string.Join(";", inventories.GetRange(0, total > 500 ? 500 : total));
                        _db.update_inventories_cutsheet_filename(inventoriesStr, null);

                        if (total > 500)
                        {
                            inventories = inventories.GetRange(500, total - 500);
                            total = inventories.Count();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        /**
         * 
         * Sets flag on assets to regenerate cutsheets
         * 
         **/
        public void SetRegenerateCutSheets(short domain_id)
        {
            string domainIdClause = Helper.ShowAudaxWareInfo(domain_id) 
                                      ? "(domain_id = 1 OR domain_id = {0})" 
                                      : "domain_id = {0}";

            this._db.Database.ExecuteSqlCommand("INSERT INTO cutsheet_to_generate(asset_id, domain_id)" +
                                                " SELECT asset_id, domain_id FROM assets" + 
                                                $" WHERE {domainIdClause}" +
                                                " AND NOT EXISTS (" +
                                                "   SELECT asset_id, domain_id" +
                                                "   FROM cutsheet_to_generate" +
                                                "   WHERE cutsheet_to_generate.asset_id = assets.asset_id" +
                                                "       AND cutsheet_to_generate.domain_id = assets.domain_id" +
                                                " )",
                                                domain_id);
        }

        private bool OverwritableFieldsHadChanged(asset itemDB, asset modifiedItem)
        {
            return itemDB.asset_suffix != modifiedItem.asset_suffix
                || itemDB.placement != modifiedItem.placement
                || itemDB.@class != modifiedItem.@class
                || itemDB.asset_description != modifiedItem.asset_description
                || itemDB.model_number != modifiedItem.model_number
                || itemDB.model_name != modifiedItem.model_name
                || itemDB.jsn_domain_id != modifiedItem.jsn_domain_id
                || itemDB.jsn_id != modifiedItem.jsn_id
                || itemDB.jsn_suffix != modifiedItem.jsn_suffix
                || (modifiedItem.jsn_utility1_ow.GetValueOrDefault() && itemDB.jsn_utility1 != modifiedItem.jsn_utility1)
                || (modifiedItem.jsn_utility2_ow.GetValueOrDefault() && itemDB.jsn_utility2 != modifiedItem.jsn_utility2)
                || (modifiedItem.jsn_utility3_ow.GetValueOrDefault() && itemDB.jsn_utility3 != modifiedItem.jsn_utility3)
                || (modifiedItem.jsn_utility4_ow.GetValueOrDefault() && itemDB.jsn_utility4 != modifiedItem.jsn_utility4)
                || (modifiedItem.jsn_utility5_ow.GetValueOrDefault() && itemDB.jsn_utility5 != modifiedItem.jsn_utility5)
                || (modifiedItem.jsn_utility6_ow.GetValueOrDefault() && itemDB.jsn_utility6 != modifiedItem.jsn_utility6)
                || (modifiedItem.jsn_utility7_ow.GetValueOrDefault() && itemDB.jsn_utility7 != modifiedItem.jsn_utility7);
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