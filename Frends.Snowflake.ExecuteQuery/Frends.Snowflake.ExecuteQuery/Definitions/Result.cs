namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Represents the result of a Task.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the Task was executed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Records affected.
    /// Some statements will return -1.
    /// </summary>
    /// <example>100</example>
    public int RecordsAffected { get; private set; }

    /// <summary>
    /// Error message.
    /// This value is generated when an exception occurs and Options.ThrowErrorOnFailure = false.
    /// </summary>
    /// <example>Table 'NOTABLE' does not exist or not authorized...</example>
    public dynamic ErrorMessage { get; private set; }

    /// <summary>
    /// Procedure's result as JToken.
    /// </summary>
    /// <example>
    /// Examples using SELECT query
    /// Input.ExecuteType = ExecuteReader: [{"NAME": "Jane","AGE": 94},{"NAME": "Eve","AGE": 53}],
    /// Input.ExecuteType = NonQuery: null,
    /// Input.ExecuteType = Scalar: {"Value": "Jane"}
    /// </example>
    public dynamic Data { get; private set; }

    internal Result(bool success, int recordsAffected, dynamic errorMessage, dynamic data)
    {
        Success = success;
        RecordsAffected = recordsAffected;
        ErrorMessage = errorMessage;
        Data = data;
    }
}
