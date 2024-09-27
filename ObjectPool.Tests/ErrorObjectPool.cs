namespace ObjectPool
{
    public class ErrorObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        private static int _counter = 0;
        public ErrorObjectPool() { }
        public ErrorObjectPool(Settings settings) : base(settings) { }
        protected override T Create()
        {
            return new T();
        }

        protected override Task Activate(T @object)
        {
            if (_counter++ == 0)
            {
                throw new Exception();
            }
            return Task.CompletedTask;
        }

        protected override void Deactivate(T @object)
        {
        }
    }
}
