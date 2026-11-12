using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Options for controlling the behavior of a Task.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets a value indicating whether an error should stop the Task and throw an exception.
    /// Kept as an alias for ThrowErrorOnFailure for compatibility with existing configurations.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowExceptionOnError
    {
        get => ThrowErrorOnFailure;
        set => ThrowErrorOnFailure = value;
    }

    /// <summary>
    /// Gets or sets the wait time (in seconds) before terminating the attempt to execute a command and generating an error.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int TimeOut { get; set; }

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Snowflake query execution failed</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
