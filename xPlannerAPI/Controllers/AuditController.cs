using System;
using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using static iTextSharp.text.pdf.AcroFields;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.SecurityUsers)]
    public class AuditController : AudaxWareController
    {

        [ActionName("All")]
        public IEnumerable<AuditWithChanges> GetAll(int id1)
        {
            using (var repository = new AuditRepository())
            {
                var result = repository.AllAuditDataWithChanges(id1);

                foreach (var item in result)
                {
                    Console.WriteLine("Original: " + item.original_fields);
                    Console.WriteLine("Modified: " + item.changed_fields);
                }

                return result;
            }
        }


        [ActionName("AllWithChanges")]
        public IEnumerable<AuditWithChanges> GetAllWithChanges(int id1)
        {
            using (var repository = new AuditRepository())
            {
                var result = repository.AllAuditDataWithChanges(id1);
                foreach (var item in result)
                {

                }

                return repository.AllAuditDataWithChanges(id1);
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