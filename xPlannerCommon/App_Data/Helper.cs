using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerCommon.App_Data
{
    public static class Helper
    {
        public static readonly Dictionary<string, UserData> TokenData = new Dictionary<string, UserData>();

        public static readonly short AudaxwareDomainId = 1;

        public static void RecordLog(string repositoryName, string methodName, object ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(DateTime.Now + " *** " + repositoryName + " - " + methodName + ": ");
            sb.AppendLine("" + ex);
            sb.AppendLine("****************************************************************************");
            sb.AppendLine("");
            System.Diagnostics.Trace.TraceError(sb.ToString());
        }

        public static bool ShowAudaxWareInfo(int domainId)
        {
            if (domainId == 1)
                return true;

            using (var db = new audaxwareEntities())
            {
                var data = db.domains.Find(domainId);

                return data != null && data.show_audax_info;
            }
        }

        public static string GetEnterpriseName(string enterpriseDomainName)
        {
            var index = enterpriseDomainName?.IndexOf(".") ?? -1;
            return index > -1 
                ? enterpriseDomainName?.Substring(0, index) 
                : enterpriseDomainName;
        }

        public static string GetEnterpriseName(int domainId)
        {
            using (var db = new audaxwareEntities())
            {
                var d = db.domains.Find(domainId);
                if (d?.name == null)
                    return null;

                var index = d.name.IndexOf(".");
                return index > -1
                    ? d.name.Substring(0, index)
                    : d.name;
            }
        }

        public static string GetInvitedEmail(string enterprise, bool newUser, string link)
        {
            return newUser ?
                string.Format("<!DOCTYPE html> <html> <head> <title></title> <meta charset=\"utf - 8\" /> </head> <body> <p><b>Welcome to XPlanner!</b></p> </br> <p>You have been invited to Enterprise {0} on Audaxware. Click here <a href=\"{1}\">link</a> to set up your account.</p> <p>This link will expire in 24 hours.</p> </br> </br> <p> Thank you for using xPlanner. </p> </body> </html>",
                    enterprise, link)
                : string.Format("<!DOCTYPE html> <html> <head> <title></title> <meta charset=\"utf - 8\" /> </head> <body> <p><b>Welcome to XPlanner!</b></p> </br> <p>You have been invited to Enterprise {0} on Audaxware. Click here <a href=\"{1}\">link</a> to login.</p> </br> </br> <p> Thank you for using xPlanner. </p> </body> </html>",
                    enterprise, link);
        }

        public static string GetRootURL()
        {
            string rootURL;
            // When debugging locally we use a custom port with the xPlannerUI
            if (HttpContext.Current.Request.Url.Port != 443 && HttpContext.Current.Request.Url.Port != 80)
            {
                rootURL = $"{HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)}/xPlannerUI/#!/";
            }
            else
            {
                rootURL = $"{HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)}/#!/";
            }

            return rootURL;
        }

        public static UserData GetDecryptedToken() {

            var token = HttpContext.Current.Request.Headers["Authorization"];

            if (token != null)
            {
                UserData userData;
                TokenData.TryGetValue(token.Replace("Bearer", "").Trim(), out userData);

                return userData;
            }

            return null;
        }


        public static T Clone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
        }
    }
}
