using System.Threading;
using Frends.Snowflake.ExecuteQuery;
using Frends.Snowflake.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.Snowflake.ExecuteQuery.Tests;

[TestClass]
public class ComplianceTests
{
    [TestMethod]
    public void ExecuteQuery_ReturnsStructuredErrorWhenThrowErrorOnFailureDisablesThrowing()
    {
        var options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = "Snowflake query execution failed",
        };
        var result = Snowflake.ExecuteQuery(
            new Input(),
            new Connection { ConnectionString = string.Empty },
            options,
            CancellationToken.None);

        Assert.IsFalse(options.ThrowErrorOnFailure);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(
            "Snowflake query execution failed: Validation failed:\nThe ConnectionString field is required.",
            result.Error.Message);
        Assert.IsNotNull(result.Error.AdditionalInfo);
    }
}
