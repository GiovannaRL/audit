using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace xPlannerAPI.Security.Services
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var mail = new MailMessage();
            var SmtpServer = new SmtpClient();
            
            mail.To.Add(message.Destination);
            mail.Subject = message.Subject;
            mail.IsBodyHtml = true;
            mail.Body = message.Body;

            SmtpServer.Send(mail);
            return Task.FromResult(HttpStatusCode.OK);
            //return Request.CreateResponse(HttpStatusCode.OK);

            /*const string apiKey = "key-ef7a2525b9a4141408b40cd4d4e438e0";
            const string sandBox = "sandbox5c2ed57ac7b94f0ea5d372f3194b026c.mailgun.org";
            byte[] apiKeyAuth = Encoding.ASCII.GetBytes($"api:{apiKey}");
            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.mailgun.net/v3/") };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(apiKeyAuth));

            var form = new Dictionary<string, string>
            {
                ["from"] = "customerservice@audaxware.com",
                ["to"] = message.Destination,
                ["subject"] = message.Subject,
                ["text"] = message.Body
            };

            HttpResponseMessage response =
                httpClient.PostAsync(sandBox + "/messages", new FormUrlEncodedContent(form)).Result;
            return Task.FromResult((int)response.StatusCode);*/
        }
    }
}