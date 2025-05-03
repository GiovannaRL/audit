using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Infrastructure;
using xPlannerAPI.Services;
using xPlannerCommon.Models;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Services;
using xPlannerCommon.Models.Enums;

namespace xPlannerAPI.Security.Models
{

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        static ClaimsIdentity viewerClaims = new ClaimsIdentity();
        static ClaimsIdentity plannerClaims = new ClaimsIdentity();
        static ClaimsIdentity adminClaims = new ClaimsIdentity();
        static ClaimsIdentity audaxWareBasicClaims = new ClaimsIdentity();
        static ClaimsIdentity manufacturerAdminClaims = new ClaimsIdentity();
        static ClaimsIdentity manufacturerPlannerClaims = new ClaimsIdentity();
        static ClaimsIdentity manufacturerBasicClaims = new ClaimsIdentity();
        static void InsertRoleClaim(ClaimsIdentity claims, AreaModule area, DataAccess access)
        {
            claims.AddClaim(new Claim(System.Enum.GetName(typeof(AreaModule), area),
               System.Enum.GetName(typeof(DataAccess), access)));
        }

        static void InsertUserClaim(ClaimsIdentity userClaims, ClaimsIdentity roleClaims, int domain_id)
        {
            InsertUserClaim(userClaims, roleClaims, domain_id, true);
        }
        static void InsertOrReplaceClaim(ClaimsIdentity userClaims, Claim claim)
        {
            var existingClaim = userClaims.Claims.SingleOrDefault(c => c.Type == claim.Type);
            if (existingClaim != null)
            {
                userClaims.RemoveClaim(existingClaim);
            }
            userClaims.AddClaim(claim);
        }
        static void InsertUserClaim(ClaimsIdentity userClaims, ClaimsIdentity roleClaims, int domain_id, bool addEnterprise)
        {
            if (addEnterprise)
            {
                userClaims.AddClaim(new Claim("Enterprise", domain_id.ToString()));
            }
            foreach (var claim in roleClaims.Claims)
            {
                InsertOrReplaceClaim(userClaims, new Claim($"{domain_id}.{claim.Type}", claim.Value));
            }
        }

        static ApplicationUser()
        {
            // Manufacturer Planner
            InsertRoleClaim(manufacturerPlannerClaims, AreaModule.AssetsCatalogAssets, DataAccess.Edit);
            InsertRoleClaim(manufacturerPlannerClaims, AreaModule.AssetsCatalogCategories, DataAccess.Edit);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.AssetsCatalogVendors, DataAccess.Edit);

            // Manufacturer Admin
            InsertRoleClaim(manufacturerAdminClaims, AreaModule.AssetsCatalogAssets, DataAccess.Edit);
            InsertRoleClaim(manufacturerAdminClaims, AreaModule.AssetsCatalogCategories, DataAccess.Edit);
            InsertRoleClaim(manufacturerAdminClaims, AreaModule.SecurityUsers, DataAccess.Edit);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.AssetsCatalogVendors, DataAccess.Edit);

            // Manufacturer Basics
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.AssetsCatalogAssets, DataAccess.View);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.AssetsCatalogCategories, DataAccess.View);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.AssetsCatalogManufacturers, DataAccess.View);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.AssetsCatalogVendors, DataAccess.View);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.UsersLicense, DataAccess.Edit);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.NotLoggedArea, DataAccess.Edit);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.GeneralInfoDomain, DataAccess.View);
            InsertRoleClaim(manufacturerBasicClaims, AreaModule.GeneralInfo, DataAccess.View);
            InsertRoleClaim(manufacturerAdminClaims, AreaModule.SecurityDomains, DataAccess.NoAccess);

            // Viewer
            InsertRoleClaim(viewerClaims, AreaModule.ProjectsDetails, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.ProjectsAssets, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.ProjectsReports, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.ProjectsPurchaseOrders, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.ProjectsDocuments, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.ProjectsITConnectivity, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.Other, DataAccess.Edit);
            InsertRoleClaim(viewerClaims, AreaModule.GeneralInfoDomain, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.GeneralInfo, DataAccess.View);
            InsertRoleClaim(viewerClaims, AreaModule.NotLoggedArea, DataAccess.Edit);
            InsertRoleClaim(viewerClaims, AreaModule.UsersLicense, DataAccess.Edit);


            // Planner
            InsertRoleClaim(plannerClaims, AreaModule.ProjectsDetails, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.ProjectsAssets, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.ProjectsReports, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.ProjectsPurchaseOrders, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.ProjectsDocuments, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.ProjectsITConnectivity, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.AssetsCatalogAssets, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.AssetsCatalogCategories, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.AssetsCatalogVendors, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.AssetsCatalogManufacturers, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.DomainTemplates, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.GeneralInfoDomain, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.GeneralInfo, DataAccess.View);
            InsertRoleClaim(plannerClaims, AreaModule.Other, DataAccess.View);
            InsertRoleClaim(plannerClaims, AreaModule.NotLoggedArea, DataAccess.Edit);
            InsertRoleClaim(plannerClaims, AreaModule.UsersLicense, DataAccess.Edit);

            // Admins
            InsertRoleClaim(adminClaims, AreaModule.ProjectsDetails, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.ProjectsAssets, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.ProjectsReports, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.ProjectsPurchaseOrders, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.ProjectsDocuments, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.ProjectsITConnectivity, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.AssetsCatalogAssets, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.AssetsCatalogCategories, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.AssetsCatalogVendors, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.AssetsCatalogManufacturers, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.DomainTemplates, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.GeneralInfoDomain, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.GeneralInfo, DataAccess.View);
            InsertRoleClaim(adminClaims, AreaModule.Other, DataAccess.View);
            InsertRoleClaim(adminClaims, AreaModule.NotLoggedArea, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.SecurityRights, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.SecurityUsers, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.SecurityDomains, DataAccess.Edit);
            InsertRoleClaim(adminClaims, AreaModule.UsersLicense, DataAccess.Edit);

            InsertRoleClaim(audaxWareBasicClaims, AreaModule.AssetsCatalogAssets, DataAccess.Edit);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.AssetsCatalogCategories, DataAccess.View);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.AssetsCatalogManufacturers, DataAccess.View);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.AssetsCatalogVendors, DataAccess.View);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.DomainTemplates, DataAccess.View);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.GeneralInfo, DataAccess.View);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.GeneralInfoDomain, DataAccess.View);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.NotLoggedArea, DataAccess.Edit);
            InsertRoleClaim(audaxWareBasicClaims, AreaModule.UsersLicense, DataAccess.Edit);
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            var rolesTable = new TableRepository<AspNetUserRole>();
            var roles = rolesTable.GetAll(new string[] { "UserId" }, new string[] { this.Id }, new string[] { "AspNetRole", "domain" });
            var projectUserTable = new TableRepository<project_user>();

            var projects = projectUserTable.GetAll(new string[] { "user_pid" }, new string[] { this.Id }, null);
            bool isInAudaxWareDomain = false;
            //var roles = rolesService.GetAll(new string[] { "UserId" }, new string[] { this.Id }, new string[] { "RoleId", "AspNetRole", "domain_id" });
            foreach (var role in roles)
            {
                if (role.domain.type == EnterpriseTypeEnum.Manufacturer)
                {
                    AddManufacturerClaims(userIdentity, role);
                }
                else
                {
                    AddEnterpriseClaims(userIdentity, projects, role);

                    if (role.domain_id == 1)
                        isInAudaxWareDomain = true;

                    if (!isInAudaxWareDomain)
                    {
                        InsertUserClaim(userIdentity, audaxWareBasicClaims, 1, false);
                    }
                }                
            }

            
            return userIdentity;
        }

        private static void AddManufacturerClaims(ClaimsIdentity userIdentity, AspNetUserRole role)
        {
            InsertUserClaim(userIdentity, manufacturerBasicClaims, role.domain_id, true);

            switch (role.AspNetRole.Name)
            {
                case "Planners":
                    InsertUserClaim(userIdentity, manufacturerPlannerClaims, role.domain_id, false);
                    break;
                case "Administrators":
                    InsertUserClaim(userIdentity, manufacturerAdminClaims, role.domain_id, false);
                    break;
            }
                        

            using (ProceduresRepository procedures = new ProceduresRepository())
            {
                var manufacturers = procedures.GetAssociatedManufacturers(role.domain_id);

                foreach (var manufacturer in manufacturers) {
                    userIdentity.AddClaim(new Claim($"{role.domain_id}.Manufacturer", string.Join(";", manufacturer.manufacturer_domain_id, manufacturer.manufacturer_id)));
                }
            }
        }

        private static void AddEnterpriseClaims(ClaimsIdentity userIdentity, System.Collections.Generic.List<project_user> projects, AspNetUserRole role)
        {
            var roleClaims = new ClaimsIdentity();

            switch (role.AspNetRole.Name)
            {
                case "Viewers":
                    roleClaims = viewerClaims;
                    break;
                case "Planners":
                    roleClaims = plannerClaims;
                    break;
                case "Administrators":
                    roleClaims = adminClaims;
                    break;
            }

            InsertUserClaim(userIdentity, roleClaims, role.domain_id);
            if (role.AspNetRole.Name == "Administrators")
            {
                // Administrators can see all projects
                userIdentity.AddClaim(new Claim($"{role.domain_id}.Projects", "*"));
            }
            else
            {
                var domainProjects = from p in projects where p.project_domain_id == role.domain_id select p.project_id;
                userIdentity.AddClaim(new Claim($"{role.domain_id}.Projects", String.Join(",", domainProjects)));
            }
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 300;
        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}