using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDocuments)]
    public class PhaseDocumentsController : TableGenericController<phase_documents>
    {
        public PhaseDocumentsController() : base(new [] { "domain_id", "project_id", "phase_id", "drawing_id" }, new [] { "domain_id", "project_id", "phase_id" }) { }

        [HttpPost]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [ActionName("upload")]
        public async Task Upload(short id1, int id2, int id3, string id4, string id5)
        {
            var doc = new phase_documents
            {
                date_added = DateTime.Now,
                domain_id = id1,
                project_id = id2,
                phase_id = id3,
                filename = id4 + "." + id5
            };

            base.Add(doc, id1, id2, id3);

            var requestStream = await Request.Content.ReadAsStreamAsync();

            using (var repositoryBlob = new FileStreamRepository())
            {
                var saveAs = doc.drawing_id + doc.filename;
                var blob = repositoryBlob.GetBlob("document" + doc.domain_id, saveAs);
                blob.Upload(requestStream, overwrite: true);
            }
        }

        [ActionName("download")]
        [HttpGet]
        public HttpResponseMessage Download(int id1, int id2, int id3, int id4)
        {
            var doc = base.GetItem(id1, id2, id3, id4);

            if (doc == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var filename = doc.drawing_id + doc.filename;

            using (var repository = new FileStreamRepository())
            {
                return repository.DownloadHttpMessage($"document{doc.domain_id}", filename, id1, null, doc.filename);
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var doc = base.GetItem(id1, id2, id3, id4, id5);
            if (doc == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            using (var repositoryBlob = new FileStreamRepository())
            {
                repositoryBlob.DeleteBlob($"document{doc.domain_id}", doc.drawing_id.ToString() + doc.filename);
            }

            return base.Delete(id1, id2, id3, id4, id5);
        }
    }
}
