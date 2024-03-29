﻿using Frends.Snowflake.ExecuteQuery.Definitions;
using Newtonsoft.Json.Linq;
using Snowflake.Data.Client;
using System;
using System.ComponentModel;
using System.Data;

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
            conn.ConnectionString = input.ConnectionString;
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
}