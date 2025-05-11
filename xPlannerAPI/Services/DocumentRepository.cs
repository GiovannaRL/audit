using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models.Enums;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using System.Diagnostics;

namespace xPlannerAPI.Services
{
    public class DocumentRepository : IDocumentRepository, IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public DocumentRepository()
        {
            this._db = new audaxwareEntities();
        }

        private void DeletePicture(int domain_id, project_documents picture)
        {
            if (picture != null)
            {
                using (FileStreamRepository fileRepository = new FileStreamRepository())
                {
                    var container = string.Format("photo{0}", domain_id);
                    var filename = picture.blob_file_name;
                    try
                    {
                        //VERIFY IF FILE EXISTS SO WE AVOID ERROR
                        var blob = fileRepository.GetBlob(container, filename);
                        if (blob.Exists()) { 
                            // Delete from blob
                            fileRepository.DeleteBlob(container, filename);

                            //AUDIT
                            using (var auditRep = new AuditRepository())
                            {
                                auditRep.CompareAndSaveAuditedData(picture, null, "DELETE", new project_documents(), "PICTURE DELETED");
                            }
                        }
                    }
                    catch (Exception ex) {
                        Trace.TraceError("Error to delete file from blob storage: Container: {0}, File Name: {1}, Exception: {2}", container, filename, ex);
                    }

                    // Delete from database
                    _db.project_documents.Remove(picture);
                    if(_db.SaveChanges() > 0) { 
                        //AUDIT
                        using (var auditRep = new AuditRepository())
                        {
                            auditRep.CompareAndSaveAuditedData(picture, null, "DELETE", new project_documents(), "DOCUMENT PICTURE DELETED");
                        }
                    }

                }

            }
        }

        public void DeleteRoomPicture(int domain_id, int project_id, int phase_id, int department_id, int room_id, int picture_id)
        {
            var picture = _db.project_documents.Include("documents_associations").Include("document_types.documents_display_levels").FirstOrDefault(pd =>
                pd.project_domain_id == domain_id && pd.project_id == project_id && pd.id == picture_id
                    && pd.document_types.documents_display_levels.description.Equals(DocumentDisplayLevelEnum.Room)
                    && pd.documents_associations.Any(da => da.phase_id == phase_id && da.department_id == department_id && da.room_id == room_id));

            DeletePicture(domain_id, picture);
        }

        public void DeleteInventoryPicture(int domain_id, int project_id, int inventory_id, int picture_id)
        {
            var picture = _db.project_documents.Include("documents_associations").Include("document_types.documents_display_levels").FirstOrDefault(pd =>
                pd.project_domain_id == domain_id && pd.id == picture_id
                    && pd.document_types.documents_display_levels.description.Equals(DocumentDisplayLevelEnum.Inventory)
                    && pd.documents_associations.Any(da => da.inventory_id == inventory_id && da.project_id == project_id));

            DeletePicture(domain_id, picture);
        }

        public List<PictureInfo> GetRoomPictures(int domain_id, int project_id, int phase_id, int department_id, int room_id)
        {
            return _db.documents_associations.Include("project_documents.document_types.documents_display_levels")
                .Where(da => da.project_domain_id == domain_id
                    && da.project_id == project_id && da.phase_id == phase_id && da.department_id == department_id
                    && da.room_id == room_id && da.inventory_id == null && da.asset_domain_id == null && da.asset_id == null
                    && da.project_documents.document_types.documents_display_levels.description.Equals(DocumentDisplayLevelEnum.Room))
                    .Select(da => new PictureInfo
                    {
                        blobFilename = da.project_documents.blob_file_name,
                        filename = da.project_documents.filename,
                        id = da.project_documents.id
                    }).ToList();
        }

        public List<InventoryPictureInfo> GetInventoryPictures(int domain_id, int project_id, int inventory_id)
        {
            return _db.documents_associations.Include("project_documents.document_types.documents_display_levels")
                .Where(da => da.project_domain_id == domain_id
                    && da.project_id == project_id && da.inventory_id == inventory_id
                    && da.project_documents.document_types.documents_display_levels.description
                    .Equals(DocumentDisplayLevelEnum.Inventory)
                    && da.document_id == da.project_documents.id)
                    .Select(da => new InventoryPictureInfo
                    {
                        blobFilename = da.project_documents.blob_file_name,
                        filename = da.project_documents.filename,
                        id = da.project_documents.id,
                        isAssetPhoto = da.project_documents.type_id == DocumentTypeIdEnum.AssetPhoto,
                        isTagPhoto = da.project_documents.type_id == DocumentTypeIdEnum.TagPhoto,
                        isArtwork = da.project_documents.type_id == DocumentTypeIdEnum.Artwork,
                        rotate = da.project_documents.rotate
                        
                    }).ToList();
        }

        public bool ChangeDocumentType(int domain_id, int project_id, int document_id, int newTypeId)
        {
            documents_associations item = _db.documents_associations.Include("project_documents").FirstOrDefault(i => i.project_domain_id == domain_id && i.project_id == project_id && i.document_id == document_id);

            if (item != null && item.project_documents != null)
            {
                item.project_documents.type_id = newTypeId;
                _db.Entry(item.project_documents).State = EntityState.Modified;
                return _db.SaveChanges() > 0;
            }

            return false;
        }

        public bool RotatePhoto(int domain_id, int project_id, int document_id, int newRotate)
        {
            documents_associations item = _db.documents_associations.Include("project_documents").FirstOrDefault(i => i.project_domain_id == domain_id && i.project_id == project_id && i.document_id == document_id);

            if(item != null && item.project_documents != null)
            {
                if (newRotate > 3)
                    newRotate = 0;

                if (newRotate < 0)
                    newRotate = 3;           
                
                item.project_documents.rotate = newRotate;
                _db.Entry(item.project_documents).State = EntityState.Modified;
                return _db.SaveChanges() > 0;
            }
            return false;
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