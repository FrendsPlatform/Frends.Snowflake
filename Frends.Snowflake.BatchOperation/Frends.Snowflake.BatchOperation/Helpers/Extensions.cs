using System;
using System.Data;
using Frends.Snowflake.BatchOperation.Definitions;

namespace Frends.Snowflake.BatchOperation.Helpers;

internal static class Extensions
{
    internal static IsolationLevel ToIsolationLevel(
        this TransactionIsolationLevel transactionIsolationLevel)
    {
        return GetEnum<IsolationLevel>(transactionIsolationLevel);
    }

    private static T GetEnum<T>(Enum enumValue)
    {
        return (T)Enum.Parse(typeof(T), enumValue.ToString());
    }
}
