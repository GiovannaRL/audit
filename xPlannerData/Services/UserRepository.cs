using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using xPlannerData.Interfaces;
using xPlannerData.Models;

namespace xPlannerData.Services
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private audaxwareDB db;
        private UserManager<ApplicationUser> manager;
        private bool disposed = false;

        public UserRepository()
        {
            this.db = new audaxwareDB();
            this.manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
        }

        // get all the users
        public List<AspNetUser> GetAll()
        {
            try
            {
                return this.db.AspNetUsers.ToList();
            }
            catch (Exception ex)
            {
                Helper.RecordLog("UserRepository", "GetAll", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        public ApplicationUser Login(string username, string password)
        {
            try
            {
                return this.manager.Find(username, password);
            }
            catch (Exception ex)
            {
                Helper.RecordLog("UserRepository", "Login", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        // get the domains of user
        public List<int> GetDomainsID(string username)
        {
            var domain = username.Split('@').Last();
            domain dm = this.db.domains.Where(d => d.name.Equals(domain)).FirstOrDefault();


            List<int> returnDomains = new List<int>();

            if (dm != null)
            {
                if (dm.show_audax_info && dm.domain_id != 1)
                {
                    returnDomains.Add(1);
                }
                returnDomains.Add(dm.domain_id);
            }

            return returnDomains;
        }

        public IdentityResult Create(ApplicationUser user)
        {
            try
            {
                user.CreationDate = DateTime.Now;
                user.IsPasswordTemporary = true;

                IdentityResult returnData =  this.manager.Create(user, user.Password);

                if (returnData.Succeeded)
                {
                   return this.AddToRoles(user.Id, user.roleNames.ToArray());
                }

                return returnData;
            }
            catch (Exception ex)
            {
                Helper.RecordLog("UserRepository", "Create", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        public IList<string> GetRoles(string userId)
        {
            try
            {
                return this.manager.GetRoles(userId);
            }
            catch (Exception ex)
            {
                Helper.RecordLog("UserRepository", "GetRoles", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        public IdentityResult AddToRoles(string userId, string[] roles)
        {
            try
            {
                return this.manager.AddToRoles(userId, roles);
            }
            catch (Exception ex)
            {
                Helper.RecordLog("UserRepository", "AddToRoles", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        public bool InsertPassword(string userId, string password)
        {
            try
            {
                var result = this.manager.AddPassword(userId, password);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Helper.RecordLog("UserRepository", "InsertPassword", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}