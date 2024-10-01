namespace ObjectPool
{
    public class DefaultObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        public DefaultObjectPool() { }
        public DefaultObjectPool(Settings settings) : base(settings) { }
        protected override T Create() => new();

        protected override Task Activate(T @object)
        {
            OnActivated();
            return Task.CompletedTask;
        }

        protected override void Deactivate(T @object) => OnDeactivated();
    }
}
