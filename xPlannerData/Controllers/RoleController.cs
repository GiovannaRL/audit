using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using xPlannerData.Components.Authorization;
using xPlannerData.Models;

namespace xPlannerData.Controllers
{
    public class RoleController : ApiController
    {
        
        [Route("api/role/{role_name}")]
        public async Task<HttpResponseMessage> Post(string role_name)
        {
            var manager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new AuthContext()));
            var roleresult = await manager.CreateAsync(new IdentityRole(role_name));

            if (roleresult.Succeeded)
            {
                return Request.CreateResponse(HttpStatusCode.OK, roleresult);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, roleresult.Errors);
        }
    }
}
