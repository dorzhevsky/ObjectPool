namespace ObjectPool
{
    internal static class ErrorMessages
    {
        public const string EmptyPoolName = "Pool name must not be empty";
        public const string NegativeOrZeroMaximumPoolSize = "Maximum pool size must be greater than zero.";
        public const string NegativeOrZeroConcurrencyFactor = "Concurrency factor must be greater than zero.";
        public const string ObjectPoolCancellationExceptionMessage = "Timeout waiting for free slot in connection pool or operation was cancelled";
    }
}
