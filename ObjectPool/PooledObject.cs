using System.Runtime;

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
        public T Object { get; internal set; }
        internal int Slot { get; set; }
        internal DateTime? LastUsedTimestamp { get; set; }
        internal bool IsNotUsable(Settings settings) => LastUsedTimestamp.HasValue
                                && (DateTime.UtcNow - LastUsedTimestamp.Value).TotalMilliseconds > settings.EvictionTimeout;
    }
}