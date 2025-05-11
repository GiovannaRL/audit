using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerCommon.Services
{
    public class WkhtmltopdfRepository
    {
        public static void ConvertToPDF(string[] htmlPages, string saveIntoPath, string coverPage = null, string tocPage = null)
        {

            StringBuilder command = new StringBuilder("--page-width 215.9 --page-height 279.4 --footer-center \"Copyright® " + DateTime.Now.Year + " AudaxWare LLC\" --footer-right \"Page [page] of [toPage]\" --footer-font-size 6 ");

            foreach (string page in htmlPages)
            {
                command.Append(page);
                command.Append(" ");
            }

            using (var p = new System.Diagnostics.Process())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "wkhtmltopdf.exe",
                    Arguments = command + " " + saveIntoPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };
                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit();
                var exitCode = p.ExitCode;
                p.Close();
            }


            foreach (string page in htmlPages)
            {
                File.Delete(page);
            }
        }

        public static void ConvertToPDF(string htmlPagePath, string saveIntoPath)
        {

            using (var p = new System.Diagnostics.Process())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    //FileName = "wkhtmltopdf.exe",
                    FileName = Path.Combine(Domain.GetRoot(), "wkhtmltopdf.exe"),
                    Arguments = "--page-width 215.9 --page-height 279.4 --footer-center \"Copyright® " + DateTime.Now.Year + " AudaxWare LLC\" --footer-right \"Page [page] of [toPage]\" --footer-font-size 6 " + htmlPagePath + " " + saveIntoPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };
                p.StartInfo = startInfo;
                p.Start();
                p.WaitForExit();
                var exitCode = p.ExitCode;
                p.Close();

                File.Delete(htmlPagePath);
            }
        }
    }
}