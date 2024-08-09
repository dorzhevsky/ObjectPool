namespace ObjectPool
{
    public class Settings
    {
        private int _maximumPoolSize;
        private int _concurrencyFactor;

        public Settings()
        {
            MaxPoolSize = 100;
            WaitingTimeout = 3000;
            EvictionInterval = 3000;
            BackoffDelayMilliseconds = 2;
            BackoffMaxDelayMilliseconds = 50;
            ConcurrencyFactor = MaxPoolSize * Environment.ProcessorCount;
        }
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
                _maximumPoolSize = value;
            }
        }
        public int WaitingTimeout { get; set; }
        public int EvictionInterval { get; set; }
        public int ConcurrencyFactor
        {
            get
            {
                return _concurrencyFactor;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), ErrorMessages.NegativeOrZeroConcurrencyFactor);
                }
                _concurrencyFactor = value;
            }
        }

        public int BackoffDelayMilliseconds { get; set; }
        public int BackoffMaxDelayMilliseconds { get; set; }

        public static Settings Default = new();
    }
}
