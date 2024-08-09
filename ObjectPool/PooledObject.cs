namespace ObjectPool
{
    public class PooledObject<T> : IDisposable
    {
        private readonly ObjectPool<T> _objectPool;
        private readonly T _object;

        public PooledObject(ObjectPool<T> objectPool, T @object)
        {
            _objectPool = objectPool;
            _object = @object;
        }

        public void Dispose()
        {
            _objectPool.Release(Slot);
        }

        ~PooledObject()
        {
            _objectPool.Release(Slot);
        }

        public T Object => _object;

        public int Slot { get; internal set; }
    }
}
