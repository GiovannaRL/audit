using System.Globalization;

namespace OfflineXPlanner.Utils
{
    public static class ConvertUtil
    {
        public static string ToUSFormat(decimal? value)
        {
            CultureInfo us = new CultureInfo("en-US");
            string x = value.GetValueOrDefault().ToString(us);
            return x;
        }
    }
}
