namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Command parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Specifies how a command string is interpreted.
    /// ExecuteNonQuery: Executes an SQL statement, and returns the number of rows affected.
    /// ExecuteReader: Executes the query, and returns an object that can iterate over the entire result set.
    /// ExecuteScalar: Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
    /// </summary>
    /// <example>ExecuteReader</example>
    public CommandTypes CommandType { get; set; }

    /// <summary>
    /// text command to run against the data source.
    /// </summary>
    /// <example>SELECT * FROM mytable</example>
    public string CommandText { get; set; }
}
