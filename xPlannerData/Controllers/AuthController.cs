using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using xPlannerData.Interfaces;
using xPlannerData.Models;
using xPlannerData.Services;
using xPlannerData.Components.Authorization;
using System.Security.Claims;

namespace xPlannerData.Controllers
{
    public class AuthController : ApiController
    {

        // Login
        [AllowAnonymous]
        [Route("api/auth")]
        [HttpGet]
        public HttpResponseMessage Get(string username, string password)
        {
            using (IUserRepository userRepository = new UserRepository())
            {
                ApplicationUser user = userRepository.Login(Uri.UnescapeDataString(username), Uri.UnescapeDataString(password));

                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Username or password invalid");
                }

                // Get the roles of the user
                var roles = userRepository.GetRoles(user.Id);

                // Sets the token in the response
                AuthorizeUserAttribute.SetJwtTokenInHttpHeader(user.Id, user.UserName, string.Join(",", roles),
                    string.Join(",", userRepository.GetDomainsID(user.UserName)));

                // Return some user's data
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    id = user.Id,
                    username = user.UserName,
                    is_pass_temp = user.IsPasswordTemporary == null ? false : user.IsPasswordTemporary,
                    accept_user_license = user.accept_user_license,
                    isAdmin = roles.Contains("Administrator")
                });
            }
        }

        [HttpGet]
        [Route("api/auth/pass")]
        public HttpResponseMessage InsertPassword()
        {
            using (IUserRepository userRepository = new UserRepository())
            {

                List<AspNetUser> users = userRepository.GetAll();

                foreach (AspNetUser user in users)
                {
                    if (!userRepository.InsertPassword(user.Id, user.Password))
                    {
                        //return Request.CreateResponse(HttpStatusCode.InternalServerError);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Passwords inserted!");
            }
        }

        //[Route("api/auth/password")]
        //public HttpResponseMessage

        
        [Route("api/auth/register")]
        [HttpPost]
        public HttpResponseMessage Register(ApplicationUser user)
        {
            using (IUserRepository userRepository = new UserRepository())
            {
                // try to create the user
                var returnData = userRepository.Create(user);

                // verifies if the user has been create successfully
                if (returnData.Succeeded)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, user);
                }
                // username/email already exists
                else if (returnData.Errors.Any(e => e.Contains("is already taken")))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, returnData.Errors);
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, returnData.Errors);
            }
        }
    }
}