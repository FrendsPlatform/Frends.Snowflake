using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using dotenv.net;
using Frends.Snowflake.BatchOperation.Definitions;
using NUnit.Framework;
using Snowflake.Data.Client;

namespace Frends.Snowflake.BatchOperation.Tests;

public abstract class TestBase
{
    protected TestBase()
    {
        DotEnv.Load();
        ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        PrivateKeyFilePath = InitPrivateKeyFile();
    }

    private string ConnectionString { get; }

    private string PrivateKeyFilePath { get; }

    protected static Options DefaultOptions() => new()
    {
        ThrowErrorOnFailure = false,
        ErrorMessageOnFailure = string.Empty,
    };

    protected static Input DefaultInput() => new();

    protected Connection DefaultConnection() => new()
    {
        ConnectionString = ConnectionString,
        PrivateKeyFilePath = PrivateKeyFilePath,
    };

    [OneTimeSetUp]
    protected async Task Setup()
    {
        var connStringBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = ConnectionString,
        };
        connStringBuilder.Add("private_key_file", PrivateKeyFilePath);
        await using var conn = new SnowflakeDbConnection();
        conn.ConnectionString = connStringBuilder.ConnectionString;
        await conn.OpenAsync().ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS TaskTestTable (name VARCHAR,age NUMBER);";
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    [OneTimeTearDown]
    protected async Task Teardown()
    {
        var connStringBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = ConnectionString,
        };
        connStringBuilder.Add("private_key_file", PrivateKeyFilePath);
        await using var conn = new SnowflakeDbConnection();
        conn.ConnectionString = connStringBuilder.ConnectionString;
        await conn.OpenAsync().ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DROP TABLE IF EXISTS TaskTestTable";
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        File.Delete(PrivateKeyFilePath);
    }

    private static string InitPrivateKeyFile()
    {
        var privateKeyB64 = Environment.GetEnvironmentVariable("PRIVATE_KEY_BASE64");

        if (string.IsNullOrWhiteSpace(privateKeyB64))
            throw new Exception("Private key not found in environment variables.");
        byte[] privateKeyBytes = Convert.FromBase64String(privateKeyB64);
        string tempFile = Path.GetTempFileName();
        string finalFile = Path.ChangeExtension(tempFile, ".p8");
        File.Move(tempFile, finalFile);
        File.WriteAllBytes(finalFile, privateKeyBytes);

        return finalFile;
    }
}
