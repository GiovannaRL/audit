using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class NotificationController : TableGenericController<user_notification>
    {
        public NotificationController() : 
            base(new [] { "domain_id", "id" }, new [] { "domain_id" }) { }

        [ActionName("All")]
        public override IEnumerable<user_notification> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (SetDomain(id1))
                return base.GetAll(id1, id2, id3, id4, id5)?.Where(un => un.userId == UserId);

            System.Web.HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        [ActionName("Read")]
        public HttpResponseMessage Read([FromBody]int[] ids, int id1)
        {
            foreach (var id in ids)
            {
                base.Delete(id1, id);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
