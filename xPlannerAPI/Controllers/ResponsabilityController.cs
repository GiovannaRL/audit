using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class ResponsabilityController : TableGenericController<responsability>
    {
        public ResponsabilityController() : base(new [] { "domain_id", "name" }, new [] { "domain_id" }, true) { }

        [ActionName("New")]
        public IEnumerable<responsability> GetNew(int id1)
        {
            return base.GetAll(id1).Where(r => r.isNew).OrderBy(x => x.domain_id).ThenBy(x => x.name);
        }

        [ActionName("Description")]
        public UniqueString GetDescription(int id1)
        {
            var uniqueString = new UniqueString
            {
                text = base.GetAll(id1).OrderBy(x => x.domain_id).ThenBy(x => x.name)
                    .Aggregate("", (x, y) => x + y.name + ": " + y.description + "\n")
            };
            return uniqueString;
        }

    }
}
