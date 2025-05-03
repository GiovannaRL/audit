using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerCommon.Models.Enums;
using xPlannerCommon.Services;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.SecurityDomains)]
    public class DomainsController : TableGenericController<DomainResponse>
    {
        public DomainsController() : base(new[] { "domain_id" }, new string[] { }) { }

        private IEnumerable<DomainResponse> GetAllDomains() 
        {
            var manufacturerRepository = new TableRepository<manufacturer>();
            using (var repository = new TableRepository<domain>())
            {
                return repository.GetAll(new string[] { }, new int[] { }, new string[] { "manufacturers1", "AspNetUsers" })
                    .Select(d => new DomainResponse
                    {
                        created_at = d.created_at,
                        domain_id = d.domain_id,
                        name = d.name,
                        show_audax_info = d.show_audax_info,
                        user_id = UserId,
                        manufacturers = d.manufacturers1,
                        type = d.type,
                        enabled = d.enabled,
                        user_count = d.AspNetUsers.Count,
                        user_last_month = d.AspNetUsers.Where(x => x.LastLoginDate > DateTime.Today.AddMonths(-1) && x.LastLoginDate < DateTime.Today).Count(),
                        user_last_year = d.AspNetUsers.Where(x => x.LastLoginDate > DateTime.Today.AddYears(-1) && x.LastLoginDate < DateTime.Today).Count()

                    });
            }
        }

        [ActionName("Available")]
        [HttpGet]
        public IEnumerable<DomainResponse> GetAllowedForUser()
        {
            var allowedDomains = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type.ToLower().Equals("enterprise")).Select(c => c.Value).ToArray();
            var userRepository = new TableRepository<AspNetUser>();
            var roleRepository = new TableRepository<AspNetUserRole>();
            var acceptLicenseAgreement = userRepository.GetFirst(new string[] { "Id" }, new string[] { UserId }, null).accept_user_license;


            return GetAllDomains().Where(d => allowedDomains.Contains(d.domain_id.ToString()) && d.enabled == true)
                .Select(d =>
                {
                    d.role_id = Convert.ToInt32(roleRepository.GetFirst(new string[] { "UserId", "domain_id" },
                            new string[] { UserId, d.domain_id.ToString() }, null).RoleId);
                    d.accept_user_license = acceptLicenseAgreement;
                    d.user_id = UserId;
                    return d;
                });
        }

        [ActionName("All")]
        public override IEnumerable<DomainResponse> GetAll()
        {
            if (!IsLoggedAsAudaxware())
            {
                throw new HttpException(403, "Unauthorized");
            }

            return GetAllDomains();
        }

        [ActionName("Item")]
        public override DomainResponse GetItem(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<domain>())
            {
                var dom = repository.Get(new[] { "domain_id" }, new[] { id1 }, null);
                if (dom == null)
                    return null;

                return new DomainResponse
                {
                    created_at = dom.created_at,
                    domain_id = dom.domain_id,
                    name = dom.name,
                    show_audax_info = dom.show_audax_info
                };
            }
        }

        [ActionName("All")]
        public override IEnumerable<DomainResponse> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return GetAll();
        }

        private domain FiilPowerBIAndRLSInformation(domain item)
        {

            string nameWithoutSpaces = item.name.Replace(" ", "").ToLower();

            item.pb_workspace_collection = string.Format("pb-{0}", nameWithoutSpaces);
            item.pb_dataset_name = item.pb_dataset_name ?? nameWithoutSpaces;
            item.pb_workspace_id = item.pb_workspace_id ?? "none";
            item.pb_access_key = item.pb_access_key ?? "none";

            item.rls_user = item.rls_user ?? nameWithoutSpaces;
            item.rls_pwd = item.rls_pwd ?? "none";

            return item;
        }

        [HttpPost]
        [ActionName("Item")]
        public DomainResponse Post([FromBody] domain item)
        {
            // Only audaxware users can use this endpoint
            if (!IsLoggedAsAudaxware())
            {
                throw new HttpException(403, "Unauthorized");
            }

            using (var repository = new TableRepository<domain>())
            {
                item.domain_id = GetNextDomainID();
                item = FiilPowerBIAndRLSInformation(item);

                item.created_at = DateTime.Now;

                var manufacturers = item.manufacturers1;
                item.manufacturers1 = null;

                repository.Add(item);

                using (ProceduresRepository proceduresRepository = new ProceduresRepository())
                {
                    proceduresRepository.UpdateDomainManufacturers(item.domain_id, manufacturers);
                }

                return new DomainResponse
                {
                    domain_id = item.domain_id,
                    name = item.name,
                    manufacturers = manufacturers,
                    show_audax_info = item.show_audax_info,
                    created_at = item.created_at
                };
            }
        }


        [HttpPut]
        [ActionName("Item")]
        public override DomainResponse Put([FromBody]DomainResponse item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (!IsLoggedAsAudaxware())
            {
                throw new HttpException(403, "Unauthorized");
            }

            using (var repository = new TableRepository<domain>())
            {
                using (ProceduresRepository proceduresRepository = new ProceduresRepository())
                {
                    domain dom = repository.Get(new string[] { "domain_id" }, new int[] { id1 }, null);

                    dom.name = item.name ?? dom.name;
                    dom.show_audax_info = item.show_audax_info;
                    dom.type = item.type ?? dom.type;
                    dom.enabled = item.enabled;

                    repository.Update(dom);

                    item.manufacturers = dom.type == EnterpriseTypeEnum.Enterprise ? null : item.manufacturers;

                    // If the domain type is `Enterprise` this function will delete all the manufacturers of the domain
                    proceduresRepository.UpdateDomainManufacturers(dom.domain_id, item.manufacturers);

                    return item;
                }
            }
        }

        private short GetNextDomainID()
        {
            return (short)(GetAllDomains().Max(d => d.domain_id) + 1);
        }
    }
}
