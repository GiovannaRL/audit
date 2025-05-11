using System;

namespace xPlannerCommon.Services
{
    static public class Domain
    {
        public static string GetRoot()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string rootLower = root.ToLower();

            if (rootLower.Contains("bin"))
            {
                root = root.Substring(0, rootLower.IndexOf("bin"));
            }

            return root;
        }
    }
}
