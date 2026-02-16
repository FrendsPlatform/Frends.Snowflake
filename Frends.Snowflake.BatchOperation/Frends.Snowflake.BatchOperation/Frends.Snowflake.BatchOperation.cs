using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frends.Snowflake.BatchOperation.Definitions;
using Frends.Snowflake.BatchOperation.Helpers;
using Snowflake.Data.Client;

namespace Frends.Snowflake.BatchOperation;

/// <summary>
/// Task Class for Snowflake operations.
/// </summary>
public static class Snowflake
{
    /// <summary>
    /// Task to run batch operation in Snowflake
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Snowflake-BatchOperation)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> BatchOperation(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Parse and validate JSON
            using JsonDocument doc = JsonDocument.Parse(input.JsonData);
            JsonElement root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("Input JSON must be an array of arrays.");

            int rowCount = root.GetArrayLength();
            if (rowCount == 0) throw new Exception("Data in json can't be empty");

            var firstElement = root[0];
            if (firstElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("Elements in array must be objects.");

            List<string> columnNames = firstElement.EnumerateObject().Select(p => p.Name).ToList();
            int colCount = columnNames.Count;

            // // 2. Transpose data (Row-based to Column-based)
            // var columns = new object[colCount][];
            // for (int i = 0; i < colCount; i++) columns[i] = new object[rowCount];
            //
            // int rowIndex = 0;
            // foreach (JsonElement row in root.EnumerateArray())
            // {
            //     if (row.GetArrayLength() != colCount)
            //         throw new ArgumentException($"Row at index {rowIndex} has invalid column count.");
            //
            //     int colIndex = 0;
            //     foreach (JsonElement cell in row.EnumerateArray())
            //     {
            //         columns[colIndex][rowIndex] = GetValue(cell);
            //         colIndex++;
            //     }
            //
            //     rowIndex++;
            // }

            // 3. Execute in Snowflake within Transaction
            await using var conn = new SnowflakeDbConnection();
            conn.ConnectionString = connection.ConnectionString;
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            // Start transaction with requested IsolationLevel
            await using var transaction = await conn
                .BeginTransactionAsync(options.IsolationLevel.ToIsolationLevel(), cancellationToken)
                .ConfigureAwait(false);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction; // Assign transaction to command
                cmd.CommandText = input.Query;

                for (int i = 0; i < colCount; i++)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = $"{i + 1}";
                    param.Value = "test";
                    param.DbType = GetDbType("string");
                    cmd.Parameters.Add(param);
                }

                // cmd.ArrayBindCount = rowCount;

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }

        return new Result();
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

    private static DbType GetDbType(object value)
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
}
