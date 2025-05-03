using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using xPlannerAPI.Services;
using xPlannerAPI.Security.Extensions;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;

namespace xPlannerAPI.Security.Attributes
{
    public class AudaxAuthorizeProjectAttribute : ActionFilterAttribute
    {
        public string DomainIdParam { get; set; }
        public string ProjectIdParam { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var identity = HttpContext.Current.User?.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
            }
            try
            {
                var domainId = Convert.ToInt16(actionContext.ActionArguments[DomainIdParam ?? "id1"]);
                var projectId = Convert.ToInt32(actionContext.ActionArguments[ProjectIdParam ?? "id2"]);
                if (!identity.CheckProjectAccess(domainId, projectId))
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
                }
            }
            catch (Exception)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
            }
            base.OnActionExecuting(actionContext);
        }
    }
}