using System.Web.Http;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using xPlannerAPI.Security.Controller;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;
using xPlannerAPI.Services;
using xPlannerAPI.Security.Extensions;
using xPlannerCommon.Models.Enums;

namespace xPlannerAPI.Controllers
{

    [Authorize]
    public class AudaxWareController : ApiController
    {
        protected ClaimsIdentity AudaxWareIdentity
        {
            get
            {
                return (ClaimsIdentity)User.Identity;
            }
        }

        protected string UserId
        {
            get
            {
                return User.Identity.GetUserId();
            }
        }

        protected string UserName
        {
            get
            {
                return User.Identity.Name;
            }
        }

        protected bool HasDomainAccess(int domainId)
        {
            return AudaxWareIdentity.CheckDomainAccess(domainId);
        }

        protected bool HasManufacturerAccess(short manufacturerDomainId, int manufacturerId)
        {
            return AudaxWareIdentity.CheckManufacturerAccess(GetLoggedDomainId(), manufacturerDomainId, manufacturerId);
        }

        protected bool ShowAudaxWareInfo(int domainId)
        {
            return HasDomainAccess(domainId) && Helper.ShowAudaxWareInfo(domainId);
        }

        protected AspNetRole GetUserRole(short domainId)
        {
            if (UserId == null) return null;

            using (var rep = new AspNetUserRoleRepository())
            {
                return rep.GetRole(domainId, UserId);
            }
        }

        public bool SetDomain(int domainId)
        {
            var authorization = System.Web.HttpContext.Current.Request.Headers["Authorization"];
            if (authorization == null)
            {
                return false;
            }
            UserData userData;
            if (Helper.TokenData.TryGetValue(authorization.Replace("Bearer", "").Trim(), out userData) && userData.loggedDomain != null &&
                userData.loggedDomain.domain_id == domainId)
            {
                return true;
            }
            // Sets the domain when we have notification requests. This will ensure the domain is properly
            // restablished in case the server is restarted
            using (var domainRep = new TableRepository<domain>())
            {
                var domain = domainRep.Get(new [] { "domain_id" }, new [] { domainId }, null);
                if (domain == null)
                {
                    return false;
                }
                var accountController = new AccountController();
                return accountController.PutLoggedDomain(domain) == Ok();
            }
        }

        protected bool IsLoggedAsAudaxware()
        {
            return Helper.GetDecryptedToken()?.loggedDomain?.domain_id == Helper.AudaxwareDomainId;
        }

        protected bool isLoggedAsManufacturer() {

            return Helper.GetDecryptedToken()?.loggedDomain?.type == EnterpriseTypeEnum.Manufacturer;
        }

        protected short GetLoggedDomainId()
        {
            return Helper.GetDecryptedToken()?.loggedDomain?.domain_id ?? 0;
        }
    }
}