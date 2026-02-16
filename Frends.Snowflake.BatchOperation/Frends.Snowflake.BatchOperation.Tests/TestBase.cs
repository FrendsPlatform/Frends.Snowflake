using System;
using dotenv.net;

namespace Frends.Snowflake.BatchOperation.Tests;

public abstract class TestBase
{
    protected TestBase()
    {
        DotEnv.Load();
        ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    }

    protected string ConnectionString { get; }
}
