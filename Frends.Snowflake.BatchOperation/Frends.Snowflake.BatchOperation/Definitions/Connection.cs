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
    /// <example>account=myaccount;host=myaccount.snowflakecomputing.com;user=myuser;db=mydb;schema=public</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Full file path to the private key (.p8) used for Snowflake key pair authentication.
    /// </summary>
    /// <example>C:\keys\rsa_key.p8</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string PrivateKeyFilePath { get; set; }

    /// <summary>
    /// Optional passphrase for the private key file, if the key was generated with encryption.
    /// Leave empty if the key file is unencrypted.
    /// </summary>
    /// <example>MySuperSecret123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    public string PrivateKeyPassphrase { get; set; }
}
