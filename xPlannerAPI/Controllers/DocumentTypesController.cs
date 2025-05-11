using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfo)]
    public class DocumentTypesController : TableGenericController<document_types>
    {
        public DocumentTypesController() : base(new string[] { }, new [] { "id" }) { }
    }
}
