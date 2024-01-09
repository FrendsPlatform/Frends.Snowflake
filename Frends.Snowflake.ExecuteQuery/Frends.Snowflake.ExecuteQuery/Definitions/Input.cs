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
    /// </summary>
    /// <example>account=myaccount;user=myuser;password=mypassword;warehouse=mywarehouse;role=myrole;db=mydb;schema=myschema</example>
    [DisplayFormat(DataFormatString = "Text")]
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
}