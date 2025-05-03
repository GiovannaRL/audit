using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace xPlannerCommon.Models
{
    public class BlobDownload
    {
        public MemoryStream BlobStream { get; set; }
        public string BlobFileName { get; set; }
        public long BlobLength { get; set; }
        public string BlobContentType { get; set; }
    }
}