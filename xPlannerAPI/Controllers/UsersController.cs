using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using xPlannerAPI.Services;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using xPlannerAPI.Models;
using xPlannerAPI.Security;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Security.Models;
using xPlannerCommon.Enumerators;
using Microsoft.AspNet.Identity;
using xPlannerCommon.App_Data;
using HelperAPI = xPlannerAPI.App_Data.Helper;

namespace xPlannerAPI.Controllers
{

    [AudaxAuthorize(AreaModule = AreaModule.SecurityUsers)]
    public class UsersController : TableGenericController<AspNetUser>
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public UsersController() : base(new[] { "Id" }, new[] { "domain_id" }, new[] { "project_user", "AspNetUserRoles" }) { }

        [ActionName("All")]
        public override IEnumerable<AspNetUser> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<AspNetUser>())
            {
                var users = repository.GetAll(new string[] { }, GetIds(null), new[] { "project_user", "AspNetuserRoles" })
                    .Where(u => u.AspNetUserRoles.Any(uc => uc.domain_id == id1)).ToList();

                users.ForEach(delegate (AspNetUser user)
                {
                    user.PasswordHash = "";
                    user.AspNetUserRoles = user.AspNetUserRoles.Where(ur => ur.domain_id == id1).ToList();
                });
                return users;
            }
        }

        [ActionName("AllWithRole")]
        public IEnumerable<AspNetUserWithRole> GetAllWithRole(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<AspNetUser>())
            {
                var users = GetAll(id1);
                var usersWithRole = new List<AspNetUserWithRole>();
                var repRoles = new TableRepository<AspNetRole>();
                var roles = repRoles.GetAll(new string[] { }, GetIds(null), new string[] { });

                foreach (var user in users)
                {
                    user.AspNetUserRoles = user.AspNetUserRoles.Where(ur => ur.domain_id == id1).ToList();

                    var userRole = new AspNetUserWithRole { aspNetUser = user };
                    //user role
                    if (user.AspNetUserRoles.Count > 0)
                    {
                        var role = roles.FirstOrDefault(x => x.Id == user.AspNetUserRoles.FirstOrDefault()?.RoleId);
                        userRole.role = role?.Name;
                        userRole.lockoutEnabled = user.AspNetUserRoles.FirstOrDefault()?.LockoutEnabled;
                    }
                    usersWithRole.Add(userRole);
                };
                return usersWithRole;
            }
        }

        [ActionName("Item")]
        public AspNetUser GetItem(short id1, string id2)
        {
            return GetAll(id1).FirstOrDefault(u => u.Id == id2);
        }

        [ActionName("Item")]
        public void Delete(int id1, string id2)
        {
            using (var userRepository = new TableRepository<AspNetUser>())
            {
                // Remove all claims for the user for this domain
                var user = userRepository.Get(new[] { "Id" }, new[] { id2 }, new[] { "AspNetUserClaims", "AspNetUserRoles" });
                var toDeleteClaims = new List<AspNetUserClaim>();
                var toDeleteRoles = new List<AspNetUserRole>();
                foreach (var claim in user.AspNetUserClaims)
                {
                    if (claim.ClaimType == "Enterprise" && claim.ClaimValue == id1.ToString())
                    {
                        toDeleteClaims.Add(claim);
                    }
                    // Check if the claim is for this domain, claims have the format <domain>.<ClaimName>
                    var split = claim.ClaimType.Split(new char[] { '.' });
                    if (split.Length > 0 && split[0] == id1.ToString())
                    {
                        toDeleteClaims.Add(claim);
                    }
                }
                using (var claimRepository = new TableRepository<AspNetUserClaim>())
                {
                    foreach (var claim in user.AspNetUserClaims)
                    {
                        claimRepository.Delete(claim);
                    }
                }
                // Removes all Roles for the user for this domain
                foreach (var role in user.AspNetUserRoles)
                {
                    if (role.domain_id == id1)
                    {
                        toDeleteRoles.Add(role);
                    }
                }
                using (var roleRepository = new TableRepository<AspNetUserRole>())
                {
                    foreach (var role in toDeleteRoles)
                    {
                        roleRepository.Delete(role);
                    }
                }

                // Removes all views for the user for this domain
                using (var viewRepository = new TableRepository<User_gridView>())
                {
                    string[] AllFields = { "user_id", "domain_id" };
                    var views = viewRepository.GetAll(AllFields, new[] { id2, id1.ToString() }, null);

                    foreach (var view in views)
                    {
                        viewRepository.Delete(view);
                    }

                }

            }
            // Getting error if this was inside the previous using
            using (var userRepository = new TableRepository<AspNetUser>())
            {
                // If the user does not belong to any domain, then we delete the user
                var user = userRepository.Get(new[] { "Id" }, new[] { id2 }, new[] { "AspNetUserClaims", "AspNetUserRoles" });
                if (user.AspNetUserClaims.Count == 0 && user.AspNetUserRoles.Count == 0)
                {
                    userRepository.Delete(user);
                }
            }
        }

        [ActionName("AddUser")]
        public AspNetUser AddUser([FromBody]AspNetUser item, short id1)
        {
            using (var repository = new ProceduresRepository())
            {
                foreach (var user in item.project_user)
                {
                    user.user_pid = item.Id;
                    user.project_domain_id = id1;
                }
                if (repository.UpdateUserProject(item, id1))
                {
                    using (var userRepository = new TableRepository<AspNetUser>())
                    {
                        if (userRepository.Update(item))
                        {
                            return item;
                        }
                    }
                }
                //return new AspNetUser();
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        [ActionName("Item")]
        public AspNetUser Put([FromBody]AspNetUser item, short id1, string id2)
        {
            using (var repository = new ProceduresRepository())
            {
                item.Id = id2;
                foreach (var user in item.project_user)
                {
                    user.user_pid = item.Id;
                    user.project_domain_id = id1;
                }

                if (repository.UpdateUserProject(item, id1))
                {
                    using (var userRepository = new TableRepository<AspNetUser>())
                    {
                        var current = userRepository.GetByKey("id", item.Id);
                        // Update only the fields we have in the form and the domain_id
                        current.Comment = item.Comment;
                        current.first_name = item.first_name;
                        current.last_name = item.last_name;
                        current.Email = item.Email;
                        current.UserName = item.UserName;
                        current.CreationDate = item.CreationDate;
                        current.domain_id = id1; // The user's domain_id must be the same of the projects that he has access
                        if (userRepository.Update(current))
                        {
                            var role = item.AspNetUserRoles?.First();
                            if (role != null)
                            {
                                using (var userRoleRepository = new AspNetUserRoleRepository())
                                {
                                    userRoleRepository.SetRole(role);
                                }
                            }
                            return item;
                        }
                    }
                }

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        [ActionName("LockUnlock")]
        public HttpResponseMessage PutLockUnlock(short id1, string id2, bool id3)
        {
            try
            {
                using (var repository = new TableRepository<AspNetUserRole>())
                {
                    var user = repository.Get(new[] { "domain_id", "UserId" }, new object[] { id1, id2 }, null);
                    user.LockoutEnabled = id3;
                    repository.Update(user);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (System.Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [ActionName("InviteUser")]
        public async Task<IHttpActionResult> InviteUser(int id1, InviteUserViewModel model)
        {
            if (!HasDomainAccess(id1))
                return BadRequest();
            model.enterprise.domain_id = model.enterprise.domain_id <=0 ? (short)id1 : model.enterprise.domain_id;
            var user = await UserManager.FindByNameAsync(model.user.Email);
            bool isNewUser = user == null;
            if (isNewUser)
            {
                var result = UserManager.Create(new ApplicationUser { Email = model.user.Email, UserName = model.user.Email }, "T3mP0R4R1!");
                if (!result.Succeeded)
                {
                    return InternalServerError();
                }
                user = await UserManager.FindByNameAsync(model.user.Email);
            }
            var roleRepository = new TableRepository<AspNetRole>();
            var role = roleRepository.Get(new[] { "Id" }, new[] { model.user.AspNetUserRoles.FirstOrDefault()?.RoleId }, null);
            var userRepository = new TableRepository<AspNetUser>();
            var newUser = userRepository.Get(new[] { "Id" }, new[] { user.Id }, null);
            var userRole = new AspNetUserRole { UserId = user.Id, RoleId = role.Id, domain_id = model.enterprise.domain_id };
            newUser.CreationDate = DateTime.Now;
            newUser.first_name = model.user.first_name;
            newUser.last_name = model.user.last_name;
            newUser.Comment = model.user.Comment;
            newUser.project_user = model.user.project_user;
            newUser.AspNetUserRoles.Clear();
            newUser.AspNetUserRoles.Add(userRole);
            newUser.domain_id = model.enterprise.domain_id;
            this.Put(newUser, model.enterprise.domain_id, newUser.Id);

            if (isNewUser)
            {
                await UserManager.SendEmailAsync(user.Id, "AudaxWare xPlanner - New invite",
                    Helper.GetInvitedEmail(model.enterprise.name, true, HelperAPI.GetResetPasswordURL(user, UserManager)));
            }
            else
            {
                await UserManager.SendEmailAsync(user.Id, "AudaxWare xPlanner - New invite",
                    Helper.GetInvitedEmail(model.enterprise.name, false, $"{Helper.GetRootURL()}login/"));
            }
            return Ok();
        }

    }
}
