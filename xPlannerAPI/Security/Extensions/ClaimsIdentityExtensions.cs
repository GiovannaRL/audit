using System.Linq;
using System.Security.Claims;

namespace xPlannerAPI.Security.Extensions
{
    public static class ClaimsIdentityExtensions
    {
         public static bool CheckDomainAccess(this ClaimsIdentity identity, int domainId)
        {
            return identity.HasClaim(claim => claim.Type == "Enterprise" && claim.Value == domainId.ToString());
        }

        public static bool CheckProjectAccess(this ClaimsIdentity identity, int domainId, int projectId)
        {
            var projectsClaim = identity.FindFirst($"{domainId}.Projects");
            if (projectsClaim == null)
                return false;
            // User has access to all projects (Administrators)
            if (projectsClaim.Value == "*")
                return true;
            var projectIds = projectsClaim.Value.Split(',');
            return projectIds.Contains(projectId.ToString());
        }

        public static bool CheckManufacturerAccess(this ClaimsIdentity identity, short domainId, short manufacturerDomainId, int manufacturerId)
        {
            return identity.HasClaim(claim => claim.Type.Equals($"{domainId}.Manufacturer") && claim.Value.Equals($"{manufacturerDomainId};{manufacturerId}"));
        }
    }
}