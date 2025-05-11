using System;
using System.IO;
using System.Text;
using System.Web.Configuration;

public class Helper
{
    public static void RecordLog(string repositoryName, string methodName, Object ex)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(DateTime.Now + " *** " + repositoryName + " - " + methodName + ": ");
        sb.AppendLine("" + ex);
        sb.AppendLine("****************************************************************************");
        sb.AppendLine("");
        string filePath = WebConfigurationManager.AppSettings["error_log_path"];
        if (filePath == null || filePath.Length == 0 || !Directory.Exists(filePath))
        {
            filePath = Path.Combine(Path.GetTempPath(), "AudaxWare");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
        }
        File.AppendAllText(Path.Combine(filePath, "error_log_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + ".txt"), sb.ToString());
    }
}