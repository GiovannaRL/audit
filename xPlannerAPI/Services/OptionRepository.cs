using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models.Enums;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace xPlannerAPI.Services
{
    public class OptionRepository : IOptionRepository, IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public OptionRepository()
        {
            this._db = new audaxwareEntities();
        }

        private string UploadFile(int domain_id, string fileName, string base64File, string extension)
        {
            using (var repositoryBlob = new FileStreamRepository())
            {
                string container = $"photo{domain_id}";
                try
                {
                    return repositoryBlob.UploadBase64Hashed(container, fileName, base64File, extension);
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error to save option's picture " + fileName, e);
                    throw e;
                }
            }
        }

        public void AddPicture(GenericOption option, FileData picture)
        {
            if (picture != null && option != null)
            {
                picture.fileName = UploadFile(option.domain_id, $"option_pic_{option.domain_id}_{option.option_id}_{Regex.Replace(DateTime.Now.ToString(), @"[\/\:\s]", "-")}", picture.base64File, picture.fileExtension);
                InsertOptionPictureAssociation(option.domain_id, option, picture);
            }
        }

        public void InsertOptionPictureAssociation(short domain_id, GenericOption option, FileData picture)
        {
            if (option != null && picture != null)
            {
                this._db.add_option_picture(domain_id, picture.fileName,
                    $"{picture.fileName}.{picture.fileExtension}",
                    picture.fileExtension, option.option_id,
                    !string.IsNullOrEmpty(option.inventories_id), option.inventories_id);
            }
        }

        public assets_options AddCustomOption(GenericOption option, string addedBy)
        {
            option.option_id = this._db.add_asset_option(option.domain_id, option.code, option.data_type, option.description, option.min_cost,
                   option.max_cost, option.unit_price, option.asset_domain_id, option.asset_id, addedBy,
                   option.settings, AssetOptionScopeEnum.Custom, option.project_domain_id, option.project_id).FirstOrDefault().GetValueOrDefault();

            return option.ToAssetOption();

        }

        public assets_options AddOption(GenericOption option, string addedBy)
        {
          option.option_id = this._db.add_asset_option(option.domain_id, option.code, option.data_type, option.description, option.min_cost,
                option.max_cost, option.unit_price, option.asset_domain_id, option.asset_id, addedBy,
                option.settings, AssetOptionScopeEnum.Catalog, option.project_domain_id, option.project_id).FirstOrDefault().GetValueOrDefault();

            return option.ToAssetOption();
        }

        public void DeletePicture(assets_options option)
        {
            if (option != null && option.domain_document != null)
            {
                using (var fileRepo = new FileStreamRepository())
                {
                    fileRepo.DeleteBlob($"photo{option.domain_id}", option.domain_document.blob_file_name);
                }

                this._db.delete_asset_option_picture(option.domain_id, option.asset_option_id);
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