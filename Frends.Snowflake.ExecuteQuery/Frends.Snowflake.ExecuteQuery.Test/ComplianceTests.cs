using System.Threading;
using Frends.Snowflake.ExecuteQuery;
using Frends.Snowflake.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.Snowflake.ExecuteQuery.Tests;

[TestClass]
public class ComplianceTests
{
    [TestMethod]
    public void ExecuteQuery_ReturnsStructuredErrorWhenLegacyOptionDisablesThrowing()
    {
        var options = new Options
        {
            ThrowExceptionOnError = false,
            ErrorMessageOnFailure = "Snowflake query execution failed",
        };
        var result = Snowflake.ExecuteQuery(
            new Input { ConnectionString = string.Empty },
            options,
            CancellationToken.None);

        Assert.IsFalse(options.ThrowErrorOnFailure);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Snowflake query execution failed: Invalid connection string.", result.Error.Message);
        Assert.IsNotNull(result.Error.AdditionalInfo);
    }
}
