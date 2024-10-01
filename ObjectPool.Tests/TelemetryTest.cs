using System.Text;
using ObjectPool.Tests.Utils;

namespace ObjectPool.Tests
{
    public class TelemetryTest
    {
        private readonly FakeTelemetryListener telemetryListener = new();

        [Test]
        public async Task ShouldWriteActivatedEvent()
        {
            DefaultObjectPool<StringBuilder> pool = new(new Settings()
            {
                EvictionInterval = Timeout.Infinite,
                TelemetryListener = telemetryListener
            }, () => new StringBuilder());
            var _ = await pool.Get();
            Assert.IsTrue(telemetryListener.WriteActivatedEventCalled);
        }

        [Test]
        public void ShouldWriteCancellationEvent()
        {
            DefaultObjectPool<StringBuilder> pool = new(new Settings()
            {
                EvictionInterval = Timeout.Infinite,
                TelemetryListener = telemetryListener
            }, () => new StringBuilder());
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();
            Assert.ThrowsAsync<ObjectPoolCancellationException>(async () =>
            {
                await pool.Get(cancellationTokenSource.Token);
            });
            Assert.IsTrue(telemetryListener.WriteCancellationErrorEventCalled);
        }

        [Test]
        public async Task ShouldWriteActivateErrorEvent()
        {
            var tries = 0;
            ErrorObjectPool<StringBuilder> pool
            = new(new Settings()
            {
                EvictionInterval = Timeout.Infinite,
                TelemetryListener = telemetryListener
            }, activator: (_) =>
            {
                if (tries++ == 0)
                {
                    throw new Exception();
                }                    
            });
            await pool.Get();
            Assert.IsTrue(telemetryListener.WriteActivateErrorEventCalled);
        }

        [Test]
        public async Task ShouldWriteEvictEvent()
        {
            DefaultObjectPool<StringBuilder> pool = new(new Settings()
            {
                EvictionInterval = Timeout.Infinite,
                TelemetryListener = telemetryListener,
                EvictionTimeout = 0
            }, () => new StringBuilder());
            var connector = await pool.Get();
            connector.Dispose();
            pool.Evict();
            Assert.IsTrue(telemetryListener.WriteEvictEventCalled);
            Assert.IsTrue(telemetryListener.WriteDeactivatedEventCalled);
        }

        [Test]
        public async Task ShouldWriteDeactivateEvent()
        {
            DefaultObjectPool<StringBuilder> pool = new(new Settings()
            {
                EvictionInterval = Timeout.Infinite,
                TelemetryListener = telemetryListener
            }, () => new StringBuilder());
            var connector = await pool.Get();
            connector.Dispose();
            pool.Dispose();
            Assert.IsTrue(telemetryListener.WriteDeactivatedEventCalled);
        }

        [Test]
        public async Task ShouldWriteDeactivateErrorEvent()
        {
            ErrorObjectPool<StringBuilder> pool
            = new(new Settings()
            {
                EvictionInterval = Timeout.Infinite,
                TelemetryListener = telemetryListener,
                EvictionTimeout = 0
            },
            deactivator: (_) =>
            {
                throw new Exception();
            });
            var connector = await pool.Get();
            connector.Dispose();
            pool.Evict();
            Assert.IsTrue(telemetryListener.WriteEvictEventCalled);
            Assert.IsTrue(telemetryListener.WriteDeactivateErrorEventCalled);
        }

        [Test]
        public async Task ShouldWriteDeactivateErrorEvent_WhenDisposing()
        {
            ErrorObjectPool<StringBuilder> pool
            = new(new Settings()
            {
                EvictionInterval = Timeout.Infinite,
                TelemetryListener = telemetryListener,
                EvictionTimeout = 0
            },
            deactivator: (_) =>
            {
                throw new Exception();
            }); 
            var connector = await pool.Get();
            connector.Dispose();
            pool.Dispose();
            Assert.IsTrue(telemetryListener.WriteDeactivateErrorEventCalled);
        }
    }
}