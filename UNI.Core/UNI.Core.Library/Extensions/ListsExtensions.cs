using System.Collections;

namespace UNI.Core.Library.Extensions
{
    public static class ListsExtensions
    {
        public static bool IsEmpty(this IList list)
        {
            if (list == null)
                return true;

            if (list.Count <= 0)
                return true;

            return false;
        }

        public static bool IsNotEmpty(this IList list) => !IsEmpty(list);
    }
}
