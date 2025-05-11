using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class PurchaseOrderExpiratedController : AudaxWareController
    {

        [ActionName("All")]
        public IEnumerable<get_expirated_pos_Result> GetAll(short id1)
        {
            using (IPurchaseOrderRepository repository = new PurchaseOrderRepository())
            {
                return repository.GetExpirated((short)id1, UserId).ToList();
            }
        }
    }
}
