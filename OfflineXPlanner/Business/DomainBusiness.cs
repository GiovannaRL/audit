using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Facade;
using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Security;
using OfflineXPlanner.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfflineXPlanner.Business
{
    public class DomainBusiness
    {
        public static List<DomainsRequestResponse> GetDomains(bool refreshFromAudaxware)
        {
            var domains = SecurityUtil.GetDomains();

            if (!SecurityUtil.IsLogged() || ListUtil.isEmptyOrNull(domains) || refreshFromAudaxware)
            {
                domains = DomainFacade.GetDomains();
                SecurityUtil.SetAvailableDomains(domains);
            }

            return domains;
        }

        public static bool AddLoggedDomain(DomainsRequestResponse selectedDomain)
        {
            var toReturn = DomainFacade.AddLoggedDomain(selectedDomain);
             AudaxwareRestApiInfo.loggedDomain = new DomainInfo(selectedDomain);
            return toReturn;
        }

        public static bool LoadChosenDomain(bool displayDomainsSelect)
        {
            ILoggedDomainDAO parametroSistemDAO = new LoggedDomainDAO();

            var loggedDomain = parametroSistemDAO.GetChosenDomain();
            if (loggedDomain == null && AudaxwareRestApiInfo.loggedDomain != null)
            {
                DomainBusiness.StoreChosenDomain(AudaxwareRestApiInfo.loggedDomain);
            }
            else
            {
                AudaxwareRestApiInfo.loggedDomain = loggedDomain;
            }
            if (AudaxwareRestApiInfo.loggedDomain == null && displayDomainsSelect)
            {
                using (SelectDomain selectDomainForm = new SelectDomain())
                {
                    return selectDomainForm.ShowDialog() == DialogResult.OK;
                }
            }

            if (AudaxwareRestApiInfo.loggedDomain != null)
            {
                return AddLoggedDomain(new DomainsRequestResponse
                {
                    created_at = AudaxwareRestApiInfo.loggedDomain.created_at,
                    domain_id = (short)AudaxwareRestApiInfo.loggedDomain.domain_id,
                    name = AudaxwareRestApiInfo.loggedDomain.domain_name,
                    role_id = AudaxwareRestApiInfo.loggedDomain.role_id,
                    show_audax_info = AudaxwareRestApiInfo.loggedDomain.show_audax_info
                });
            }

            return false;
        }

        public static void StoreChosenDomain(DomainInfo domain)
        {
            ILoggedDomainDAO parametroSistemDAO = new LoggedDomainDAO();
            parametroSistemDAO.StoreChosenDomain(domain);
        }
    }
}
