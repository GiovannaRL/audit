using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using xPlannerAPI.Services;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Net.Http.Headers;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models.Enum;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsReports)]
    public class ReportController : TableGenericController<project_report>
    {
        public ReportController() : base(new [] { "project_domain_id", "project_id", "id" },
            new [] { "project_domain_id", "project_id", "phase_id", "department_id", "room_id" }, new [] { "report_type" })
        { }

        protected override bool UpdateReferences(project_report item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            item.project_domain_id = (short)id1;
            item.project_id = id2.GetValueOrDefault();

            if (id3 > 0) {
                item.id = id3.GetValueOrDefault();
            } else {
                // This informations are automatically set only when inserting a new report
                item.status = 0;
                item.status_category = ReportStatusCategory.Waiting.Category;
                item.generated_by = UserId;
                item.last_run = DateTime.Now;
            }

            return true;
        }
        
        [ActionName("Item")]
        public async Task<project_report> Add(int id1, int id2, [FromBody] project_report item)
        {
            using (var repository = new ReportRepository())
            {
                if (!UpdateReferences(item, id1, id2, 0))
                    return null;

                return await repository.Add(item);
            }
        }
        
        [ActionName("All")]
        public override IEnumerable<project_report> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return base.GetAll(id1, id2, id3, id4, id5).Where(pr => !pr.isPrivate || pr.generated_by.Equals(UserId));
        }
        
        [ActionName("OnlyStatuses")]
        public HttpResponseMessage GetOnlyStatuses(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var statuses = base.GetAll(id1, id2, id3, id4, id5).Where(pr => !pr.isPrivate || pr.generated_by.Equals(UserId)).Select(pr => new {
                id = pr.id,
                statusPercentage = pr.status,
                statusCategory = pr.status_category
            });

            return Request.CreateResponse(HttpStatusCode.OK, statuses);
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var report = base.GetItem(id1, id2, id3, id4, id5);

            if (report == null)
                return base.Delete(id1, id2, id3, id4, id5);

            using (var repository = new FileStreamRepository())
            {
                repository.DeleteBlob("reports", report.file_name + ".pdf");
                repository.DeleteBlob("reports", report.file_name + ".xlsx");
            }

            return base.Delete(id1, id2, id3, id4, id5);
        }

        [ActionName("Download")]
        public HttpResponseMessage Get(int id1, int id2, int id3, [FromUri] string extension, [FromUri] string timestamp)
        {
            try
            {
                var report = base.GetItem(id1, id2, id3);

                if (report?.file_name != null && (!report.isPrivate || report.generated_by.Equals(UserId)))
                {
                    using (var repository = new FileStreamRepository())
                    {
                        var downloadedFile = repository.DownloadBlobFile("reports", report.file_name + "." + (extension ?? "pdf"));

                        if (downloadedFile == null)
                        {
                            return new HttpResponseMessage(HttpStatusCode.NotFound);
                        }

                        // Reset the stream position; otherwise, download will not work
                        downloadedFile.BlobStream.Position = 0;

                        // Create response message with blob stream as its content
                        var message = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StreamContent(downloadedFile.BlobStream)
                        };

                        // Set content headers
                        message.Content.Headers.ContentLength = downloadedFile.BlobLength;
                        message.Content.Headers.ContentType = new MediaTypeHeaderValue(downloadedFile.BlobContentType);
                        message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = HttpUtility.UrlDecode(downloadedFile.BlobFileName),
                            Size = downloadedFile.BlobLength
                        };

                        var response = HttpContext.Current.Response;
                        response.Clear();
                        response.AppendCookie(new HttpCookie(report.id + "Token", timestamp));

                        // Need to add timestamp passed previously as cookie to browser be able to know when the file download is finished
                        message.Headers.AddCookies(new[] { new CookieHeaderValue(report.id + "Token", timestamp) });

                        return message;
                    }
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [ActionName("Regenerate")]
        public async Task<HttpResponseMessage> GetRegenerated(int id1, int id2, int id3)
        {
            using (var rep = new TableRepository<project_report>())
            {
                var report = rep.Get(new [] { "project_domain_id", "project_id", "id" }, new [] { id1, id2, id3 }, new [] { "report_type", "project_room" });

                if (report == null)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);

                using (var repository = new ReportRepository())
                {
                    if (!UpdateReferences(report, id1, id2, id3))
                        return null;

                    //report.status = 0;
                    //report.last_run = DateTime.Now;
                    await repository.Regenerate(report);

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
        }
    }
}
