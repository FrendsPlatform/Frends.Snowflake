using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.Json;
using Frends.Snowflake.BatchOperation.Definitions;

namespace Frends.Snowflake.BatchOperation.Helpers;

internal static class SnowflakeHandler
{
    internal static DbType GetDbType(object value)
    {
        return value switch
        {
            int or long => DbType.Int64,
            double or float or decimal => DbType.Double,
            bool => DbType.Boolean,
            DateTime => DbType.DateTime,
            _ => DbType.String,
        };
    }

    internal static string BuildConnectionString(Connection connection)
    {
        try
        {
            var connStringBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connection.ConnectionString,
            };

            if (!string.IsNullOrWhiteSpace(connection.PrivateKeyFilePath))
            {
                if (connStringBuilder.ContainsKey("private_key_file"))
                {
                    throw new Exception(
                        "ConnectionString already contains a private key. Use either ConnectionString parameter OR PrivateKeyFilePath, not both.");
                }

                if (!File.Exists(connection.PrivateKeyFilePath))
                    throw new FileNotFoundException($"Private key file not found: {connection.PrivateKeyFilePath}");

                connStringBuilder.Add("private_key_file", connection.PrivateKeyFilePath.Replace('\\', '/'));
            }

            if (!string.IsNullOrWhiteSpace(connection.PrivateKeyPassphrase))
            {
                if (connStringBuilder.ContainsKey("private_key_pwd"))
                {
                    throw new Exception(
                        "ConnectionString already contains a private key password. Use either ConnectionString parameter OR PrivateKeyPassphrase, not both.");
                }

                connStringBuilder.Add("private_key_pwd", connection.PrivateKeyPassphrase);
            }

            return connStringBuilder.ConnectionString;
        }
        catch (Exception e)
        {
            throw new Exception("Error building connection string.", e);
        }
    }

    internal static Dictionary<string, List<object>> GetData(string jsonData)
    {
        using JsonDocument doc = JsonDocument.Parse(jsonData);
        JsonElement root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Input JSON must be an array of arrays.");

        int rowCount = root.GetArrayLength();

        if (rowCount == 0) return new Dictionary<string, List<object>>();

        var result = new Dictionary<string, List<object>>();

        var objIndex = 0;

        foreach (var obj in root.EnumerateArray())
        {
            foreach (var prop in obj.EnumerateObject())
            {
                var value = GetValue(prop.Value);
                result.TryAdd(prop.Name, []);
                result[prop.Name].SafeInsert(objIndex, value);
            }

            objIndex++;
        }

        var maxListSize = result.Values.Max(list => list.Count);

        foreach (var col in result.Where(col => col.Value.Count < maxListSize))
        {
            col.Value.SafeInsert(maxListSize - 1, null);
        }

        return result;
    }

    private static object GetValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => DBNull.Value,
            _ => element.ToString(),
        };
    }
}
