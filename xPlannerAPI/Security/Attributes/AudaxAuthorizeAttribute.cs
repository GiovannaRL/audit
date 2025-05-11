using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using xPlannerCommon.Enumerators;
using xPlannerAPI.Security.Extensions;
using xPlannerCommon.App_Data;

namespace xPlannerAPI.Security.Attributes
{
    public class AudaxAuthorizeAttribute : ActionFilterAttribute
    {
        public AreaModule AreaModule { get; set; }
        public string DomainIdParam { get; set; }
        public string ProjectIdParam { get; set; }
        private static readonly AreaModule[] CheckProjectIdAreas = {
            AreaModule.ProjectsAssets, AreaModule.ProjectsReports, AreaModule.ProjectsDetails, AreaModule.ProjectsPurchaseOrders,
            AreaModule.ProjectsDocuments, AreaModule.ProjectsITConnectivity
        };

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (AreaModule == AreaModule.NotLoggedArea)
            {
                base.OnActionExecuting(actionContext);
                return;
            }

            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
            }
            var verb = HttpContext.Current.Request.HttpMethod.ToUpper();


            // These controllers only allow the user to retrieve data. Domain is not required
            // but the only action allowed is GET
            if (AreaModule == AreaModule.GeneralInfo || AreaModule == AreaModule.SecurityDomains)
            {
                if (verb == "GET")
                {
                    base.OnActionExecuting(actionContext);
                    return;
                // Check if it's a audaxware user and he has write permissions to the controller
                } else if (AreaModule == AreaModule.SecurityDomains) {
                    var audaxClaim = identity?.Claims
                        .SingleOrDefault(c => c.Type.Equals($"{Helper.AudaxwareDomainId}.{AreaModule.ToString()}"));
                    if (audaxClaim.Value.Equals(DataAccess.Edit.ToString())) {
                        base.OnActionExecuting(actionContext);
                        return;
                    }
                }

                throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
            }

            // This is the user capability to update information related to him,
            // such as the Grid Views
            if (AreaModule == AreaModule.UserGridViews)
            {
                // This is a programming error. You shouldnot create a parameter called UserId for the profile functions
                // as you can retrieve the user in the controller based on the logged in identity
                if (actionContext.ActionArguments.ContainsKey("UserId"))
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
                }
                base.OnActionExecuting(actionContext);
                return;
            }

            // If no user or no domain_id property user is forbidden
            if (!actionContext.ActionArguments.ContainsKey(DomainIdParam ?? "id1"))
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
            }

            // Verify if user has access to the domain
            var domainId = Convert.ToInt16(actionContext.ActionArguments[DomainIdParam ?? "id1"]);
            // Users might not belong to the AudaxWare enterprise, but they can query for data. The protection
            // on Accessing from AudaxWare is based on the claims, but the user might not have access to the Audaxware Enterprise
            // So we let the domain 1 pass for further validation
            if (domainId != 1 && (domainId <= 0 || !identity.CheckDomainAccess(domainId)))
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
            }



            // Get user's access claims to this domain
            var claim = identity?.Claims.SingleOrDefault(c => c.Type.Equals($"{domainId}.{AreaModule.ToString()}"));

            // User has not access to AudaxWare domain, but he is trying to see templates
            // In this case we will have ProjectAssets request for domainId 1, which will 
            // cause a problem. In the future we might put this on differen security group
            // This access will allow everyone to query projects from AudaxWare domain using API
            // which should not be a problem as none of those are supposed to be real projects
            if (claim == null && (AreaModule == AreaModule.ProjectsAssets && domainId == 1))
            {
                if (verb == "GET")
                    base.OnActionExecuting(actionContext);
                else
                    throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
                return;
            }

            // verify if the claim is not 'NoAccess' type and when not GET verb if it is 'Edit' type
            if (claim == null || !verb.Equals("GET") && !claim.Value.Equals(DataAccess.Edit.ToString()))
                throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);

            if (CheckProjectIdAreas.Contains(AreaModule))
            {
                // Verify if user has access to the domain
                var projectId = Convert.ToInt32(actionContext.ActionArguments[ProjectIdParam ?? "id2"]);
                if (!identity.CheckProjectAccess(domainId, projectId) && projectId > 0) //if project = 0 means it's a new project
                    throw new HttpResponseException(System.Net.HttpStatusCode.Forbidden);
            }
            base.OnActionExecuting(actionContext);
        }
    }
}