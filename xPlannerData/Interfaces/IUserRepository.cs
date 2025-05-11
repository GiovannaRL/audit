
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerData.Models;

namespace xPlannerData.Interfaces
{
    interface IUserRepository : IDisposable
    {
        ApplicationUser Login(string username, string password);
        List<int> GetDomainsID(string username);
        IdentityResult Create(ApplicationUser user);
        IdentityResult AddToRoles(string userId, string[] roles);
        IList<string> GetRoles(string userId);
        bool InsertPassword(string userId, string password);
        List<AspNetUser> GetAll();
    }
}
