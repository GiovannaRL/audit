using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models.Enums;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using System.Web;
using xPlannerAPI.App_Data;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDocuments)]
    public class ProjectDocumentsController : TableGenericController<project_documents>
    {
        public ProjectDocumentsController() : base(new[] { "project_domain_id", "project_id", "id" }, new[] { "project_domain_id", "project_id" }, new[] { "document_types", "project_room_inventory", "document_types.documents_display_levels" }) { }

        protected override bool UpdateReferences(project_documents item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            item.type_id = item.document_types.id;
            item.document_types = null;

            return true;
        }

        [ActionName("Upload")]
        public async Task<HttpResponseMessage> Post(int id1, int id2, int id3, string id4)
        {
            try
            {
                var document = base.GetItem(id1, id2, id3);

                if (document != null)
                {
                    var oldBlobName = document.blob_file_name;

                    using (var repositoryBlob = new FileStreamRepository())
                    {
                        Stream requestStream;

                        if (id4.ToLower().Equals("jpg") || id4.ToLower().Equals("png") || id4.ToLower().Equals("jpeg"))
                        {
                            var requestStreamPhoto = await Request.Content.ReadAsMultipartAsync();
                            requestStream = requestStreamPhoto.Contents[0].ReadAsStreamAsync().Result;
                        }
                        else
                        {
                            requestStream = await Request.Content.ReadAsStreamAsync();
                        }


                        document.blob_file_name = string.Format("{0}{1}{2}_{3}.{4}", document.project_domain_id, document.project_id, document.id, document.filename, id4);
                        var blob = repositoryBlob.GetBlob(string.Format("project-documents{0}", document.project_domain_id), document.blob_file_name);

                        blob.Upload(requestStream, overwrite: true);

                        base.Put(document, document.project_domain_id, document.project_id, document.id);

                        if (oldBlobName != null)
                        {
                            repositoryBlob.DeleteBlob(string.Format("project-documents{0}", document.project_domain_id), oldBlobName);
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK);
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

        [ActionName("All")]
        public override IEnumerable<project_documents> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return base.GetAll(id1, id2, id3, id4, id5)
                .Where(pd => pd.document_types.documents_display_levels.description.Equals(DocumentDisplayLevelEnum.Project));
        }

        [ActionName("Item")]
        public override project_documents GetItem(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var doc = base.GetItem(id1, id2, id3, id4, id5);
            if (doc != null)
            {
                doc.document_types.documents_display_levels = null;
            }
            return doc;
        }

        [ActionName("Download")]
        public HttpResponseMessage Get(short id1, int id2, int id3)
        {
            try
            {
                var document = base.GetItem(id1, id2, id3);

                using (var repositoryBlob = new FileStreamRepository())
                {
                    return repositoryBlob.DownloadHttpMessage(string.Format("project-documents{0}", document.project_domain_id), document.blob_file_name, id1, null, string.Format("{0}.{1}", document.filename, document.file_extension));
                }
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        [ActionName("Associations")]
        public HttpResponseMessage GetAvailableAssociations(short id1, int id2, int id3, string id4)
        {
            id4 = id4.ToLower();

            using (var rep = new ProceduresRepository())
            {
                var items = rep.GetAvailableAssociations(id1, id2, id3, id4);

                switch (id4)
                {
                    case "asset":
                        return Request.CreateResponse(HttpStatusCode.OK, items);
                    case "room":
                        return Request.CreateResponse(HttpStatusCode.OK, items.Select(i => new
                        {
                            domain_id = i.domain_id,
                            project_id = i.project_id,
                            project_description = i.project_description,
                            phase_id = i.phase_id,
                            phase_description = i.phase_description,
                            department_id = i.department_id,
                            department_description = i.department_description,
                            room_id = i.room_id,
                            room_number = i.room_number,
                            room_name = i.room_name
                        }));
                    case "department":
                        return Request.CreateResponse(HttpStatusCode.OK, items.Select(i => new
                        {
                            domain_id = i.domain_id,
                            project_id = i.project_id,
                            project_description = i.project_description,
                            phase_id = i.phase_id,
                            phase_description = i.phase_description,
                            department_id = i.department_id,
                            department_description = i.department_description
                        }));
                    case "phase":
                        return Request.CreateResponse(HttpStatusCode.OK, items.Select(i => new
                        {
                            domain_id = i.domain_id,
                            project_id = i.project_id,
                            project_description = i.project_description,
                            phase_id = i.phase_id,
                            phase_description = i.phase_description
                        }));
                    default:
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var doc = base.GetItem(id1, id2, id3, id4, id5);

            if (doc == null) return
                base.Delete(id1, id2, id3, id4, id5);

            using (var repository = new FileStreamRepository())
            {
                repository.DeleteBlob($"project-documents{doc.project_domain_id}",
                    doc.blob_file_name);
            }

            return base.Delete(id1, id2, id3, id4, id5);
        }

        [HttpPut]
        [ActionName("Associations")]
        public project_documents PutAssociations(project_documents item, short id1, int id2, int id3)
        {
            using (var rep = new ProjectFilesRepository())
            {
                rep.UpdateLinkedAssets(item);
                return item;
            }
        }

        /*
         * id1 = domain
         * id2 = project_id
         * id3 = document_id
         */
        [HttpPatch]
        [ActionName("Type")]
        public HttpResponseMessage Patch([FromBody] project_documents item, short id1, int id2, int id3)
        {
            using (IDocumentRepository rep = new DocumentRepository())
            {
                if (rep.ChangeDocumentType(id1, id2, id3, item.type_id))
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPatch]
        [ActionName("Rotate")]
        public HttpResponseMessage Rotate([FromBody] project_documents item, short id1, int id2, int id3)
        {
            using (IDocumentRepository rep = new DocumentRepository())
            {
                if (rep.RotatePhoto(id1, id2, id3, item.rotate))
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }
    }
}