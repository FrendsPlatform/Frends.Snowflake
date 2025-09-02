using Frends.Snowflake.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Snowflake.Data.Client;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
    public static Result ExecuteQuery([PropertyTab] Input input, [PropertyTab] Options options)
    {
        if (string.IsNullOrWhiteSpace(input.ConnectionString))
            throw new Exception(@$"Invalid connection string.");
        IDbConnection conn = new SnowflakeDbConnection();
        try
        {
            string finalConnectionString = input.ConnectionString;
            if (!string.IsNullOrWhiteSpace(input.PrivateKeyFilePath))
            {
                if (finalConnectionString.Contains("private_key=", StringComparison.OrdinalIgnoreCase) ||
                    finalConnectionString.Contains("private_key_file=", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("ConnectionString already contains a private_key. Use either ConnectionString OR PrivateKeyFilePath, not both.");
                if (!File.Exists(input.PrivateKeyFilePath))
                    throw new FileNotFoundException($"Private key file not found: {input.PrivateKeyFilePath}");

                string pem = File.ReadAllText(input.PrivateKeyFilePath);

                string formattedPrivateKey;
                if (pem.Contains("ENCRYPTED PRIVATE KEY"))
                {
                    formattedPrivateKey = ConvertEncryptedPrivateKey(pem, input.PrivateKeyPassphrase);
                }
                else
                {
                    formattedPrivateKey = FormatPrivateKeyForSnowflake(pem);
                }
                finalConnectionString += $";private_key={formattedPrivateKey}";

            }
            conn.ConnectionString = finalConnectionString;
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
                    var table = new DataTable();
                    var reader = cmd.ExecuteReader();
                    table.Load(reader);
                    var result = new Result(true, reader.RecordsAffected, null, table);
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
        finally
        {
            if (conn.State != ConnectionState.Closed)
                conn.Close();
            conn.Dispose();
        }
    }

    private static string FormatPrivateKeyForSnowflake(string pemContent)
    {
        string normalized = pemContent
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        return DoubleTrailingEquals(normalized);
    }

    private static string ConvertEncryptedPrivateKey(string encryptedPem, string passphrase)
    {
        if (string.IsNullOrEmpty(passphrase))
            throw new ArgumentException("Passphrase is required for encrypted private keys");

        RSA rsa = RSA.Create();
        rsa.ImportFromEncryptedPem(encryptedPem, passphrase);

        byte[] privateKeyBytes = rsa.ExportPkcs8PrivateKey();
        string privateKeyBase64 = Convert.ToBase64String(privateKeyBytes);

        StringBuilder pemBuilder = new StringBuilder();
        pemBuilder.Append("-----BEGIN PRIVATE KEY-----\n");

        for (int i = 0; i < privateKeyBase64.Length; i += 64)
        {
            int length = Math.Min(64, privateKeyBase64.Length - i);
            pemBuilder.Append(privateKeyBase64.Substring(i, length));
            pemBuilder.Append("\n");
        }

        pemBuilder.Append("-----END PRIVATE KEY-----");

        string result = DoubleTrailingEquals(pemBuilder.ToString());

        return result;
    }

    private static string DoubleTrailingEquals(string pemContent)
    {
        if (pemContent.EndsWith("==="))
        {
            return pemContent.Substring(0, pemContent.Length - 3) + "======";
        }
        else if (pemContent.EndsWith("=="))
        {
            return pemContent.Substring(0, pemContent.Length - 2) + "====";
        }
        else if (pemContent.EndsWith("="))
        {
            return pemContent.Substring(0, pemContent.Length - 1) + "==";
        }

        return pemContent;
    }
}