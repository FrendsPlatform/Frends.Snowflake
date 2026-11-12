namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Represents the result of a Task.
/// </summary>
public class Result
{
    internal Result(bool success, int recordsAffected, dynamic data, Error error = null)
    {
        Success = success;
        RecordsAffected = recordsAffected;
        Data = data;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the Task was executed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Records affected.
    /// Some statements will return -1.
    /// </summary>
    /// <example>100</example>
    public int RecordsAffected { get; set; }

    /// <summary>
    /// Procedure's result as JToken.
    /// </summary>
    /// <example>
    /// Examples using SELECT query
    /// Input.ExecuteType = ExecuteReader: [{"NAME": "Jane","AGE": 94},{"NAME": "Eve","AGE": 53}],
    /// Input.ExecuteType = NonQuery: null,
    /// Input.ExecuteType = Scalar: {"Value": "Jane"}
    /// </example>
    public dynamic Data { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
