using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Models;
using System.Net;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Security.Services;
using xPlannerCommon.Enumerators;

/* It sends email to audaxware when a user requires to join the site */
namespace xPlannerAPI.Controllers
{
    [AllowAnonymous]
    [AudaxAuthorize(AreaModule = AreaModule.NotLoggedArea)]
    public class SubscriptionController : AudaxWareController
    {
        [ActionName("Item")]
        public HttpResponseMessage Post([FromBody] SubscriptionInfo info)
        {
            var sendEmail = new EmailService();
            var message = new Microsoft.AspNet.Identity.IdentityMessage
            {
                Destination = System.Web.Configuration.WebConfigurationManager.AppSettings["support_email"],
                Subject = "[Audaxware] Request Subscription",
                Body = "<p><strong>Name:</strong> " + info.name + "</p>"
            };
            message.Body += "<p><strong>Email from:</strong> " + info.email + "</p>";
            message.Body += "<p><strong>Phone Number:</strong> " + info.phone_number + "</p>";
            message.Body += "<p><strong>Company Name:</strong> " + info.company + "</p>";
            message.Body += "<p><strong>Message:</strong> " + info.message + "</p>";

            sendEmail.SendAsync(message);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
