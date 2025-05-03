using System;
using System.Data.Entity;
using System.Linq;
using xPlannerCommon.Models;

namespace xPlannerAPI.Security.Services
{
    public class LoginService : IDisposable
    {
        private audaxwareEntities db;
        private bool disposed = false;

        public LoginService()
        {
            this.db = new audaxwareEntities();
        }

        public void UpdateLogin(string userName)
        {
            var user = this.db.AspNetUsers.Where(x => x.UserName == userName).FirstOrDefault();
            user.LastLoginDate = DateTime.Now;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
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