using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerCommon.Services;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.IO.IsolatedStorage;
using System.Reflection;
using xPlannerCommon.Enumerators;
using System;
using System.Diagnostics;
using System.Web;
using System.Web.Configuration;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsAssets)]
    //[AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class AssetsInventoryImporterController : AudaxWareController
    {
        /// <summary>
        /// If an attacker posts several calls, we will create multiple task and the system might lock down. Hence we limit
        /// the number of calls that can be made to the asynchronous import
        /// </summary>
        static int resourceStarvationCounter = 0;

        const int resourceStarvationLimit = 1;

        [ActionName("Analyze")]
        public async Task<HttpResponseMessage> Analyze(int id1, int id2, int id3)
        {
            try
            {
                const string directory = "import_inventory";
                var timestamp = DateTime.Now;
                var filename = String.Format("{0}_{1}_{2}_{3}.xlsx", id1, id2, timestamp.ToShortDateString().Replace("/", ""), timestamp.ToShortTimeString().Replace(":", ""));
                var fullPath = Path.Combine(directory, filename);

                var isoStore = IsolatedStorageFile.GetMachineStoreForAssembly();
                if (!isoStore.DirectoryExists(directory))
                    isoStore.CreateDirectory(directory);

                if (isoStore.FileExists(fullPath))
                    isoStore.DeleteFile(fullPath);

                var fullDir = "";
                using (var isoStream = new IsolatedStorageFileStream(fullPath, FileMode.CreateNew, isoStore))
                {
                    var requestStreamXX = await Request.Content.ReadAsMultipartAsync();
                    var requestStream = requestStreamXX.Contents[0].ReadAsStreamAsync().Result;
                    requestStream.CopyTo(isoStream);
                    fullDir = isoStream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(isoStream)?.ToString();
                }

                using (IAssetsInventoryImporterRepository rep = new AssetsInventoryImporterRepository())
                {
                    var result = rep.Analyze(id1, id2, fullDir, (ImportColumnsFormat)id3);
                    var valid = false;

                    for (int i = 0; i < result.Count; i++)
                    {
                        if (result[i].Status == ImportAnalysisResultStatus.Ok)
                        {
                            valid = true;
                        }
                    }

                    if (valid == true)
                        return Request.CreateResponse(HttpStatusCode.OK, result);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest, result);
                }
            }
            catch(IOException e)
            {
                if (e.InnerException != null && e.InnerException is HttpException && (e.InnerException as HttpException).WebEventCode == 3004)
                {
                    HttpRuntimeSection section = System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                    var message = string.Format(StringMessages.ImportFileSizeExceeded, Math.Floor((decimal) section.MaxRequestLength / 1024).ToString());
                    return Request.CreateResponse(HttpStatusCode.RequestEntityTooLarge, new ImportAnalysisResult { Status = ImportAnalysisResultStatus.Invalid, ErrorMessage = message });
                }
                
                throw e;
            }
            catch (Exception e)
            {
                Trace.TraceError("Error import assets to inventory {0}", (e.InnerException != null ? e.InnerException.Message : e.Message));
                throw;
            }
        }

        [ActionName("ImportAsync")]
        public HttpResponseMessage PutImportAsync(int id1, int id2, [FromBody] ImportItem[] data)
        {
            if (resourceStarvationCounter >= resourceStarvationLimit)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }
            var userId = UserId;
            var userName = UserName;
            var domainId = id1;
            var showAudaxwareInfo = xPlannerCommon.App_Data.Helper.ShowAudaxWareInfo(domainId);
            Task.Run(() =>
            {
                using (var rep = new AssetsInventoryImporterRepository())
                {
                    foreach (var item in data)
                    {
                        if (item.NetworkRequired != null || item.NetworkRequired == "")
                        {
                            item.NetworkOption = rep._networks[item.NetworkRequired.ToLower()];
                        }
                        else
                        {
                            item.NetworkOption = null;                           
                        }                        
                    }

                    using (var security = new SessionConnectionInterceptor.ThreadSecurityInterceptor(id1, showAudaxwareInfo))
                    {
                        using (NotificationRepository notificationRepo = new NotificationRepository((short)id1, userId))
                        {
                            try
                            {
                                DateTime start = DateTime.Now;
                                var result = rep.Import(data, id1, id2, userName, notificationRepo);
                                var timeImport = DateTime.Now - start;
                                if (timeImport.Minutes > 5)
                                {
                                    Trace.TraceWarning($"Import for domain {id1} took more than 5 minutes. {data.Length} items were imported");
                                }

                                string message;
                                if (result.Status == ImportAnalysisResultStatus.Ok)
                                {
                                    message = String.Format(StringMessages.ImportCompletedSuccess, (int)timeImport.TotalMinutes);
                                }
                                else
                                {
                                    message = String.Format(StringMessages.ImportCompletedError, result.Status, result.ErrorMessage);
                                }
                                notificationRepo.Notify(message);
                            }
                            catch(Exception ex)
                            {
                                notificationRepo.Notify(StringMessages.ImportExceptionError);
                                Trace.TraceError($"Exception generated while importing {ex.Message} - Stack:{ex.StackTrace}");
                            }
                        }
                    }
                }
            });
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [ActionName("Import")]
        public HttpResponseMessage PutImport(int id1, int id2, [FromBody] ImportItem[] data)
        {
            using (IAssetsInventoryImporterRepository rep = new AssetsInventoryImporterRepository())
            {
                var result = rep.Import(data, id1, id2, UserName, null);
                return Request.CreateResponse(result.Status == ImportAnalysisResultStatus.Ok ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
            }
        }
    }
}