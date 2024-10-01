namespace ObjectPool
{
    public class DefaultObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        public DefaultObjectPool(Func<T> factory) : base(factory) { }
        public DefaultObjectPool(Settings settings, Func<T> factory) : base(settings, factory) { }
        protected override Task Activate(T @object)
        {
            OnActivated();
            return Task.CompletedTask;
        }
        protected override void Deactivate(T @object) => OnDeactivated();
    }
}
