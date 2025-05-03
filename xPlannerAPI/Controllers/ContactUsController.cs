using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Security.Services;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AllowAnonymous]
    [AudaxAuthorize(AreaModule = AreaModule.NotLoggedArea)]
    public class ContactUsController : AudaxWareController
    {
        [ActionName("Item")]
        public HttpResponseMessage Post([FromBody] SubscriptionInfo info)
        {
            var sendEmail = new EmailService();
            var message = new Microsoft.AspNet.Identity.IdentityMessage
            {
                Destination = System.Web.Configuration.WebConfigurationManager.AppSettings["support_email"],
                Subject = "[AudaxWare - Contact Us] " + info.subject,
                Body = "<p><strong>Name:</strong> " + info.name + "</p>"
            };
            message.Body += "<p><strong>Email from:</strong> " + info.email + "</p>";
            message.Body += "<p><strong>Phone Number:</strong> " + info.phone_number + "</p>";
            message.Body += "<p><strong>Message:</strong> " + info.message + "</p>";

            sendEmail.SendAsync(message);
            
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
