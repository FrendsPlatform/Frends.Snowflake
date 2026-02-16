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
    /// <example>foobar</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("Lorem ipsum dolor sit amet.")]
    public string Query { get; set; }

    /// <summary>
    /// Json data in specific format
    /// </summary>
    /// <example>2</example>
    [DefaultValue(3)]
    public string JsonData { get; set; }
}
