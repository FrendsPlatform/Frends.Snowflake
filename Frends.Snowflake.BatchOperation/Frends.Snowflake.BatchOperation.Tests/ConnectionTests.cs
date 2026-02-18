using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.Snowflake.BatchOperation.Tests;

public class ConnectionTests : TestBase
{
    [Test]
    public async Task ShouldFailWithInvalidConnectionString()
    {
        var conn = DefaultConnection();
        conn.ConnectionString = "INVALID CONNECTION STRING";

        var result = await Snowflake.BatchOperation(DefaultInput(), conn, DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("Error building connection string."));
        Assert.That(result.AffectedRows, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldFailWithInvalidUser()
    {
        var conn = DefaultConnection();
        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = conn.ConnectionString,
        };
        csb["user"] = "invalidUser";
        conn.ConnectionString = csb.ConnectionString;

        var result = await Snowflake.BatchOperation(DefaultInput(), conn, DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("Unable to connect"));
        Assert.That(result.AffectedRows, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldFailWithMissingPrivateKey()
    {
        var conn = DefaultConnection();
        var invalidKeyPath = Path.GetTempFileName();
        conn.PrivateKeyFilePath = invalidKeyPath;

        try
        {
            var result = await Snowflake.BatchOperation(DefaultInput(), conn, DefaultOptions(), CancellationToken.None);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Message, Contains.Substring("Unable to connect"));
            Assert.That(result.AffectedRows, Is.EqualTo(0));
        }
        finally
        {
            File.Delete(invalidKeyPath);
        }
    }
}
