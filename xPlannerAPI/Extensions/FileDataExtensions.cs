using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using xPlannerCommon.Models;

namespace xPlannerAPI.Extensions
{
    public static class FileDataExtensions
    {
        public static bool IsImageValid(this FileData picture)
        {
            byte[] imageBytes = Convert.FromBase64String(picture.base64File);
            using (var stream = new MemoryStream(imageBytes))
            {
                return stream.IsImageValid();
            }
        }
    }
}