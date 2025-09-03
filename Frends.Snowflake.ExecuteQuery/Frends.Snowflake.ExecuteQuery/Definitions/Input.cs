using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Snowflake.ExecuteQuery.Definitions;

/// <summary>
/// Connection and command parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// String that contains the necessary data to connect to the database.
    /// <example>
    /// account=testaccount-123;user=user;role=ACCOUNTADMIN;db=SHOP;schema=PUBLIC;
    /// authenticator=SNOWFLAKE_JWT;private_key_file=C:/snow/rsa_key.p8;private_key_pwd=MyStrongPassphrase
    /// </example>
    [PasswordPropertyText]
    public string ConnectionString { get; set; }

    /// <summary>
    /// Specifies how a command string is interpreted.
    /// ExecuteNonQuery: Executes an SQL statement, and returns the number of rows affected.
    /// ExecuteReader: Executes the query, and returns an object that can iterate over the entire result set.
    /// ExecuteScalar: Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
    /// </summary>
    public CommandTypes CommandType { get; set; }

    /// <summary>
    /// text command to run against the data source.
    /// </summary>
    /// <example>SELECT * FROM mytable</example>
    public string CommandText { get; set; }

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