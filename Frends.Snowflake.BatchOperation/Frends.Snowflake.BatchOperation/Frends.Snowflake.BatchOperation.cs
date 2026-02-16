using System;
using System.ComponentModel;
using System.Linq;
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
    /// Task to run a batch operation in Snowflake
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
            await using var conn = new SnowflakeDbConnection();
            conn.ConnectionString = SnowflakeHandler.BuildConnectionString(connection);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var cmd = conn.CreateCommand();

            var data = SnowflakeHandler.GetData(input.JsonData);

            try
            {
                cmd.CommandText = "BEGIN TRANSACTION";
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                cmd.CommandText = input.Query;

                foreach (var row in data)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = row.Key;
                    param.Value = row.Value.ToArray();
                    param.DbType = SnowflakeHandler.GetDbType(row.Value.First());
                    cmd.Parameters.Add(param);
                }

                var affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                cmd.CommandText = "COMMIT";
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                return new Result
                {
                    Success = true,
                    AffectedRows = affectedRows,
                };
            }
            catch (Exception e)
            {
                cmd.CommandText = "ROLLBACK";
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                throw new Exception("Transaction failed. Rolled back.", e);
            }
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
