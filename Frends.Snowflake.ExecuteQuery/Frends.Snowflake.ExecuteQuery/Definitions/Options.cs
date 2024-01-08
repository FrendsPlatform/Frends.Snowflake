using System.ComponentModel;

namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Options for controlling the behavior of a Task.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets a value indicating whether an error should stop the Task and throw an exception.
    /// If set to true, an exception will be thrown when an error occurs. If set to false, Task will try to continue and the error message will be added into Result.ErrorMessage and Result.Success will be set to false.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowExceptionOnError { get; set; }

    /// <summary>
    /// Gets or sets the wait time (in seconds) before terminating the attempt to execute a command and generating an error.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int TimeOut { get; set; }
}