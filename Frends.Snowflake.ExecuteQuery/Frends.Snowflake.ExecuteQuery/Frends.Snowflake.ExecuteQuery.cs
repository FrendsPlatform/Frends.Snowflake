using Frends.Snowflake.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Snowflake.Data.Client;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Snowflake.ExecuteQuery;

/// <summary>
/// Snowflake Task.
/// </summary>
public class Snowflake
{
    /// <summary>
    /// Execute Snowflake query.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Snowflake.ExecuteQuery)
    /// </summary>
    /// <param name="input">Connection and command parameters.</param>
    /// <param name="options">Options for controlling the behavior of a Task.</param>
    /// <returns>Object { bool Success, int RecordsAffected, dynamic ErrorMessage, dynamic Data }</returns>
    public static Result ExecuteQuery([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.ConnectionString))
            throw new Exception("Invalid connection string.");

        try
        {
            using IDbConnection conn = new SnowflakeDbConnection();
            var csb = new DbConnectionStringBuilder { ConnectionString = input.ConnectionString };

            if (!string.IsNullOrWhiteSpace(input.PrivateKeyFilePath))
            {
                var csLower = csb.ConnectionString.ToLowerInvariant();
                if (csb.ContainsKey("private_key") || csb.ContainsKey("private_key_file"))
                    throw new Exception("ConnectionString already contains a private key. Use either ConnectionString OR PrivateKeyFilePath, not both.");
                if (!File.Exists(input.PrivateKeyFilePath))
                    throw new FileNotFoundException($"Private key file not found: {input.PrivateKeyFilePath}");

                csb["private_key_file"] = input.PrivateKeyFilePath.Replace('\\', '/');
                if (!string.IsNullOrEmpty(input.PrivateKeyPassphrase))
                    csb["private_key_pwd"] = input.PrivateKeyPassphrase;
            }

            conn.ConnectionString = csb.ConnectionString;
            conn.Open();
            using IDbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = options.TimeOut;
            cmd.CommandText = input.CommandText;
            switch (input.CommandType)
            {
                case CommandTypes.ExecuteNonQuery:
                    var executeNQ = cmd.ExecuteNonQuery();
                    return new Result(true, executeNQ, null, null);
                case CommandTypes.ExecuteReader:
                    var reader = cmd.ExecuteReader();
                    var jToken = LoadData(reader, cancellationToken);
                    var result = new Result(true, reader.RecordsAffected, null, jToken);
                    if (!reader.IsClosed)
                        reader.Close();
                    reader.Dispose();
                    return result;
                case CommandTypes.ExecuteScalar:
                    var executeS = cmd.ExecuteScalar();
                    return new Result(true, 1, null, JToken.FromObject(new { Value = executeS }));
            }
            if (options.ThrowExceptionOnError)
                throw new Exception("Invalid Command type.");
            return new Result(false, 0, "Invalid Command type", null);
        }
        catch (Exception ex)
        {
            if (options.ThrowExceptionOnError)
                throw;
            return new Result(false, 0, ex, null);
        }
    }

    private static JToken LoadData(IDataReader reader, CancellationToken cancellationToken)
    {
        var table = new JArray();

        // Cache metadata up-front
        var fieldCount = reader.FieldCount;
        var names = new string[fieldCount];

        for (var i = 0; i < fieldCount; i++)
        {
            names[i] = reader.GetName(i);
        }

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = new JObject();

            for (var i = 0; i < fieldCount; i++)
            {
                object o = reader.GetValue(i);

                JToken token;
                if (o == DBNull.Value)
                {
                    token = JValue.CreateNull();
                }
                else if (o is byte[] bytes)
                {
                    // Snowflake BINARY -> base64 string is the most interoperable representation
                    token = new JValue(Convert.ToBase64String(bytes));
                }
                else if (o is float f)
                {
                    token = FloatToJsonToken(f);
                }
                else if (o is double d)
                {
                    token = FloatToJsonToken(d);
                }
                else if (o is decimal m)
                {
                    // decimals are safe
                    token = new JValue(m);
                }
                else if (o is DateTimeOffset dto)
                {
                    // Keep offset — JSON.NET will emit ISO 8601 with offset
                    token = new JValue(dto);
                }
                else if (o is DateTime dt)
                {
                    token = new JValue(dt);
                }
                else if (o is string s)
                {
                    token = new JValue(s);
                }
                else
                {
                    token = JToken.FromObject(o);
                }

                row.Add(new JProperty(names[i], token));
            }

            table.Add(row);
        }

        return table;
    }

    private static bool LooksLikeJson(string s)
    {
        // Cheap pre-check to avoid exceptions for normal strings
        s = s.Trim();
        return (s.Length > 1 && (s[0] == '{' && s[^1] == '}')) ||
               (s.Length > 1 && (s[0] == '[' && s[^1] == ']'));
    }

    private static bool TryParseJson(string s, out JToken? token)
    {
        try { token = JToken.Parse(s); return true; }
        catch { token = null; return false; }
    }

    private static JToken FloatToJsonToken(double d)
    {
        if (double.IsNaN(d) || double.IsInfinity(d))
            return JValue.CreateNull(); // or new JValue(d.ToString()) if you prefer strings
        return new JValue(d);
    }
}