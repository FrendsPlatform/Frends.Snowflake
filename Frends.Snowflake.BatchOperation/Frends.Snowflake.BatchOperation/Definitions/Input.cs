using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Snowflake.BatchOperation.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Query to perform.
    /// </summary>
    /// <example>INSERT INTO TableName (NAME, AGE) VALUES (:Name, :Age)</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Query { get; set; }

    /// <summary>
    /// Json data in a specific format
    /// </summary>
    /// <example>
    /// [
    ///     {
    ///         "Name": "Matti",
    ///         "Age": 12
    ///     },
    ///     {
    ///         "Name": "Joni",
    ///         "Age": 34
    ///     }
    /// ]
    /// </example>
    [DefaultValue("")]
    public string JsonData { get; set; }
}
