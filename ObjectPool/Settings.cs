using ObjectPool;

namespace ConsoleApp1
{
    public class Settings
    {
        private int _maximumPoolSize;
        public int MaxPoolSize
        {
            get
            {
                return _maximumPoolSize;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), ErrorMessages.NegativeOrZeroMaximumPoolSize);
                }
            }
        }
        public int WaitTimeout { get; set; } = 3;
        public BackoffStrategy Backoff { get; set;  } = new (2, 50);
    }
}
