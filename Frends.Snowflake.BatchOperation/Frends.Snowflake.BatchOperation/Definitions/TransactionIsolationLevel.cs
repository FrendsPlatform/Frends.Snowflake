namespace Frends.Snowflake.BatchOperation.Definitions;

public enum TransactionIsolationLevel
{
    Default = 1,
    ReadCommitted = 2,
    None = 3,
    Serializable = 4,
    ReadUncommitted = 5,
    RepeatableRead = 6,
    Snapshot = 7,
}
