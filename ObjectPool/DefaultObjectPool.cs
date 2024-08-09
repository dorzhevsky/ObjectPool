namespace ObjectPool
{
    public class DefaultObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        public DefaultObjectPool() { }
        public DefaultObjectPool(Settings settings) : base(settings) { }
        protected override Task<T> CreatePooledObject() => Task.FromResult(new T());
        protected override void ReleasePooledObject(T @object) { }
    }
}
