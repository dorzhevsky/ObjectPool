namespace ConsoleApp1
{
    public abstract class ObjectPool<T>: IDisposable where T: class
    {
        private readonly Settings _settings;
        private readonly int _maxPoolSize;
        private readonly int _waitTimeout;
        private readonly int[] _slots;
        private readonly PooledObject<T>?[] _pooledObjects;
        private readonly SemaphoreSlim _semaphore;
        private readonly ReaderWriterLockSlim _readerWriterLock;
        private readonly Timer _evictionTimer;

        private bool _disposed;

        public ObjectPool(Settings settings)
        {
            _settings = settings;
            _maxPoolSize = _settings.MaxPoolSize;
            _waitTimeout = _settings.WaitTimeout;

            _slots = Enumerable.Repeat(SlotState.Free, _maxPoolSize).ToArray();
            _pooledObjects = Enumerable.Repeat<PooledObject<T>?>(null, _maxPoolSize).ToArray();
            _semaphore = new SemaphoreSlim(_maxPoolSize*Environment.ProcessorCount);
            _readerWriterLock = new ReaderWriterLockSlim();
            _evictionTimer = new Timer(Evict, this, 0, 100);
        }

        public Task<PooledObject<T>> Get()
        {
            ThrowIfDisposed();
            return Get(CancellationToken.None);
        }

        public async Task<PooledObject<T>> Get(CancellationToken cancellationToken)
        {
            var timedCancellationToken = cancellationToken.CancelAfter(_waitTimeout);

            try
            {
                _readerWriterLock.EnterReadLock();

                ThrowIfDisposed();

                await _semaphore.WaitAsync(timedCancellationToken);

                while (true)
                {
                    var i = 0;
                    for (; i < _maxPoolSize; i++)
                    {
                        timedCancellationToken.ThrowIfCancellationRequested();

                        if (Interlocked.CompareExchange(ref _slots[i], 1, 0) == 0)
                        {
                            try
                            {
                                if (_pooledObjects[i] is null)
                                {
                                    Console.WriteLine("Opening");
                                    var obj = await CreatePooledObject().ConfigureAwait(false);
                                    _pooledObjects[i] = new PooledObject<T>(this, obj) { Slot = i };
                                    Console.WriteLine("Opened");
                                }
                                var pooledObject = _pooledObjects[i];
                                return pooledObject!;
                            }
                            catch (Exception)
                            {
                                Interlocked.Exchange(ref _slots[i], 0);
                            }
                        }
                    }

                    await _settings.Backoff.Delay(timedCancellationToken).ConfigureAwait(false);                    
                }
            }
            catch (OperationCanceledException)
            {
                throw new Exception("Timeout waiting for free slot in connection pool");
            }
            finally
            {
                _semaphore.Release();
                _readerWriterLock.ExitReadLock();
            }
        }

        internal void Release(int slot)
        {
            ThrowIfDisposed();

            Console.WriteLine("Releasing=" + slot);
            Interlocked.Exchange(ref _slots[slot], 0);
            Console.WriteLine("Released=" + slot);
        }

        private void Evict(object? state)
        {
            ThrowIfDisposed();

            try
            {
                _readerWriterLock.EnterReadLock();

                var i = 0;
                for (; i < _maxPoolSize; i++)
                {
                    if (Interlocked.CompareExchange(ref _slots[i], 1, 0) == 0)
                    {
                        var pooledObject = _pooledObjects[i];

                        try
                        {
                            if (pooledObject is not null)
                            {
                                Console.WriteLine("Evicting");
                                ReleasePooledObject(pooledObject.Object);
                                Console.WriteLine("Evicted");
                            }
                        }
                        finally
                        {
                            _slots[i] = 0;
                            _pooledObjects[i] = null;
                        }
                    }
                }
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
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
                _readerWriterLock.EnterWriteLock();

                if (!_disposed)
                {
                    if (disposing)
                    {
                        _semaphore?.Dispose();
                        _evictionTimer?.Dispose();
                        _readerWriterLock?.Dispose();
                    }

                    var done = false;
                    while (!done)
                    {
                        var i = 0;
                        for (; i < _maxPoolSize; i++)
                        {
                            if (Interlocked.CompareExchange(ref _slots[i], 2, 0) == 0)
                            {
                                var pooledObject = _pooledObjects[i];

                                if (pooledObject is not null)
                                {
                                    try
                                    {
                                        ReleasePooledObject(pooledObject.Object);
                                    }
                                    catch
                                    {
                                        Interlocked.Exchange(ref _slots[i], 0);
                                    }
                                }
                            }
                        }

                        done = _slots.All(e => e == 2);
                    }

                    _disposed = true;
                }
            }    
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        ~ObjectPool()
        {
            Dispose(false);
        }
    }
}
