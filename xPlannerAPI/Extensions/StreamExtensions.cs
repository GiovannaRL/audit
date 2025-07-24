using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using xPlannerCommon.Models;

namespace xPlannerAPI.Extensions
{
    public static class StreamExtensions
    {
        public static bool IsImageValid(this Stream requestStream) 
        {
            try
            {
                using (var image = System.Drawing.Image.FromStream(requestStream, false, true))
                {
                    requestStream.Position = 0;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}