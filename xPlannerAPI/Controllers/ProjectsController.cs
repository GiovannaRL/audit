using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Diagnostics;
using xPlannerAPI.Services;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using System.Threading.Tasks;
using xPlannerCommon;
using xPlannerCommon.Services;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class ProjectsController : TableGenericController<project>
    {

        static int resourceStarvationCounter = 0;

        const int resourceStarvationLimit = 1;

        public ProjectsController() : base(new[] { "domain_id", "project_id" }, new[] { "domain_id" }) { }

        protected override bool UpdateReferences(project item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (id2 == null && item.project_id <= 0)
            {
                item.copy_link = Guid.NewGuid();
            }

            if (item.client != null)
            {
                var clientsController = new ClientsController();
                item.client = clientsController.GetItem(id1, item.client.id);
                if (item.client != null)
                {
                    item.client_id = item.client.id;
                    item.client_domain_id = (short)id1;
                    item.client = null;
                }
                else
                {
                    Trace.TraceError("The client received from client is invalid");
                    return false;
                }
            }

            if (item.facility != null)
            {
                var facilitiesController = new FacilitiesController();
                item.facility = facilitiesController.GetItem(id1, item.facility.id);
                if (item.facility != null)
                {
                    item.facility_id = item.facility.id;
                    item.facility_domain_id = (short)id1;
                    item.facility = null;
                }
                else
                {
                    Trace.TraceError("The facility received from client is invalid");
                    return false;
                }
            }

            //update budget values for inventories
            if (id3 == 1)
            {
                using (ProjectRepository rep = new ProjectRepository())
                    rep.UpdateAssetsBudget(item, id1, id2 ?? 0);

            }

            //MADE TO SET TO NULL WHEN USER PUT ZERO IN THIS FIELDS
            item.markup = item.markup == 0 ? item.markup = null : item.markup;
            item.escalation = item.escalation == 0 ? item.escalation = null : item.escalation;
            item.tax = item.tax == 0 ? item.tax = null : item.tax;
            item.freight_markup = item.freight_markup == 0 ? item.freight_markup = null : item.freight_markup;
            item.install_markup = item.install_markup == 0 ? item.install_markup = null : item.install_markup;

            if (!item.status.Equals("L"))
            {
                item.locked_comment = null;
                item.locked_date = null;
            }

            return true;
        }

        [ActionName("All")]
        public override IEnumerable<project> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            try
            {
                using (var repository = new ProjectRepository())
                {
                    return repository.GetAll(id1, AudaxWareIdentity);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error to return project list: {ex.Message}");
                return null;
            }
        }

        [ActionName("Summarized")]
        public HttpResponseMessage GetAllSummarized(int id1, int? id2 = null)
        {
            try
            {
                using (var repository = new ProjectRepository())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, repository.GetAll(id1, AudaxWareIdentity).Select(p => new
                    {
                        p.domain_id,
                        p.project_id,
                        p.project_description,
                        p.status
                    }));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error to return summarized project list: {ex.Message}");
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        public override project Add([FromBody] project item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var retProject = base.Add(item, id1, id2, id3, id4, id5);

            using (ProjectRepository rep = new ProjectRepository())
                rep.AddProjectToUser(retProject, id1, AudaxWareIdentity.Name);

            return retProject;
        }


        [AudaxAuthorizeProject(DomainIdParam = "id3", ProjectIdParam = "id4")]
        [ActionName("CopyFrom")]
        public HttpResponseMessage Post(short id1, int id2, short id3, int id4, bool id5, int id6, int id7, int id8,
             int id9, int id10, int id11)
        {
            using (ICopyFromRepository repository = new CopyFromRepository())
            {
                var msg = repository.Copy(id1, id2, id6, id8, id10, id3, id4, id7, id9, id11, UserName, id5);
                return msg.Equals("")
                    ? Request.CreateResponse(HttpStatusCode.OK)
                    : Request.CreateResponse(HttpStatusCode.Conflict, msg);
            }
        }

        //[ActionName("CopyProject")]
        //public HttpResponseMessage PostCopyProject([FromBody] UniqueString item, short id1, int id2, bool id3 = false)
        //{
        //    using (ICopyFromRepository repository = new CopyFromRepository())
        //    {
        //        var copy = repository.CopyProject(id1, id2, item.text, id3, AudaxWareIdentity.Name);

        //        return Request.CreateResponse(copy == "" ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, copy);
        //    }
        //}


        [ActionName("CopyProjectAsync")]
        public HttpResponseMessage PostCopyProjectAsync([FromBody] UniqueString item, short id1, int id2, bool id3 = false)
        {
            if (resourceStarvationCounter >= resourceStarvationLimit)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }
            var userId = UserId;
            var username = UserName;
            var domainId = id1;
            var showAudaxwareInfo = Helper.ShowAudaxWareInfo(domainId);
            Task.Run(() =>
            {
                using (ICopyFromRepository rep = new CopyFromRepository())
                {

                    using (var security = new SessionConnectionInterceptor.ThreadSecurityInterceptor(id1, showAudaxwareInfo))
                    {
                        using (NotificationRepository notificationRepo = new NotificationRepository(id1, userId))
                        {
                            try
                            {
                                DateTime start = DateTime.Now;
                                var result = rep.CopyProject(id1, id2, item.text, id3, username);
                                var timeImport = DateTime.Now - start;
                                if (timeImport.Minutes > 5)
                                {
                                    Trace.TraceWarning($"Copy Project for project {id2} took more than 5 minutes. Still running.");
                                }

                                string message;
                                if (result == "")
                                {
                                    message = String.Format($"Project {item.text} copied successfully. Please refresh your projects list.", (int)timeImport.TotalMinutes);
                                }
                                else
                                {
                                    message = String.Format("Error trying to copy project", HttpStatusCode.InternalServerError, result);
                                }
                                notificationRepo.Notify(message);
                            }
                            catch (Exception ex)
                            {
                                notificationRepo.Notify(StringMessages.ImportExceptionError);
                                Trace.TraceError($"Exception generated while copying project {ex.Message} - Stack:{ex.StackTrace}");
                            }
                        }
                    }
                }
            });
            return new HttpResponseMessage(HttpStatusCode.OK);
        }



        [ActionName("Item")]
        public override project GetItem(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (id2 == 1)
                return null;

            using (var repository = new TableRepository<project>())
            {
                var proj = repository.Get(new[] { "domain_id", "project_id" }, GetIds(id1, id2, id3, id4, id5), new[] { "user_project_mine" });
                proj.user_project_mine = proj.user_project_mine.Where(u => u.userId.Equals(UserId)).ToList();

                return proj;
            }
        }

        [ActionName("CostField")]
        public HttpResponseMessage Get(int id1, int id2)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { cost_field = base.GetItem(id1, id2).default_cost_field });
        }

        [ActionName("CopiedProjects")]
        public IEnumerable<project> GetCopiedProjects(int id1, int id2)
        {
            using (var repository = new TableRepository<project>())
            {
                var actual_project = repository.Get(new[] { "domain_id", "project_id" }, GetIds(id1, id2), null);
                var projects = repository.GetAll(new[] { "domain_id" }, GetIds(id1), null)
                    .Where(x => x.copy_link == actual_project.copy_link && x.project_id != actual_project.project_id 
                        && (x.status == null || !x.status.Equals("R"))).ToList();
                return projects;
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var project = base.GetItem(id1, id2);
            project.status = "R"; //removed
            base.Put(project, id1, id2);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public override project Put(project item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            project oldProject = base.GetItem(id1, id2);
            if (oldProject.status?.Equals("L") == true)
            {
                if (item.status.Equals("L"))
                {
                    throw new InvalidOperationException("Locked projects cannot be updated");
                }

                using (TableRepository<AspNetUserRole> repository = new TableRepository<AspNetUserRole>())
                {
                    var role = repository.GetAll(new string[] { "UserId" }, new string[] { UserId }, new string[] { "AspNetRole" })
                        .FirstOrDefault(r => r.domain_id == id1);

                    if (role == null || !role.AspNetRole.Name.Equals("Administrators"))
                    {
                        throw new InvalidOperationException("Only administrators can change the status of a locked project");
                    }
                }
            } else
            {
                item.locked_date = DateTime.Now;
            }

            return base.Put(item, id1, id2, id3, id4, id5);
        }

    }
}
