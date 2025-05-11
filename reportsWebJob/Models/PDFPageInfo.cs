using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class PDFPageInfo
    {
        public string path { get; set; }
        public string title { get; set; }

        public PDFPageInfo(string path, string title)
        {
            this.path = path;
            this.title = title;
        }
    }
}
