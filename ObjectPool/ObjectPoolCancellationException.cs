namespace ObjectPool
{
    public class ObjectPoolCancellationException : Exception
    {
        public ObjectPoolCancellationException()
        {
        }

        public ObjectPoolCancellationException(string message)
            : base(message)
        {
        }

        public ObjectPoolCancellationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
