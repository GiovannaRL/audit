using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using xPlannerAPI.Controllers;
using xPlannerAPI.Security.Models;
using xPlannerAPI.Services;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;
using HelperAPI = xPlannerAPI.App_Data.Helper;

namespace xPlannerAPI.Security.Controller
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : AudaxWareController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

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

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            var token = HttpContext.Current.Request.Headers["Authorization"];
            if (token != null)
            {
                Helper.TokenData.Remove(token.Replace("Bearer", "").Trim());
            }
            return Ok();
        }

        private domain GetFullDomain(short domain_id) {
            using (var repository = new TableRepository<domain>())
            {
                return repository.Get(new[] { "domain_id" }, new[] { domain_id }, null);
            }
        }

        // POST api/Account/AddLoggedDomain
        [Route("AddLoggedDomain")]
        public IHttpActionResult AddLoggedDomain([FromBody]domain domain)
        {
            var token = HttpContext.Current.Request.Headers["Authorization"];

            if (token == null || domain == null || !HasDomainAccess(domain.domain_id))
                return Unauthorized();

            var key = token.Replace("Bearer", "").Trim();
            if (Helper.TokenData.ContainsKey(key))
            {
                Helper.TokenData.Remove(key);
            }

            var userData = new UserData { loggedDomain = GetFullDomain(domain.domain_id) };
            Helper.TokenData.Add(token.Replace("Bearer", "").Trim(), userData);

            return Ok();
        }

        // PUT api/Account/ChangeLoggedDomain
        [Route("ChangeLoggedDomain")]
        public IHttpActionResult PutLoggedDomain([FromBody] domain domain)
        {
            var token = HttpContext.Current.Request.Headers["Authorization"];
            if (token == null || domain == null || !HasDomainAccess(domain.domain_id))
                return Unauthorized();

            lock (Helper.TokenData)
            {
                token = token.Replace("Bearer", "").Trim();
                Helper.TokenData.Remove(token);

                var userData = new UserData { loggedDomain = GetFullDomain(domain.domain_id) };
                Helper.TokenData.Add(token, userData);
            }

            return Ok();

        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            return !result.Succeeded ? GetErrorResult(result) : Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        private static string GetResetEmail(string resetLink)
        {
            return string.Format("<!DOCTYPE html> <html> <head> <title></title> <meta charset=\"utf - 8\" /> </head> <body> <p>You are receiving this e-mail because you have either signed up for AudaxWare xPlanner or you have requested to reset your password.</p> <p>Please use this  <a href=\"{0}\">link</a> to reset your password.</p> <p> Thank you for using xPlanner. </p> </body> </html>",
                resetLink);
        }

        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                return NotFound();
            }

            await UserManager.SendEmailAsync(user.Id, "AudaxWare xPlanner - New Account / Password Reset",
                GetResetEmail(HelperAPI.GetResetPasswordURL(user, UserManager)));
            return Ok();

            // If we got this far, something failed, redisplay form
        }

        [AllowAnonymous]
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByNameAsync(HttpContext.Current.Server.UrlDecode(model.Email));

            if (user == null)
            {
                return NotFound();
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, HttpContext.Current.Server.UrlDecode(model.Token), model.Password);
            if (result.Succeeded)
                return Ok();
            else
                return BadRequest(result.Errors.FirstOrDefault());

            // If we got this far, something failed, redisplay form
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (result.Succeeded)
                return null;

            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            if (ModelState.IsValid)
            {
                // No ModelState errors are available to send, so just return an empty BadRequest.
                return BadRequest();
            }

            return BadRequest(ModelState);

        }

        private static class RandomOAuthStateGenerator
        {
            private static readonly RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", nameof(strengthInBits));
                }

                var strengthInBytes = strengthInBits / bitsPerByte;

                var data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }
        #endregion
    }
}
