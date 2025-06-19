using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class AspNetUserRoleRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public AspNetUserRoleRepository()
        {
            this._db = new audaxwareEntities();
        }

        public string GetId(short domain_id, string user_name)
        {
            var role = this._db.AspNetUserRoles.Include("AspNetUser").Where(x => x.domain_id == domain_id && x.AspNetUser.UserName == user_name).FirstOrDefault();
            if (role == null)
            {
                return "";
            }
            return role.RoleId;
        }

        public AspNetRole GetRole(short domain_id, string userId)
        {
            var role = this._db.AspNetUserRoles.Include("AspNetRole").Where(x => x.domain_id == domain_id && x.UserId.Equals(userId)).Select(ur => ur.AspNetRole).FirstOrDefault();
            if (role == null)
            {
                // Return viewer role (lowest access level)
                return this._db.AspNetRoles.Where(ur => ur.Name.ToLower().Equals("viewers")).FirstOrDefault();
            }

            return role;
        }


        public AspNetUserRole SetRole(AspNetUserRole role)
        {
            var user_role = this._db.AspNetUserRoles.Where(x => x.domain_id == role.domain_id && x.UserId == role.UserId).ToList();
            if (user_role != null)
            {
                foreach (var item in user_role)
                {
                    _db.Entry(item).State = EntityState.Deleted;
                    _db.SaveChanges();
                }

            }
            role.AspNetUser = null;
            _db.Entry(role).State = EntityState.Added;
            _db.SaveChanges();

            return role;
        }

        public HttpResponseMessage PutAcceptLicenseAgreement(string userId)
        {
            try
            {
                var user = this._db.AspNetUsers.Where(x => x.Id == userId).FirstOrDefault();
                user.accept_user_license = true;
                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error on AspNetUserRepository:PutAcceptLicenseAgreement. ErrorMessage: {e.Message}. InnerException: {e.InnerException}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }



        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}