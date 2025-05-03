using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using xPlannerCommon.Extensions;
using xPlannerCommon.Models;

namespace xPlannerCommon.Services
{
    public class FileStreamRepository : IDisposable
    {
        private bool _disposed = false;
        private audaxwareEntities _db;
        private BlobServiceClient _blobServiceClient;

        private void Init()
        {
            this._db = new audaxwareEntities();
            this._blobServiceClient = new BlobServiceClient(AudaxWareConfiguration.GetStorageConnectionString());
        }

        public FileStreamRepository()
        {
            this.Init();
        }

        private BlobContainerClient GetContainerClient(string container_name)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container_name);
            containerClient.CreateIfNotExists();
            return containerClient;
        }

        public BlobClient GetBlob(string container_name, string filename)
        {
            var containerClient = GetContainerClient(container_name);
            var blobClient = containerClient.GetBlobClient(filename);
            return blobClient;
        }

        public void copyBlob(string sourceContainer, string sourceBlob, string destinationContainer, string destinationBlob)
        {
            var source = GetBlob(sourceContainer, sourceBlob);
            if (source.Exists())
            {
                var destination = GetBlob(destinationContainer, destinationBlob);
                destination.StartCopyFromUri(source.Uri);
            }
        }

        public void moveBlob(string sourceContainer, string sourceBlob, string destinationContainer, string destinationBlob)
        {
            copyBlob(sourceContainer, sourceBlob, destinationContainer, destinationBlob);
            DeleteBlob(sourceContainer, sourceBlob);
        }

        public void DeleteBlob(string container_name, string filename)
        {
            var referenceCount = _db.project_documents.Where(x => x.blob_file_name == filename).Count();

            if (container_name != null && filename != null && referenceCount == 1)
            {
                var blob = GetBlob(container_name, filename);

                if (blob.Exists())
                    blob.Delete();
            }
        }

        public void DeleteBlobs(string container_name, IEnumerable<string> filenames)
        {
            if (!string.IsNullOrEmpty(container_name))
            {
                foreach (var filename in filenames)
                {
                    DeleteBlob(container_name, filename);
                }
            }
        }

        public void UploadToCloud(string filePath, string container, string filename)
        {
            using (var fileStream = System.IO.File.OpenRead(filePath))
            {
                var blob = GetBlob(container, filename);

                blob.Upload(fileStream, overwrite: true);
            }
        }

        public DirectoryInfo CreateLocalDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return Directory.CreateDirectory(directoryPath);
            }

            return new DirectoryInfo(directoryPath);
        }

        public void DeleteLocalDirectory(string directoryPath, bool recursive = false)
        {
            if (directoryPath != null && Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive);
            }
        }

        public string UploadBase64Hashed(string container, string filePrefix, string base64File, string extension)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = Convert.FromBase64String(base64File);
            var md5Picture = BitConverter.ToString(md5.ComputeHash(bytes));

            var ret = $"{filePrefix}_{md5Picture}";
            var fileName = $"{ret}.{extension}";
            var blob = GetBlob(container, fileName);
            // If the picture already exists, we keep it there, if the MD5 is the same
            // the picture is identical, so no need to upload
            if (!blob.Exists())
            {
                var contents = new MemoryStream(bytes);
                blob.Upload(contents, overwrite: true);
            }
            return ret;
        }

        public string GetBlobSasUri(BlobClient blobClient)
        {
            //  Defines the resource being accessed and for how long the access is allowed.
            var blobSasBuilder = new BlobSasBuilder
            {
                StartsOn = DateTime.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTime.UtcNow.AddHours(24)
            };
            //  Defines the type of permission.
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(blobSasBuilder).ToString();
        }

        public string SaveDocumentTemporarily(string file, string directory, string fileName)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string path = Path.Combine(directory, fileName);
                System.IO.File.WriteAllText(path, file);
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public BlobDownload DownloadBlobFile(string container, string filename)
        {
            var blob = GetBlob(container, filename);

            if (blob.Exists())
            {
                var ms = new MemoryStream();
                blob.DownloadTo(ms);

                // Build and return the download model with the blob stream and its relevant info
                return new BlobDownload
                {
                    BlobStream = ms,
                    BlobFileName = filename,
                    BlobLength = ms.Length,
                    BlobContentType = blob.GetProperties().Value.ContentType
                };
            }

            return null;
        }

        public HttpResponseMessage DownloadHttpMessage(string container, string filename, int domainId, int? assetId, string downloadFileName = null)
        {
            if (assetId != null)
            {
                using (var cutsheetRepository = new CutSheetRepository(1))
                    cutsheetRepository.CheckAndRegenerateCutsheet(assetId??0, domainId, container, filename);
            }

            BlobDownload file = DownloadBlobFile(container, filename);

            if (file != null)
            {
                file.BlobStream.Position = 0;

                // Create response message with blob stream as its content
                var message = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(file.BlobStream)
                };

                // Set content headers
                message.Content.Headers.ContentLength = file.BlobLength;
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(file.BlobContentType);
                message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = downloadFileName ?? filename,
                    Size = file.BlobLength
                };

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        public string SaveDocumentTemporarily(string file, string path)
        {
            try
            {
                System.IO.File.WriteAllText(path, file);
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool DeleteAllTemporaryDocs(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    foreach (var file in new DirectoryInfo(directory).GetFiles())
                    {
                        file.Delete();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteTemporaryDoc(string directory, string filename)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    new DirectoryInfo(directory).GetFiles(filename).FirstOrDefault().Delete();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteFile(string path)
        {
            return DeleteFile(path, false);
        }

        /// <summary>
        /// Deletes a file protecting against exceptions
        /// </summary>
        /// <param name="path">Path to the file to be deleted</param>
        /// <param name="ensureDelete">If the delete should keep retrying the delete. Notice the function will return
        /// but it will create a task that keeps trying to delete. Use this to true only for unique file names that you generate
        /// either from Guids or temporary file generation functions
        /// </param>
        /// <returns></returns>
        public bool DeleteFile(string path, bool ensureDelete)
        {
            try
            {
                var task = Task.Run(() =>
                {
                    int retryCount = 0;
                    while (File.Exists(path) && retryCount < 60)
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Error to delete file {0} - Exception {1}", path, ex);
                        }
                        if (File.Exists(path))
                        {
                            System.Threading.Thread.Sleep(10000);
                            ++retryCount;
                        }
                        if (!ensureDelete)
                            break;
                    }
                });
                task.Wait(1000);

                return !File.Exists(path);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error to delete file {0} - Exception {1}", path, ex);
                return false;
            }
        }

        public bool DeleteFiles(List<string> paths)
        {
            bool result = true;

            foreach (string path in paths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception)
                {
                    result = false;
                    continue;
                }
            }

            return result;
        }

        public MemoryStream GetMemoryStreamFile(string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            MemoryStream stream = new MemoryStream(data);
            stream.Position = 0;

            return stream;
        }

        public bool MergeFiles(string[] paths, string saveInto, string[] titles = null)
        {
            try
            {
                var pdf_helper = new PDFHelper.PDF();
                pdf_helper.MergeFiles(paths, titles, saveInto, 800 * 1024 * 1024);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }
            if (this._db != null)
            {
                this._db.Dispose();
                this._db = null;
            }
            this._blobServiceClient = null;
            this._disposed = true;
        }
    }
}
