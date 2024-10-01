using System.Text;

namespace ObjectPool.Tests
{
    public class ObjectPoolTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Get_WhenAlreadyDisposed_ShouldThrowException()
        {
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                DefaultObjectPool<StringBuilder> pool = new(() => new StringBuilder());
                pool.Dispose();
                await pool.Get();
            });
        }

        [Test]
        public async Task Get_ShouldGetObject()
        {
            DefaultObjectPool<StringBuilder> pool = new(() => new StringBuilder());
            var obj = await pool.Get(CancellationToken.None);
            Assert.IsNotNull(obj);
        }

        [Test]
        public void Get_ShouldThrowObjectPoolCancellationException()
        {
            Assert.ThrowsAsync<ObjectPoolCancellationException>(async () =>
            {
                DefaultObjectPool<StringBuilder> pool = new(() => new StringBuilder());
                CancellationTokenSource cancellationTokenSource = new();
                cancellationTokenSource.Cancel();
                var obj = await pool.Get(cancellationTokenSource.Token);
            });
        }

        [Test]
        public async Task Get_WhenSlotIsBusy_ThrowObjectPoolCancellationException()
        {
            DefaultObjectPool<StringBuilder> pool = new(new Settings { MaxPoolSize = 1, WaitingTimeout = 100 }, () => new StringBuilder());
            var obj = await pool.Get(CancellationToken.None);
            obj.Object.Append("Some string");
            Assert.IsNotNull(obj);
            Assert.ThrowsAsync<ObjectPoolCancellationException>(async () =>
            {
                await pool.Get(CancellationToken.None);
            });
        }

        [Test]
        public async Task Get_WhenSlotIsNotBusy_ShouldReturnObject()
        {
            DefaultObjectPool<StringBuilder> pool = new(new Settings { MaxPoolSize = 1, WaitingTimeout = 100 }, () => new StringBuilder());
            var obj1 = await pool.Get(CancellationToken.None);
            obj1.Dispose();
            var obj2 = await pool.Get(CancellationToken.None);
            Assert.That(obj2.Object, Is.SameAs(obj1.Object));
        }

        [Test]
        public async Task Get_Dispose()
        {
            DefaultObjectPool<StringBuilder> pool = new(new Settings
            {
                MaxPoolSize = 1,
                WaitingTimeout = 100,
                EvictionInterval = Timeout.Infinite
            }, () => new StringBuilder());
            var obj = await pool.Get(CancellationToken.None);
            obj.Dispose();
            pool.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await pool.Get(CancellationToken.None);
            });
        }
    }
}