using System.Collections.Generic;
using System.Linq;

namespace OfflineXPlanner.Utils
{
    public static class ListUtil
    {
        public static bool isEmptyOrNull<T>(IEnumerable<T> items)
        {
            return isNull(items) || !items.Any();
        }

        public static bool isNull<T>(IEnumerable<T> items)
        {
            return items == null;
        }
    }
}
