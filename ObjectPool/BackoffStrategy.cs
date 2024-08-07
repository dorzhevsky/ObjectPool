using System.Threading;

namespace ConsoleApp1
{
    public class BackoffStrategy
    {
        private readonly int _delayMilliseconds;
        private readonly int _maxDelayMilliseconds;
        private int _retries;
        private int _pow;

        public BackoffStrategy(int delayMilliseconds, int maxDelayMilliseconds)
        {
            _delayMilliseconds = delayMilliseconds;
            _maxDelayMilliseconds = maxDelayMilliseconds;
            _retries = 0;
            _pow = 1;
        }

        public Task Delay(CancellationToken cancellationToken)
        {
            if (_retries < 31) 
            {
                _pow = 1 << _retries;
            }
            int resultDelay = _delayMilliseconds * _pow;
            var delay = Math.Min(resultDelay < 0 ? _maxDelayMilliseconds : resultDelay, _maxDelayMilliseconds);
            _retries++;
            return Task.Delay(delay, cancellationToken);
        }
    }
}
