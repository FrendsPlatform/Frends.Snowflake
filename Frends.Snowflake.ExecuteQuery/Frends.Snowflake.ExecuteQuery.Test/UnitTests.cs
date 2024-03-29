using Frends.Snowflake.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snowflake.Data.Client;
using System;
using System.Collections.Generic;

namespace Frends.Snowflake.ExecuteQuery.Tests;

[TestClass]
public class UnitTests
{

    private static readonly string? _connectionString = Environment.GetEnvironmentVariable("Snowflake_ConnectionString");
    private static Input _input = new();
    private static Options _options = new();
    private static readonly List<string> _names = new() { "John", "Jane", "Tom", "Alice", "Bob", "Charlie", "David", "Eve", "Mallory", "Oscar" };
    private static readonly Random _random = new();
    private static readonly int _age = _random.Next(1, 101);


    [TestInitialize]
    public void Setup()
    {
        _input = new Input()
        {
            ConnectionString = _connectionString,
            CommandText = null,
            CommandType = CommandTypes.ExecuteNonQuery,
        };

        _options = new Options()
        {
            ThrowExceptionOnError = true,
            TimeOut = 30
        };
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_TableExists()
    {
        _input.CommandText = @$"insert into TaskTestTable values ('{_names[_random.Next(_names.Count)]}', 10);";
        _input.CommandType = CommandTypes.ExecuteReader;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.RecordsAffected);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_TableExists()
    {
        _input.CommandText = @$"insert into TaskTestTable values ('{_names[_random.Next(_names.Count)]}', 20);";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_TableExists()
    {
        _input.CommandText = @$"insert into TaskTestTable values ('{_names[_random.Next(_names.Count)]}', 30);";
        _input.CommandType = CommandTypes.ExecuteScalar;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_TableDoesntExists_ThrowExceptionOnError_True()
    {
        _input.CommandText = "insert into NoTable values ('A', 1);";
        _input.CommandType = CommandTypes.ExecuteReader;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_TableDoesntExists_ThrowExceptionOnError_False()
    {
        _input.CommandText = "insert into NoTable values ('A', 1);";
        _input.CommandType = CommandTypes.ExecuteReader;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("Table 'NOTABLE' does not exist or not authorized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_TableDoesntExists_ThrowExceptionOnError_True()
    {
        _input.CommandText = "insert into NoTable values ('A', 1);";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_TableDoesntExists_ThrowExceptionOnError_False()
    {
        _input.CommandText = "insert into NoTable values ('A', 1);";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("Table 'NOTABLE' does not exist or not authorized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_TableDoesntExists_ThrowExceptionOnError_True()
    {
        _input.CommandText = "insert into NoTable values ('A', 1);";
        _input.CommandType = CommandTypes.ExecuteScalar;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_TableDoesntExists_ThrowExceptionOnError_False()
    {
        _input.CommandText = "insert into NoTable values ('A', 1);";
        _input.CommandType = CommandTypes.ExecuteScalar;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("Table 'NOTABLE' does not exist or not authorized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_InvalidStatement_ThrowExceptionOnError_True()
    {
        _input.CommandText = "insert into TaskTestTable values (1, 'A');";
        _input.CommandType = CommandTypes.ExecuteReader;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteReader_InvalidStatement_ThrowExceptionOnError_False()
    {
        _input.CommandText = "insert into TaskTestTable values (1, 'A');";
        _input.CommandType = CommandTypes.ExecuteReader;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("Numeric value 'A' is not recognized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_InvalidStatement_ThrowExceptionOnError_True()
    {
        _input.CommandText = "insert into TaskTestTable values (1, 'A');";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteNonQuery_InvalidStatement_ThrowExceptionOnError_False()
    {
        _input.CommandText = "insert into TaskTestTable values (1, 'A');";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("Numeric value 'A' is not recognized"));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_InvalidStatement_ThrowExceptionOnError_True()
    {
        _input.CommandText = "insert into TaskTestTable values (1, 'A');";
        _input.CommandType = CommandTypes.ExecuteScalar;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Insert_ExecuteScalar_InvalidStatement_ThrowExceptionOnError_False()
    {
        _input.CommandText = "insert into TaskTestTable values (1, 'A');";
        _input.CommandType = CommandTypes.ExecuteScalar;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("Numeric value 'A' is not recognized"));
    }

    [TestMethod]
    public void ExecuteTest_Select_ExecuteReader_TableExists()
    {
        _input.CommandText = "Select * from TaskTestTable;";
        _input.CommandType = CommandTypes.ExecuteReader;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Data.Rows.Count > 0);
        Assert.IsNull(result.ErrorMessage);
        Assert.AreEqual(-1, result.RecordsAffected);
    }

    [TestMethod]
    public void ExecuteTest_Select_ExecuteNonQuery_TableExists()
    {
        _input.CommandText = "Select * from TaskTestTable;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsNull(result.ErrorMessage);
        Assert.AreEqual(0, result.RecordsAffected);
    }

    [TestMethod]
    public void ExecuteTest_Select_ExecuteScalar_TableExists()
    {
        _input.CommandText = "Select * from TaskTestTable;";
        _input.CommandType = CommandTypes.ExecuteScalar;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(_names.Contains(result.Data.Value.ToString()));
        Assert.IsNull(result.ErrorMessage);
        Assert.AreEqual(1, result.RecordsAffected);
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteReader_InvalidStatement_ThrowExceptionOnError_True()
    {
        _input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        _input.CommandType = CommandTypes.ExecuteReader;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteReader_InvalidStatement_ThrowExceptionOnError_False()
    {
        _input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        _input.CommandType = CommandTypes.ExecuteReader;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("is not recognized SqlState: 22018"));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteNonQuery_InvalidStatement_ThrowExceptionOnError_True()
    {
        _input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteNonQuery_InvalidStatement_ThrowExceptionOnError_False()
    {
        _input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("is not recognized SqlState: 22018"));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteScalar_InvalidStatement_ThrowExceptionOnError_True()
    {
        _input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteScalar_InvalidStatement_ThrowExceptionOnError_False()
    {
        _input.CommandText = "update TaskTestTable set age = 'B' where name = 10;";
        _input.CommandType = CommandTypes.ExecuteScalar;
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("is not recognized SqlState: 22018"));
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteReader_TableExists()
    {
        _input.CommandText = "update TaskTestTable set age = 21 where age = 20;";
        _input.CommandType = CommandTypes.ExecuteReader;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.RecordsAffected > 0);

        // restore
        _input.CommandText = "update TaskTestTable set age = 20 where age = 21;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        Snowflake.ExecuteQuery(_input, _options);
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteNonQuery_TableExists()
    {
        _input.CommandText = "update TaskTestTable set age = 31 where age = 30;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.RecordsAffected > 0);

        // restore
        _input.CommandText = "update TaskTestTable set age = 30 where age = 31;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        Snowflake.ExecuteQuery(_input, _options);
    }

    [TestMethod]
    public void ExecuteTest_Update_ExecuteScalar_TableExists()
    {
        _input.CommandText = "update TaskTestTable set age = 11 where age = 10;";
        _input.CommandType = CommandTypes.ExecuteScalar;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Data.Value > 0);
        Assert.IsTrue(result.RecordsAffected > 0);

        // restore
        _input.CommandText = "update TaskTestTable set age = 10 where age = 11;";
        _input.CommandType = CommandTypes.ExecuteNonQuery;
        Snowflake.ExecuteQuery(_input, _options);
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_IsNULL()
    {
        _input.ConnectionString = null;
        Assert.ThrowsException<Exception>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_Empty()
    {
        _input.ConnectionString = "";
        Assert.ThrowsException<Exception>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_Foo_ThrowExceptionOnError_True()
    {
        _input.ConnectionString = "foo";
        Assert.ThrowsException<SnowflakeDbException>(() => Snowflake.ExecuteQuery(_input, _options));
    }

    [TestMethod]
    public void ExecuteTest_InvalidConnectionString_Foo_ThrowExceptionOnError_False()
    {
        _input.ConnectionString = "foo";
        _options.ThrowExceptionOnError = false;
        var result = Snowflake.ExecuteQuery(_input, _options);
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Data);
        Assert.IsTrue(result.ErrorMessage.Message.Contains("Error: Connection string is invalid: Format of the initialization string does not conform"));
    }
}