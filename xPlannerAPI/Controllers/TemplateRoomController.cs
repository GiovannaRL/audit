using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.DomainTemplates)]
    public class TemplateRoomController : AudaxWareController
    {
        private static string[] _getItem = new string[] { "domain_id", "project_id", "phase_id", "department_id", "room_id" };
        private static string[] _getAll = new string[] { "domain_id", "project_id", "phase_id", "department_id" };
        private static string[] _include = new string[] { "department_type", "project" };

        private void UpdateIds(project_room item, int domain_id, int? project_id, int? phase_id, int? department_id, int? room_id)
        {
            item.domain_id = (short)domain_id;
            if (project_id != null)
                item.project_id = project_id.GetValueOrDefault();

            if (phase_id != null)
                item.phase_id = phase_id.GetValueOrDefault();

            if (department_id != null)
                item.department_id = department_id.GetValueOrDefault();

            if (room_id != null)
                item.room_id = room_id.GetValueOrDefault();
        }

        protected bool UpdateReferences(project_room item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            item.is_template = true;
            item.drawing_room_number = "-";
            item.department_type_domain_id_template = item.department_type.domain_id;
            item.department_type_id_template = item.department_type.department_type_id;
            item.department_type = null;

            UpdateIds(item, id1, id2, id3, id4, id5);

            if (item.project != null)
            {
                if (item.project.domain_id == id1)
                {
                    item.project_domain_id_template = item.project.domain_id;
                    item.project_id_template = item.project.project_id;
                }
                item.project = null;
            }
            else
            {
                item.project_domain_id_template = null;
                item.project_id_template = null;
            }

            return true;
        }

        [HttpGet]
        [ActionName("TemplateList")]
        public IEnumerable<get_templates_Result> GetTemplateList(int id1, int? id2 = null, int? id3 = null)
        {
            using (ITemplateRepository repository = new TemplateRepository())
            {
                return repository.GetAll(id1, ShowAudaxWareInfo(id1), id2, id3);
            }
        }

        [HttpGet]
        [ActionName("Item")]
        public project_room Get(int id1, int id2, int id3, int id4, int id5)
        {
            using (var repository = new TableRepository<project_room>())
            {
                return repository.Get(_getItem, new [] { id1, id2, id3, id4, id5 }, _include);
            }
        }

        /**
         * id1 = domain_id
         * id2 = id
         */
        [HttpGet]
        [ActionName("ItemById")]
        public project_room GetById(int id1, int id2)
        {
            using (var repository = new TableRepository<project_room>())
            {
                return repository.GetAll(new[] { "domain_id", "id" }, new[] { id1, id2 }, null, true).FirstOrDefault();
            }
        }

        [HttpGet]
        [ActionName("AllByLinkedIdTemplate")]
        public IEnumerable<LinkedRooms> GetAllByLinkedIdTemplate(int id1, int id2, int id3, int id4, int id5)
        {
            using (var repository = new TableRepository<project_room>())
            {
                var room = repository.Get(_getItem, new int[] { id1, id2, id3, id4, id5 }, _include);

                using (ITemplateRepository template = new TemplateRepository())
                {
                    return template.GetLinkedRooms(room.id);
                }
            }
        }

        [HttpPut]
        [ActionName("Item")]
        public HttpResponseMessage Put([FromBody]project_room item, int id1)
        {
            using (ITemplateRepository repository = new TemplateRepository())
            {
                if (repository.GetUsedNames(id1, item.room_id, item.department_type_id_template).Contains(item.drawing_room_name.ToLower()))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "This enterprise already has a template with this name. Please choose a different name.");
                }

                UpdateReferences(item, id1);

                using (var repository2 = new TableRepository<project_room>())
                {
                    var oldItem = repository2.Get(_getItem, new[] { item.domain_id, item.project_id, item.phase_id, item.department_id, item.room_id }, null);


                    //check if there is linked template and if it's trying to change the department type and scope
                    if (repository2.GetAll(new[] { "applied_id_template" }, new[] { item.id.GetValueOrDefault() }, null)
                    .Any(pr => pr.linked_template == true) && (oldItem.department_type_id_template != item.department_type_id_template || oldItem.project_id_template != item.project_id_template))
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict, "The scope template could not be updated. There is linked room(s).");

                    }
                }

                var controller = new RoomsController();
                return Request.CreateResponse(HttpStatusCode.OK, controller.Put(item, id1, 1, 1, 1, item.room_id));
            }
        }

        [HttpPost]
        [ActionName("Item")]
        public HttpResponseMessage Post([FromBody]project_room item, int id1)
        {
            using (ITemplateRepository repository = new TemplateRepository())
            {
                if (repository.GetUsedNames(id1).Contains(item.drawing_room_name.ToLower()))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "This enterprise already has a template with this name. Please choose a different name.");
                }

                using (TableRepository<project_room> roomRepository = new TableRepository<project_room>())
                {
                    UpdateReferences(item, id1, 1, 1, 1);
                    var added = roomRepository.Add(item);

                    return Request.CreateResponse(HttpStatusCode.OK, added);
                }}
        }

        [HttpPost]
        [ActionName("Clone")]
        public HttpResponseMessage Clone([FromBody]project_room item, int id1, int id2, int id3, int id4, int id5)
        {
            using (ITemplateRepository repository = new TemplateRepository())
            {
                if (repository.GetUsedNames(id1).Contains(item.drawing_room_name.ToLower()))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "This enterprise already has a template with this name. Please choose a different name.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, repository.CloneTemplate(item, (short)id1, UserName, 1, id2, id3, id4, id5));
            }
        }

        [ActionName("Apply")]
        public HttpResponseMessage Apply([FromBody]ApplyTemplateData item, int id1, int id2, int id3, int id4, int id5)
        {
            using (ITemplateRepository repository = new TemplateRepository())
            {
                if (repository.ApplyTemplate(item, (short)id1, id2, id3, id4, id5, UserName))
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [ActionName("Unlink")]
        public HttpResponseMessage Unlink([FromBody]ApplyTemplateData item, int id1, int id2, int id3, int id4, int id5)
        {
            using (ITemplateRepository repository = new TemplateRepository())
            {
                return Request.CreateResponse(HttpStatusCode.OK, repository.UnlinkTemplate(item, (short)id1, id2, id3, id4, id5));
            }
        }

        [HttpPost]
        [ActionName("SaveRoom")]
        public HttpResponseMessage SaveRoom([FromBody]project_room item, int id1, int id2, int id3, int id4, int id5)
        {
            using (ITemplateRepository repository = new TemplateRepository())
            {
                if (repository.GetUsedNames(id1).Contains(item.drawing_room_name.ToLower()))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "This enterprise already has a template with this name. Please choose a different name.");
                }

                item.project_id = id2;
                item.domain_id = (short)id1;
                return Request.CreateResponse(HttpStatusCode.OK, repository.CloneTemplate(item, (short)id1, UserName, (short)id1, id2, id3, id4, id5, true, item.comment));
            }
        }

        [HttpDelete]
        [ActionName("Item")]
        public HttpResponseMessage Delete(int id1, int id2, int id3, int id4, int id5)
        {
            using (var repository = new TableRepository<project_room>())
            {
                var itemToDelete = repository.Get(_getItem, new[] { id1, id2, id3, id4, id5 }, null);

                if (itemToDelete.id == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                if (repository.GetAll(new[] { "applied_id_template" }, new[] { (int)itemToDelete.id }, null)
                    .Any(pr => pr.linked_template == true))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "The template could not be deleted. There is linked room(s).");
                }

                repository.Delete(itemToDelete);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
