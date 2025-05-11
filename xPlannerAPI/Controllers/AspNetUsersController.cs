using System;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Services;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.UsersLicense)]

    public class AspNetUsersController : AudaxWareController
    {

        [ActionName("AcceptLicenseAgreement")]
        public HttpResponseMessage PutAcceptLicenseAgreement(short id1, string id2)
        {
            using (var repository = new AspNetUserRoleRepository())
            {
                return repository.PutAcceptLicenseAgreement(id2);
            }
        }
    }
}
