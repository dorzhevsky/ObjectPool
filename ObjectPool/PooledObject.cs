namespace ObjectPool
{
    public class PooledObject<T> : IDisposable
    {
        private readonly ObjectPool<T> _objectPool;

        public PooledObject(ObjectPool<T> objectPool, T @object)
        {
            _objectPool = objectPool;
            Object = @object;
        }

        public void Dispose() => _objectPool.Release(Slot);
        public T Object { get; set; }
        public int Slot { get; internal set; }
        public DateTime? LastUsedTimestamp { get; internal set; }
    }
}