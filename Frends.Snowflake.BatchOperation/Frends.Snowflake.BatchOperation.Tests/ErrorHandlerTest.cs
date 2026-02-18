using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.Snowflake.BatchOperation.Tests;

[TestFixture]
public class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = true;
        var ex = Assert.ThrowsAsync<Exception>(() =>
            Snowflake.BatchOperation(DefaultInput(), DefaultConnection(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var result = await Snowflake.BatchOperation(
            DefaultInput(),
            DefaultConnection(),
            DefaultOptions(),
            CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = true;
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.ThrowsAsync<Exception>(() =>
            Snowflake.BatchOperation(DefaultInput(), DefaultConnection(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}
