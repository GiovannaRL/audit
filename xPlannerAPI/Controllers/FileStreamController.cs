using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Diagnostics;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Services;
using System.Web;
using xPlannerAPI.App_Data;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class FileStreamController : AudaxWareController
    {
        [ActionName("MaxFileSize")]
        public HttpResponseMessage GetMaxFileSize(short id1)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new UniqueString() { text = (Helper.GetMaxRequestLength()) });
        }

        [HttpPost]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ActionName("upload")]
        public async Task<HttpResponseMessage> Add(short id1, int id2, string id3, string id4, string id5)
        {
            try
            {
                var domainId = id1;
                var assetId = id2;
                var filename = id3;
                var columnName = id4;
                var container = id5 + id1.ToString();
                Trace.TraceInformation("File upload: id1:{0}, id2: {1}, id3: {2}, id4: {3}", id1, id2, id3, id4);
                var isAssetFile = columnName != "quote" && columnName != "po";
                if (!SetDomain(domainId))
                {
                    Trace.TraceError("Error to set domain id {0}", domainId);
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }


                Stream requestStream;

                try
                {
                    //I don't know exactly why, but readAsStream is not working for jpg files
                    if (columnName == "photo")
                    {
                        var requestStreamXX = await Request.Content.ReadAsMultipartAsync();
                        requestStream = requestStreamXX.Contents[0].ReadAsStreamAsync().Result;
                    }
                    else
                    {
                        requestStream = await Request.Content.ReadAsStreamAsync();
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error to load file content from the request: id1:{0}, id2: {1}, id3: {2}, id4: {3}, Exception: {4}", id1, id2, id3, id4, ex);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                try
                {
                    using (var repositoryBlob = new FileStreamRepository())
                    {
                        Trace.TraceInformation("Uploading file to blob storage: container {0}, filename {1}", container, filename);
                        var blob = repositoryBlob.GetBlob(container, filename);
                        //rename old blob if exists
                        if (blob.Exists())
                        {
                            using (var stream = new MemoryStream())
                            {
                                blob.DownloadTo(stream);
                                stream.Seek(0, SeekOrigin.Begin);
                                var newBlobName = blob.Name.Substring(0, blob.Name.LastIndexOf(".")) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "." + blob.Name.Split('.').Last();
                                var newBlob = repositoryBlob.GetBlob(container, newBlobName);
                                Trace.TraceInformation("A file with the same name already exists, renaming it to: container {0}, filename {1}", container, newBlobName);
                                newBlob.Upload(stream);
                                Trace.TraceInformation("Blob renamed");
                            }

                        }
                        blob.Upload(requestStream, overwrite: true);
                        Trace.TraceInformation("Completed uploading file: container {0}, filename {1}", container, filename);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error to upload file to blob storage: Container: {0}, File Name: {1}, Exception: {2}", container, filename, ex);
                    var message = string.Format(StringMessages.ImportFileSizeExceeded, Helper.GetMaxRequestLength());
                    return Request.CreateResponse(HttpStatusCode.RequestEntityTooLarge, new { ErrorMessage = message });
                    //return Request.CreateResponse(HttpStatusCode.InternalServerError, Helper.GetMaxRequestLengthMessage());
                }

                if (isAssetFile)
                {
                    try
                    {
                        var assetsCtrl = new AssetsController();
                        var asset = assetsCtrl.GetItem(domainId, assetId);

                        switch (columnName)
                        {
                            case "cut_sheet":
                                asset.cut_sheet = filename;
                                break;
                            case "cad_block":
                                asset.cad_block = filename;
                                break;
                            case "revit":
                                asset.revit = filename;
                                break;
                            case "photo":
                                asset.photo = filename;
                                break;
                        }
                        using (var repository = new CutSheetRepository(asset.domain_id))
                        {
                            if (repository.BuildFullFromZero(asset, BlobContainersName.FullCutsheet(asset.domain_id), asset.asset_id.ToString() + asset.domain_id.ToString() + ".pdf"))
                            {
                                using (IAssetRepository assetRepository = new AssetRepository())
                                {
                                    assetRepository.UpdateAssetSimple(asset);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Error to upload file for asset. The asset for domain id {0}, asset id {1}, Exception {2}", domainId, assetId, ex);
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    var projectId = id2;
                    var poId = Convert.ToInt32(filename.Split('.').First());
                    try
                    {
                        var poCtrl = new PurchaseOrdersController();
                        var po = poCtrl.GetItem(domainId, projectId, poId);
                        if (po == null)
                        {
                            Trace.TraceError("Error to upload file for PO. The PO for domain id {0}, project id {1}, po id {2} could not be found", domainId, projectId, poId);
                            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        }

                        switch (columnName)
                        {
                            case "quote":
                                po.quote_file = filename;
                                break;
                            case "po":
                                po.po_file = filename;
                                break;
                        }
                        poCtrl.Put(po, domainId, projectId, poId);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Could not update database with new PO (domain id {0}, project id {1}, po id {2}): Exception {3}",
                            domainId, projectId, poId, ex);
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (IOException e)
            {
                if (e.InnerException != null && e.InnerException is HttpException && (e.InnerException as HttpException).WebEventCode == 3004)
                {
                    var message = string.Format(StringMessages.ImportFileSizeExceeded, Helper.GetMaxRequestLength());
                    return Request.CreateResponse(HttpStatusCode.RequestEntityTooLarge, new { ErrorMessage = message });
                }

                throw e;
            }
        }

        [ActionName("File")]
        public HttpResponseMessage Get(int id1, string id2, string id3, int? id4 = null, int? id5 = null)
        {
            try
            {
                var filename = id2;
                var blobFilename = filename;
                var containerName = id3;
                var containerNameWithDomain = id3 + id1;

                var drawingId = id5;

                //THIS IS NECESSARY BECAUSE THE DRAWING FILE HAS THE DRAWING ID AS NAME, NOT THE FILENAME AS CUTSHEET, CADBLOCK ETC
                if (drawingId != null)
                    blobFilename = drawingId + "." + filename.Split('.').Last();

                //THIS IS NECESSARY BECAUSE PHOTOS DOES NOT HAVE DOMAIN
                //if (container_name.Equals(BlobsName.Photo))
                //    container_name_with_domain = id3;
                //else 
                if (containerName.Equals(BlobContainersName.GenericCoversheet) || containerName.Equals(BlobContainersName.GenericFullCutSheet))
                    blobFilename = id2 + id1 + ".pdf";

                using (var repositoryBlob = new FileStreamRepository())
                {
                    var assetId = 0;
                    int.TryParse(id2, out assetId);
                    return repositoryBlob.DownloadHttpMessage(containerNameWithDomain, blobFilename, id1, assetId);
                }
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
    }
}