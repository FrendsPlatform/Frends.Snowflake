using System.Threading;
using System.Threading.Tasks;
using Frends.Snowflake.BatchOperation.Definitions;
using NUnit.Framework;

namespace Frends.Snowflake.BatchOperation.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    [Test]
    public async Task ShouldInsertSimpleData()
    {
        var input = new Input
        {
            Query = "INSERT INTO TaskTestTable (NAME, AGE) VALUES (:NAME, :AGE)",
            JsonData = """
                       [
                         {
                           "NAME": "Mat",
                           "AGE": 12
                         },
                         {
                           "NAME": "Mon",
                           "AGE": 34
                         }
                       ]
                       """,
        };

        var result =
            await Snowflake.BatchOperation(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.AffectedRows, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldInsertIncompleteData()
    {
        var input = new Input
        {
            Query = "INSERT INTO TaskTestTable (NAME, AGE) VALUES (:NAME, :AGE)",
            JsonData = """
                       [
                            {
                                "NAME": "Mat"
                            },
                            {
                                "NAME": "Mon",
                                "AGE": 34
                            },
                            {
                                "NAME": "Jefim",
                                "AGE": 11
                            },
                            {
                                "NAME": "Michal"
                            },
                            {
                                "NAME": "Jacek",
                                "AGE": 22
                            }
                       ]
                       """,
        };

        var result =
            await Snowflake.BatchOperation(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.AffectedRows, Is.EqualTo(5));
    }

    [Test]
    public async Task ShouldInsertIncompleteDataAtTheEnd()
    {
        var input = new Input
        {
            Query = "INSERT INTO TaskTestTable (NAME, AGE) VALUES (:NAME, :AGE)",
            JsonData = """
                       [
                            {
                                "NAME": "Mat"
                            },
                            {
                                "NAME": "Mon",
                                "AGE": 34
                            },
                            {
                                "NAME": "Jefim",
                                "AGE": 11
                            },
                            {
                                "NAME": "Michal"
                            },
                            {
                                "NAME": "Jacek"
                            }
                       ]
                       """,
        };

        var result =
            await Snowflake.BatchOperation(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.AffectedRows, Is.EqualTo(5));
    }

    [Test]
    public async Task ShouldFailWithInvalidDataFormat()
    {
        var input = new Input
        {
            Query = "INSERT INTO TaskTestTable (NAME, AGE) VALUES (:NAME, :AGE)",
            JsonData = """
                       [
                         {
                           "NAME": "Mat",
                           "AGE": 12
                         },
                       ]
                       """,
        };

        var result =
            await Snowflake.BatchOperation(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.AffectedRows, Is.EqualTo(0));
        Assert.That(result.Error.Message, Is.Not.Empty);
    }

    [Test]
    public async Task ShouldRollbackWithInvalidQuery()
    {
        var input = new Input
        {
            Query = "INVALID QUERY",
            JsonData = """
                       [
                         {
                           "NAME": "Mat",
                           "AGE": 12
                         },
                         {
                           "NAME": "Mon ",
                           "AGE": 34
                         }
                       ]
                       """,
        };

        var result =
            await Snowflake.BatchOperation(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.AffectedRows, Is.EqualTo(0));
        Assert.That(result.Error.Message, Contains.Substring("Transaction failed. Rolled back."), result.Error.Message);
    }

    [Test]
    public async Task ShouldRollbackWithInvalidQueryParams()
    {
        var input = new Input
        {
            Query = "INSERT INTO TaskTestTable (NAME, AGE) VALUES (:col1, :col2)",
            JsonData = """
                       [
                         {
                           "NAME": "Mat",
                           "AGE": 12
                         },
                         {
                           "NAME": "Mon ",
                           "AGE": 34
                         }
                       ]
                       """,
        };

        var result =
            await Snowflake.BatchOperation(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.AffectedRows, Is.EqualTo(0));
        Assert.That(result.Error.Message, Contains.Substring("Transaction failed. Rolled back."), result.Error.Message);
    }

    [Test]
    public async Task ShouldUseCorrectTypesData()
    {
        var input = new Input
        {
            Query = "INSERT INTO TaskTestTable (NAME, AGE, DOUBLEVAL) VALUES (:NAME, :AGE, :DOUBLEVAL)",
            JsonData = """
                       [
                            {
                                "NAME": "Mon",
                                "AGE": 34.3
                            },
                            {
                                "NAME": "Mat",
                                "DOUBLEVAL": 11.5
                            }
                       ]
                       """,
        };

        var result =
            await Snowflake.BatchOperation(input, DefaultConnection(), DefaultOptions(), CancellationToken.None);
        var monData = await ExecuteReader("SELECT AGE FROM TaskTestTable WHERE NAME = 'Mon';");
        var matData = await ExecuteReader("SELECT DOUBLEVAL FROM TaskTestTable WHERE NAME = 'Mat';");

        Assert.That(result.Success, Is.True);
        Assert.That(result.AffectedRows, Is.EqualTo(2));
        Assert.That(monData, Is.EqualTo("34"));
        Assert.That(matData, Is.EqualTo("11.5"));
    }
}
