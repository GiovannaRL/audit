using System;
using System.Text;
using System.Web;
using Microsoft.AspNet.Identity;
using xPlannerAPI.Security;
using xPlannerAPI.Security.Models;
using xPlannerCommon.Models;
using System.Configuration;
using System.Web.Configuration;

namespace xPlannerAPI.App_Data
{
    public class Helper
    {
        public static void RecordLog(string repositoryName, string methodName, object ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(DateTime.Now + " *** " + repositoryName + " - " + methodName + ": ");
            sb.AppendLine("" + ex);
            sb.AppendLine("****************************************************************************");
            sb.AppendLine("");
            System.Diagnostics.Trace.TraceError(sb.ToString());
        }

        public static string GetResetPasswordURL(ApplicationUser user, ApplicationUserManager userManager)
        {
            string resetUrl;

            var code = userManager.GeneratePasswordResetToken(user.Id);
            // When debugging locally we use a custom port with the xPlannerUI
            if (HttpContext.Current.Request.Url.Port != 443 && HttpContext.Current.Request.Url.Port != 80)
            {
                resetUrl = $"{xPlannerCommon.App_Data.Helper.GetRootURL()}resetPassword/";
            }
            else
            {
                resetUrl = $"{xPlannerCommon.App_Data.Helper.GetRootURL()}resetPassword/";
            }
            resetUrl += $"{HttpContext.Current.Server.UrlEncode(HttpContext.Current.Server.UrlEncode(user.Email))}/{HttpContext.Current.Server.UrlEncode(HttpContext.Current.Server.UrlEncode(code))}";

            return resetUrl;
        }

        public static string GetAssetCatalogURL(asset item)
        {
            return $"{xPlannerCommon.App_Data.Helper.GetRootURL()}workspace/assets/{item.domain_id}/{item.asset_id}";
        }

        public static string GetMaxRequestLengthMessage()
        {
            return "The file size limit is " + GetMaxFileSize() + "MB";
        }

        public static string GetMaxRequestLength()
        {
            return GetMaxFileSize();

        }

        private static string GetMaxFileSize()
        {
            int maxRequestLength = 4096; //default value
            HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            if (section != null)
                maxRequestLength = section.MaxRequestLength;
            return (maxRequestLength / 1024).ToString();
        }

    }
}
