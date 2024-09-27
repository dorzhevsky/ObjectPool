using Nito.AsyncEx;

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
        private readonly AsyncReaderWriterLock _lock = new();

        protected readonly TelemetryListener _telemetry;
        public ObjectPool() : this(Settings.Default) {}

        public ObjectPool(Settings settings)
        {
            _settings = settings;

            _telemetry = new TelemetryListener(_settings.Name);
            _slots = Enumerable.Repeat(0, _settings.MaxPoolSize).ToArray();
            _pooledObjects = Enumerable.Range(0, _settings.MaxPoolSize).Select(e => new PooledObject<T>(this, default!)).ToArray();
            _semaphore = new SemaphoreSlim(_settings.ConcurrencyFactor);
            _evictionTimer = new Timer(Evict, this, 0, _settings.EvictionInterval);
        }

        public Task<PooledObject<T>> Get() => Get(CancellationToken.None);

        public async Task<PooledObject<T>> Get(CancellationToken cancellationToken)
        {
            var timedCancellationToken = cancellationToken.CancelAfter(_settings.WaitingTimeout);

            using var _ = _lock.ReaderLock(cancellationToken);

            ThrowIfDisposed();

            try
            {
                await _semaphore.WaitAsync(timedCancellationToken).ConfigureAwait(false);

                var backoff = new BackoffStrategy(_settings.BackoffDelayMilliseconds, _settings.BackoffMaxDelayMilliseconds);

                while (true)
                {
                    ThrowIfDisposed();

                    for (var i = 0; i < _settings.MaxPoolSize; i++)
                    {
                        timedCancellationToken.ThrowIfCancellationRequested();

                        if (Interlocked.CompareExchange(ref _slots[i], 1, 0) == 0)
                        {
                            try
                            {
                                var pooledObject = _pooledObjects[i]!;

                                pooledObject.Object ??= Create();
                                await Activate(pooledObject.Object).ConfigureAwait(false);

                                pooledObject.LastUsedTimestamp = DateTime.UtcNow;
                                pooledObject.Slot = i;
                                
                                return pooledObject!;
                            }
                            catch (Exception)
                            {
                                Interlocked.Exchange(ref _slots[i], 0);
                                _telemetry.WriteActivateErrorEvent();
                                break;
                            }
                        }
                    }

                    await backoff.Delay(timedCancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                _telemetry.WriteCancellationErrorEvent();
                throw new ObjectPoolCancellationException(ErrorMessages.ObjectPoolCancellationExceptionMessage);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected virtual void WriteCancellationError() { }

        protected virtual void WriteGetError() { }

        internal void Release(int slot)
        {
            ThrowIfDisposed();
            Interlocked.CompareExchange(ref _slots[slot], 0, 1);
        }

        public void Evict(object? state)
        {
            ThrowIfDisposed();

            _telemetry.WriteEvictEvent();

            var i = 0;
            for (; i < _settings.MaxPoolSize; i++)
            {
                if (Interlocked.CompareExchange(ref _slots[i], 1, 0) == 0)
                {
                    var pooledObject = _pooledObjects[i];

                    try
                    {
                        if (pooledObject!.Object is not null)
                        {
                            if (pooledObject.LastUsedTimestamp.HasValue
                                && (DateTime.UtcNow - pooledObject.LastUsedTimestamp.Value).TotalMilliseconds > _settings.EvictionTimeout)
                            {
                                Deactivate(pooledObject.Object);
                            }
                        }
                    }
                    catch
                    {
                        _telemetry.WriteDeactivateErrorEvent();
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _slots[i], 0);
                    }
                }
            }            
        }

        protected abstract T Create();
        protected abstract Task Activate(T @object);
        protected abstract void Deactivate(T @object);
        protected void OnActivated() => _telemetry.WriteActivatedEvent();
        protected void OnDeactivated() => _telemetry.WriteDeactivatedEvent();

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
            using var _ = _lock.WriterLock();

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

        private void CleanUp()
        {
            var processed = 0;
            while (processed != _settings.MaxPoolSize)
            {
                var i = 0;
                for (; i < _settings.MaxPoolSize; i++)
                {
                    if (Interlocked.CompareExchange(ref _slots[i], 1, 0) == 0)
                    {
                        var pooledObject = _pooledObjects[i];
                        Interlocked.Increment(ref processed);
                        if (pooledObject!.Object is not null)
                        {
                            try
                            {
                                Deactivate(pooledObject.Object);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }
    }
}