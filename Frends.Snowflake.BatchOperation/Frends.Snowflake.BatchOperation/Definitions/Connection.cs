using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Snowflake.BatchOperation.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Connection string to Snowflake.
    /// </summary>
    /// <example>Host=127.0.0.1;Port=5432</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ConnectionString { get; set; } = string.Empty;
}
