using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.UserGridViews)]
    public class GridViewController : AudaxWareController
    {
        private static readonly string[] KeyFields = {"added_by", "type", "name", "domain_id"};
        private static readonly string[] AllFields = { "added_by", "type", "domain_id" };

        [ActionName("All")]
        public List<User_gridView> GetAll(string id1, int id2)
        {
            using (var repository = new GridViewRepository())
            {
                var test = UserName;
                return repository.GetAll(UserName, id1, id2);
            }
        }

        [ActionName("Item")]
        public HttpResponseMessage Get(string id1, string id2, int id3)
        {
            try {
                using (var repository = new GridViewRepository())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, repository.Get(UserName, id1, id2, id3));
                }
            }
            catch (ObjectNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }


        [ActionName("Item")]
        public HttpResponseMessage Post([FromBody]User_gridView item, int id1, string id2)
        {
            try
            {
                using (var repository = new TableRepository<User_gridView>())
                {
                    item.user_id = UserId;
                    item.added_by = UserName;
                    var views = repository.GetAll(new[] {"domain_id"}, new[] { id1 }, null);
                    if ( !item.is_private && views.Any(x => x.added_by != item.added_by && x.name == item.name && !x.is_private))
                        return Request.CreateResponse(HttpStatusCode.Conflict, "There is another Shared View created with the same name.Please choose another name.");

                    var view = repository.Add(item);
                    return Request.CreateResponse(HttpStatusCode.OK, view);
                }
            }
            catch (Exception e)
            {
                var test = e;
                return null;
            }    
        }

        [ActionName("Item")]
        public User_gridView Put([FromBody] User_gridView item, int id1, string id2)
        {
            using (var repository = new TableRepository<User_gridView>())
            {
                var oldView = repository.Get(new[] { "domain_id", "gridview_id" }, new[] { id1, item.gridview_id }, null);
                if (oldView.added_by == UserName)
                {
                    oldView.grid_state = item.grid_state;
                    oldView.consolidated_columns = item.consolidated_columns;
                    oldView.user_id = UserId;
                    oldView.type = id2;
                    oldView.is_private = item.is_private;
                    if (repository.Update(oldView))
                        return oldView;
                }

                var isAdmin = GetUserRole((short)id1).Id == "1";
                if (isAdmin)
                {
                    oldView.grid_state = item.grid_state;
                    if (repository.Update(oldView))
                        return oldView;
                }
            }

            return null;
        }

        [HttpPost]
        [ActionName("Item")]
        public HttpResponseMessage Delete([FromBody] User_gridView item, string id1)
        {
            try
            {
                using (var repository = new TableRepository<User_gridView>())
                {
                    
                    if (!item.is_private && item.added_by != UserName)
                    {
                        if (GetUserRole((short)item.domain_id).Id == "1")
                        {
                            var gridName = item.name.Remove(item.name.Length - 9);
                            repository.Delete(KeyFields, new[] { item.added_by, id1, gridName, item.domain_id.ToString() });
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        return Request.CreateResponse(HttpStatusCode.Conflict, "You cannot delete shared views.");
                    }
                        

                    var gridView = repository.Get(KeyFields, new[] { UserName, id1, item.name, item.domain_id.ToString() }, null);
                    repository.Delete(KeyFields, new[] { UserName, id1, item.name, item.domain_id.ToString() });
                    return Request.CreateResponse(HttpStatusCode.OK);

                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error in GridViewController:Delete. ErrorMessage: {e.Message}. InnerException: {e.InnerException}");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error to try delete de view, please contact the technical support");
            }
        }
    }
}
