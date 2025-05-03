namespace OfflineXPlanner.Utils
{
    public static class ValidFormUtil
    {
        public static bool AllNotNull(params string[] fields)
        {
            if (fields != null || fields.Length > 0)
            {
                foreach (string field in fields)
                {
                    if (string.IsNullOrEmpty(field))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
