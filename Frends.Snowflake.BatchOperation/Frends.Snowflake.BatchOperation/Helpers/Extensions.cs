using System.Collections.Generic;

namespace Frends.Snowflake.BatchOperation.Helpers;

internal static class Extensions
{
    internal static void SafeInsert<T>(this List<T> list, int index, T value)
    {
        while (list.Count < index)
        {
            list.Add(default);
        }

        if (index == list.Count)
        {
            list.Add(value);
        }
        else
        {
            list.Insert(index, value);
        }
    }
}
