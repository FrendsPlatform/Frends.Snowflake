using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// String that contains the necessary data to connect to the database.
    /// </summary>
    /// <example>
    /// account=testaccount-123;user=user;role=ACCOUNTADMIN;db=SHOP;schema=PUBLIC;
    /// authenticator=SNOWFLAKE_JWT;private_key_file=C:/snow/rsa_key.p8;private_key_pwd=MyStrongPassphrase
    /// </example>
    [PasswordPropertyText]
    [Required]
    public string ConnectionString { get; set; }

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
