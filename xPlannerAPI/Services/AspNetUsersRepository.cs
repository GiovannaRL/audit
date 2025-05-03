using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web.Http;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class AspNetUsersRepository : ApiController
    {
        private readonly audaxwareEntities _db;

        public AspNetUsersRepository()
        {
            _db = new audaxwareEntities();
        }
        public string GetLoggedUserEmail()
        {
            return User.Identity.Name;
        }

        public string GetLoggedUserId()
        {
            return User.Identity.GetUserId();
        }

        public string GetLoggedUserName()
        {
            using (var _db = new audaxwareEntities())
            {
                var user = _db.AspNetUsers.Where(x => x.Id == User.Identity.GetUserId()).FirstOrDefault();
                return user.first_name.Trim() + " " + user.last_name.Trim();
            }
        }

       
    }
}