using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.Snowflake.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Snowflake.Data.Client;

namespace Frends.Snowflake.ExecuteQuery.Tests;

// Running tests locally:
//   1. Copy .env.example to .env in the test project and fill in the values
//      (loaded with dotenv.net; real environment variables are used when .env is absent).
//   2. Run tests as usual (`dotnet test`).
//
// Running tests in GitHub Actions:
//   - Tests will use the Snowflake_PrivateKeyB64 secret.
//   - The key is decoded in InitPrivateKeyFile() and written to a temp file.
[TestClass]
public class UnitTests
{
    private static readonly bool EnvLoaded = LoadEnv();
    private static readonly string? ConnectionString = GetEnv("Snowflake_ConnectionString");
    private static readonly string? PrivateKeyFilePath = InitPrivateKeyFile();
    private static readonly string? PrivateKeyPassphrase = GetEnv("Snowflake_PrivateKeyPassphrase");
    private static readonly List<string> Names = new() { "John", "Jane", "Tom", "Alice", "Bob", "Charlie", "David", "Eve", "Mallory", "Oscar" };
    private static readonly Random Random = new();
    private static readonly int Age = Random.Next(1, 101);
    private static string? tempPrivateKeyFile;
    private static Input input = new();
    private static Connection connection = new();
    private static Options options = new();

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        var connStringBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = ConnectionString,
        };
        connStringBuilder.Add("private_key_file", PrivateKeyFilePath!);
        connStringBuilder.Add("private_key_pwd", PrivateKeyPassphrase!);
        await using var conn = new SnowflakeDbConnection();
        conn.ConnectionString = connStringBuilder.ConnectionString;
        await conn.OpenAsync().ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS TaskTestTable (name VARCHAR,age NUMBER);";
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    [ClassCleanup]
    public static async Task GlobalTeardown()
    {
        var connStringBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = ConnectionString,
        };
        connStringBuilder.Add("private_key_file", PrivateKeyFilePath!);
        connStringBuilder.Add("private_key_pwd", PrivateKeyPassphrase!);
        await using var conn = new SnowflakeDbConnection();
        conn.ConnectionString = connStringBuilder.ConnectionString;
        await conn.OpenAsync().ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DROP TABLE IF EXISTS TaskTestTable";
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        File.Delete(PrivateKeyFilePath!);
    }

    [TestInitialize]
    public void Setup()
    {
        input = new Input()
        {
            CommandText = null,
            CommandType = CommandTypes.ExecuteNonQuery,
        };

        connection = new Connection()
        {
            PrivateKeyFilePath = PrivateKeyFilePath,
            PrivateKeyPassphrase = PrivateKeyPassphrase,
            ConnectionString = ConnectionString,
        };

        options = new Options()
        {
            ThrowErrorOnFailure = true,
            TimeOut = 30,
        };
    }

    [TestMethod]
    public void InvalidCommandType_Fails()
    {
        input.CommandText = @$"insert into TaskTestTable values ('{Names[Random.Next(Names.Count)]}', 10);";
        input.CommandType = (CommandTypes)999;
        var ex = Assert.Throws<Exception>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Invalid Command type"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_TableExists()
    {
        input.CommandText = @$"insert into TaskTestTable values ('{Names[Random.Next(Names.Count)]}', 10);";
        input.CommandType = CommandTypes.ExecuteReader;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.RecordsAffected);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_TableExists()
    {
        input.CommandText = @$"insert into TaskTestTable values ('{Names[Random.Next(Names.Count)]}', 20);";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_TableExists()
    {
        input.CommandText = @$"insert into TaskTestTable values ('{Names[Random.Next(Names.Count)]}', 30);";
        input.CommandType = CommandTypes.ExecuteScalar;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_TableDoesntExists_ThrowErrorOnFailure_True()
    {
        input.CommandText = "insert into NoTable values ('A', 1);";
        input.CommandType = CommandTypes.ExecuteReader;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_TableDoesntExists_ThrowErrorOnFailure_False()
    {
        input.CommandText = "insert into NoTable values ('A', 1);";
        input.CommandType = CommandTypes.ExecuteReader;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Table 'NOTABLE' does not exist or not authorized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_TableDoesntExists_ThrowErrorOnFailure_True()
    {
        input.CommandText = "insert into NoTable values ('A', 1);";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_TableDoesntExists_ThrowErrorOnFailure_False()
    {
        input.CommandText = "insert into NoTable values ('A', 1);";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Table 'NOTABLE' does not exist or not authorized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_TableDoesntExists_ThrowErrorOnFailure_True()
    {
        input.CommandText = "insert into NoTable values ('A', 1);";
        input.CommandType = CommandTypes.ExecuteScalar;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_TableDoesntExists_ThrowErrorOnFailure_False()
    {
        input.CommandText = "insert into NoTable values ('A', 1);";
        input.CommandType = CommandTypes.ExecuteScalar;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Table 'NOTABLE' does not exist or not authorized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_InvalidStatement_ThrowErrorOnFailure_True()
    {
        input.CommandText = "insert into TaskTestTable values (1, 'A');";
        input.CommandType = CommandTypes.ExecuteReader;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_InvalidStatement_ThrowErrorOnFailure_False()
    {
        input.CommandText = "insert into TaskTestTable values (1, 'A');";
        input.CommandType = CommandTypes.ExecuteReader;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Numeric value 'A' is not recognized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_InvalidStatement_ThrowErrorOnFailure_True()
    {
        input.CommandText = "insert into TaskTestTable values (1, 'A');";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_InvalidStatement_ThrowErrorOnFailure_False()
    {
        input.CommandText = "insert into TaskTestTable values (1, 'A');";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Numeric value 'A' is not recognized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_InvalidStatement_ThrowErrorOnFailure_True()
    {
        input.CommandText = "insert into TaskTestTable values (1, 'A');";
        input.CommandType = CommandTypes.ExecuteScalar;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_InvalidStatement_ThrowErrorOnFailure_False()
    {
        input.CommandText = "insert into TaskTestTable values (1, 'A');";
        input.CommandType = CommandTypes.ExecuteScalar;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Numeric value 'A' is not recognized"));
    }

    [TestMethod]
    public void ExecuteTest_Select_ExecuteReader_TableExists()
    {
        input.CommandText = "Select * from TaskTestTable;";
        input.CommandType = CommandTypes.ExecuteReader;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Data.Count > 0);
        Assert.IsNull(result.Error);
        Assert.AreEqual(-1, result.RecordsAffected);

        // Check props
        var jObject = (JObject)result.Data[0];
        Assert.IsTrue(jObject.ContainsKey("NAME"));
        Assert.IsTrue(jObject.ContainsKey("AGE"));
        Assert.AreEqual(2, jObject.Count);
        Assert.IsTrue(!string.IsNullOrWhiteSpace((string)jObject["NAME"]!));
        Assert.IsTrue((int)jObject["AGE"]! > 0);
    }

    [TestMethod]
    public void ExecuteTest_Select_ExecuteNonQuery_TableExists()
    {
        input.CommandText = "Select * from TaskTestTable;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsNull(result.Error);
        Assert.AreEqual(0, result.RecordsAffected);
    }

    [TestMethod]
    public void ExecuteTest_Select_ExecuteScalar_TableExists()
    {
        input.CommandText = "Select * from TaskTestTable;";
        input.CommandType = CommandTypes.ExecuteScalar;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(Names.Contains(result.Data.Value.ToString()));
        Assert.IsNull(result.Error);
        Assert.AreEqual(1, result.RecordsAffected);
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteReader_InvalidStatement_ThrowErrorOnFailure_True()
    {
        input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        input.CommandType = CommandTypes.ExecuteReader;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteReader_InvalidStatement_ThrowErrorOnFailure_False()
    {
        input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        input.CommandType = CommandTypes.ExecuteReader;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("is not recognized SqlState: 22018"));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteNonQuery_InvalidStatement_ThrowErrorOnFailure_True()
    {
        input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteNonQuery_InvalidStatement_ThrowErrorOnFailure_False()
    {
        input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("is not recognized SqlState: 22018"));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteScalar_InvalidStatement_ThrowErrorOnFailure_True()
    {
        input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteScalar_InvalidStatement_ThrowErrorOnFailure_False()
    {
        input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        input.CommandType = CommandTypes.ExecuteScalar;
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("is not recognized SqlState: 22018"));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteReader_TableExists()
    {
        input.CommandText = "update TaskTestTable set age = 21 where age = 20;";
        input.CommandType = CommandTypes.ExecuteReader;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.RecordsAffected > 0);

        // restore
        input.CommandText = "update TaskTestTable set age = 20 where age = 21;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteNonQuery_TableExists()
    {
        input.CommandText = "update TaskTestTable set age = 31 where age = 30;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.RecordsAffected > 0);

        // restore
        input.CommandText = "update TaskTestTable set age = 30 where age = 31;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteScalar_TableExists()
    {
        input.CommandText = "update TaskTestTable set age = 11 where age = 10;";
        input.CommandType = CommandTypes.ExecuteScalar;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Data.Value > 0);
        Assert.IsTrue(result.RecordsAffected > 0);

        // restore
        input.CommandText = "update TaskTestTable set age = 10 where age = 11;";
        input.CommandType = CommandTypes.ExecuteNonQuery;
        Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_IsNULL()
    {
        connection.ConnectionString = null;
        Assert.ThrowsException<Exception>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_Empty()
    {
        connection.ConnectionString = string.Empty;
        Assert.ThrowsException<Exception>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_Foo_ThrowErrorOnFailure_True()
    {
        connection.ConnectionString = "foo";
        Assert.ThrowsException<ArgumentException>(() => Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None));
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_Foo_ThrowErrorOnFailure_False()
    {
        connection.ConnectionString = "foo";
        options.ThrowErrorOnFailure = false;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Format of the initialization string does not conform to specification"));
    }

    [TestMethod]
    public void ExecuteTest_KeyPairAuth_ValidCredentials_ShouldReturnUser()
    {
        input.CommandText = "SELECT CURRENT_USER;";
        input.CommandType = CommandTypes.ExecuteScalar;
        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("FRENDSFSP", result.Data.Value.ToString().ToUpper());
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void ExecuteTest_KeyPairAuth_InvalidKeyFile_ShouldFail()
    {
        connection.PrivateKeyFilePath = "non_existing_key_file.p8";
        options.ThrowErrorOnFailure = false;

        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("Private key file not found"));
    }

    [TestMethod]
    public void ExecuteTest_KeyPairAuth_InvalidPassphrase_ShouldFail()
    {
        connection.PrivateKeyPassphrase = "WrongPassphrase";
        options.ThrowErrorOnFailure = false;

        var result = Snowflake.ExecuteQuery(input, connection, options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.Error.Message.Contains("problem creating ENCRYPTED private key"));
    }

    private static string? GetEnv(string name) => Environment.GetEnvironmentVariable(name);

    private static bool LoadEnv()
    {
        DotEnv.Load(new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 6));
        return true;
    }

    private static string? InitPrivateKeyFile()
    {
        var privateKeyB64 = GetEnv("Snowflake_PrivateKeyB64");
        if (string.IsNullOrWhiteSpace(privateKeyB64))
            return null;

        byte[] privateKeyBytes = Convert.FromBase64String(privateKeyB64);

        string tempFile = Path.GetTempFileName();
        string finalFile = Path.ChangeExtension(tempFile, ".p8");
        File.Move(tempFile, finalFile);
        File.WriteAllBytes(finalFile, privateKeyBytes);

        tempPrivateKeyFile = finalFile;
        return finalFile;
    }
}
