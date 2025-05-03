using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerAPI.Models;
using System;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.SecurityUsers)]
    public class AuditController : AudaxWareController
    {
        
        [ActionName("All")]
        public IEnumerable<get_all_audit_data_Result> GetAll(int id1)
        {
            using (var repository = new AuditRepository())
            {
                return repository.AllAuditData(id1);
            }
        }


        [ActionName("GetItem")]
        public IEnumerable<AuditData> GetItem(int id1, int id2)
        {
            using (var repository = new AuditRepository())
            {
                return repository.GetAuditData(id2);
            }
        }

    }
}
