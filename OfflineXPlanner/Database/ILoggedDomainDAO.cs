using OfflineXPlanner.Domain;

namespace OfflineXPlanner.Database
{
    public interface ILoggedDomainDAO
    {
        DomainInfo GetChosenDomain();
        void StoreChosenDomain(DomainInfo domain);
    }
}
