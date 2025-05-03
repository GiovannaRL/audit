using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace xPlannerAPI.Tests
{
    class EmbeddedResourceHelper
    {
        static Assembly _assembly;
        static EmbeddedResourceHelper()
        {
            _assembly = Assembly.GetExecutingAssembly();
        }
        static public string GetTempFilePathFromResource(string embeddedName)
        {
            var fileStream = _assembly.GetManifestResourceStream(embeddedName);
            string destPath = Path.GetTempFileName();
            using (var tempFileStream = new FileStream(destPath, FileMode.OpenOrCreate | FileMode.Truncate, FileAccess.Write))
            {
                fileStream.CopyTo(tempFileStream);
            }
            return destPath;
        }
    }
}
