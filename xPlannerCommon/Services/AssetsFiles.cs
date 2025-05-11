using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerCommon.Models;

namespace xPlannerCommon.Services
{
    public class AssetsFiles : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;
        private asset[] _assets;

        public AssetsFiles(int domain_id)
        {
            this._db = new audaxwareEntities();
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '" + domain_id + "';";
                cmd.ExecuteNonQuery();
            }
            this._assets = this._db.assets.Where(a => a.domain_id == domain_id).ToArray();
            //this._toDelete = new List<Blob>();
        }

        public void RenameAssetsPhoto(int domain_id)
        {
            string containerName = BlobContainersName.Photo(domain_id);
            string newPhotoName;
            List<string> toDelete = new List<string>();

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                foreach (asset ass in this._assets)
                {
                    if (ass.photo != null && !ass.photo.Equals(""))
                    {
                        newPhotoName = String.Format("{0}.{1}", ass.asset_id, ass.photo.Substring(ass.photo.Length - 3));
                        if (!newPhotoName.Equals(ass.photo))
                        {
                            toDelete.Add(ass.photo);
                            fileRepository.copyBlob(containerName, ass.photo, containerName, newPhotoName);
                            //ass.photo = newPhotoName;
                        }
                    }
                }

                using (FileStreamRepository repository = new FileStreamRepository())
                {
                    foreach (string oldPhoto in toDelete)
                    {
                        repository.DeleteBlob(containerName, oldPhoto);
                    }
                }
            }
        }

        public void RenameAssetsCutsheet(int domain_id)
        {
            string containerName = BlobContainersName.Cutsheet(domain_id);
            string newCutsheetName;
            List<string> toDelete = new List<string>();

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                foreach (asset ass in this._assets)
                {
                    if (ass.cut_sheet != null && !ass.cut_sheet.Equals(""))
                    {
                        newCutsheetName = String.Format("{0}.{1}", ass.asset_id, ass.cut_sheet.Substring(ass.cut_sheet.Length - 3));
                        if (!newCutsheetName.Equals(ass.cut_sheet))
                        {
                            toDelete.Add(ass.cut_sheet);
                            fileRepository.copyBlob(containerName, ass.cut_sheet, containerName, newCutsheetName);
                            //ass.cut_sheet = newCutsheetName;
                        }
                    }
                }

                using (FileStreamRepository repository = new FileStreamRepository())
                {
                    foreach (string oldCutSheet in toDelete)
                    {
                        repository.DeleteBlob(containerName, oldCutSheet);
                    }
                }
            }
        }

        public void RenameAssetsRevit(int domain_id)
        {
            string containerName = BlobContainersName.Cutsheet(domain_id);
            string newRevitName;
            List<string> toDelete = new List<string>();

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                foreach (asset ass in this._assets)
                {
                    if (ass.revit != null && !ass.revit.Equals(""))
                    {
                        newRevitName = String.Format("{0}.{1}", ass.asset_id, ass.revit.Substring(ass.revit.Length - 3));
                        if (!newRevitName.Equals(ass.revit))
                        {
                            toDelete.Add(ass.revit);
                            fileRepository.copyBlob(containerName, ass.revit, containerName, newRevitName);
                            //ass.revit = newRevitName;
                        }
                    }
                }

                using (FileStreamRepository repository = new FileStreamRepository())
                {
                    foreach (string oldRevit in toDelete)
                    {
                        repository.DeleteBlob(containerName, oldRevit);
                    }
                }
            }
        }

        public void RenameAssetsCadBlock(int domain_id)
        {
            string containerName = BlobContainersName.Cutsheet(domain_id);
            string newCadBlockName;
            List<string> toDelete = new List<string>();

            using (FileStreamRepository fileRepository = new FileStreamRepository())
            {
                foreach (asset ass in this._assets)
                {
                    if (ass.cad_block != null && !ass.cad_block.Equals(""))
                    {
                        newCadBlockName = String.Format("{0}.{1}", ass.asset_id, ass.cad_block.Substring(ass.cad_block.Length - 3));
                        if (!newCadBlockName.Equals(ass.cad_block))
                        {
                            toDelete.Add(ass.cad_block);
                            fileRepository.copyBlob(containerName, ass.cad_block, containerName, newCadBlockName);
                            //ass.cad_block = newCadBlockName;
                        }
                    }
                }

                using (FileStreamRepository repository = new FileStreamRepository())
                {
                    foreach (string oldCadBlock in toDelete)
                    {
                        repository.DeleteBlob(containerName, oldCadBlock);
                    }
                }
            }
        }

        public void SaveChanges(int domain_id)
        {
            string SqlCommand = String.Format("UPDATE assets SET photo = CONCAT(asset_id, '.', RIGHT(photo, 3)) WHERE domain_id = {0} AND photo IS NOT NULL AND photo <> '';", domain_id);
            this._db.Database.ExecuteSqlCommand(SqlCommand);
            this._db.Database.ExecuteSqlCommand(SqlCommand.Replace("photo", "cut_sheet"));
            this._db.Database.ExecuteSqlCommand(SqlCommand.Replace("photo", "revit"));
            this._db.Database.ExecuteSqlCommand(SqlCommand.Replace("photo", "cad_block"));
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
