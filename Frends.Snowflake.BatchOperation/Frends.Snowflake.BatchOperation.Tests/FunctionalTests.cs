using System.Threading;
using System.Threading.Tasks;
using Frends.Snowflake.BatchOperation.Definitions;
using NUnit.Framework;

namespace Frends.Snowflake.BatchOperation.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    [Test]
    public async Task ShouldRepeatContentWithDelimiter()
    {
        var input = new Input
        {
            Query = "INSERT INTO TABLE (Col1, Col2) DB VALUES ()",
            JsonData = """
                       [
                         {
                           "ParameterName1": "Value1",
                           "ParameterName2": 123
                         },
                         {
                           "ParameterName1": "Value2",
                           "ParameterName2": 456
                         }
                       ]
                       """,
        };

        var connection = new Connection
        {
            ConnectionString = ConnectionString,
        };

        var options = new Options
        {
            ThrowErrorOnFailure = true,
            ErrorMessageOnFailure = null,
        };

        var result = await Snowflake.BatchOperation(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
    }
}
