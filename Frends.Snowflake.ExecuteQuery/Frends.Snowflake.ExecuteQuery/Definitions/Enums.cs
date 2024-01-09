namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Command types.
/// </summary>
public enum CommandTypes
{
    /// <summary>
    /// Executes an SQL statement, and returns the number of rows affected.
    /// </summary>
    ExecuteNonQuery,
    /// <summary>
    /// Executes the query, and returns an object that can iterate over the entire result set.
    /// </summary>
    ExecuteReader,
    /// <summary>
    /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
    /// </summary>
    ExecuteScalar
}