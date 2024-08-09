namespace ObjectPool
{
    public abstract class ObjectPool<T> : IDisposable
    {
        private readonly Settings _settings;
        private readonly int[] _slots;
        private readonly PooledObject<T>?[] _pooledObjects;
        private readonly SemaphoreSlim _semaphore;
        private readonly Timer _evictionTimer;

        private volatile bool _disposed;
        private volatile bool _disposing;

        public ObjectPool() : this(Settings.Default)
        {
        }

        public ObjectPool(Settings settings)
        {
            _settings = settings;

            _slots = Enumerable.Repeat(SlotState.Free, _settings.MaxPoolSize).ToArray();
            _pooledObjects = Enumerable.Repeat<PooledObject<T>?>(null, _settings.MaxPoolSize).ToArray();
            _semaphore = new SemaphoreSlim(settings.ConcurrencyFactor);
            _evictionTimer = new Timer(Evict, this, 0, _settings.EvictionInterval);
        }

        public Task<PooledObject<T>> Get() => Get(CancellationToken.None);

        public async Task<PooledObject<T>> Get(CancellationToken cancellationToken)
        {            
            try
            {
                ThrowIfDisposed();

                var timedCancellationToken = cancellationToken.CancelAfter(_settings.WaitingTimeout);

                await _semaphore.WaitAsync(timedCancellationToken);

                var backoff = new BackoffStrategy(_settings.BackoffDelayMilliseconds, _settings.BackoffMaxDelayMilliseconds);

                while (true)
                {
                    ThrowIfDisposed();

                    for (var i = 0; i < _settings.MaxPoolSize; i++)
                    {
                        if (_disposing)
                        {
                            break;
                        }

                        timedCancellationToken.ThrowIfCancellationRequested();

                        if (Interlocked.CompareExchange(ref _slots[i], SlotState.Busy, SlotState.Free) == SlotState.Free)
                        {
                            try
                            {
                                if (_pooledObjects[i] is null)
                                {
                                    var obj = await CreatePooledObject().ConfigureAwait(false);
                                    _pooledObjects[i] = new PooledObject<T>(this, obj) { Slot = i };
                                }
                                var pooledObject = _pooledObjects[i];
                                return pooledObject!;
                            }
                            catch (Exception)
                            {
                                Interlocked.Exchange(ref _slots[i], SlotState.Free);
                            }
                        }
                    }

                    await backoff.Delay(timedCancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                throw new ObjectPoolCancellationException(ErrorMessages.ObjectPoolCancellationExceptionMessage);
            }
            finally
            {                
                _semaphore.Release();
            }
        }

        internal void Release(int slot)
        {
            ThrowIfDisposed();
            Interlocked.Exchange(ref _slots[slot], SlotState.Free);
        }

        private void Evict(object? state)
        {
            ThrowIfDisposed();

            var i = 0;
            for (; i < _settings.MaxPoolSize; i++)
            {
                if (Interlocked.CompareExchange(ref _slots[i], SlotState.Busy, SlotState.Free) == SlotState.Free)
                {
                    var pooledObject = _pooledObjects[i];

                    try
                    {
                        if (pooledObject is not null)
                        {
                            ReleasePooledObject(pooledObject.Object);
                            _pooledObjects[i] = null;
                        }
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _slots[i], SlotState.Free);
                    }
                }
            }
        }

        protected abstract Task<T> CreatePooledObject();
        protected abstract void ReleasePooledObject(T @object);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                _disposing = true;
                if (!_disposed)
                {
                    CleanUp();

                    if (disposing)
                    {
                        _semaphore?.Dispose();
                        _evictionTimer?.Dispose();
                    }

                    _disposed = true;
                }
            }
            catch
            {
                _disposing = false;
            }
        }

        private void CleanUp()
        {
            var done = false;
            while (!done)
            {
                var i = 0;
                for (; i < _settings.MaxPoolSize; i++)
                {
                    if (Interlocked.CompareExchange(ref _slots[i], SlotState.Disposed, SlotState.Free) == SlotState.Free)
                    {
                        var pooledObject = _pooledObjects[i];

                        if (pooledObject is not null)
                        {
                            ReleasePooledObject(pooledObject.Object);
                            _pooledObjects[i] = null;
                        }
                    }
                }

                done = _slots.All(e => e == SlotState.Disposed);
            }            
        }

        ~ObjectPool()
        {
            Dispose(false);
        }
    }
}
