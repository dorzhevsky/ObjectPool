namespace ObjectPool
{
    public class Settings
    {
        private int _maximumPoolSize;
        private int _concurrencyFactor;
        private string _name;

        public Settings()
        {
            Name = string.Format("{0}_{1}", Constants.ObjectPool, Guid.NewGuid().ToString());
            MaxPoolSize = 100;
            WaitingTimeout = 3000;
            EvictionInterval = 1000;
            EvictionTimeout = 1000;
            BackoffDelayMilliseconds = 2;
            BackoffMaxDelayMilliseconds = 50;
            ConcurrencyFactor =  MaxPoolSize * Environment.ProcessorCount;
        }

        public string Name 
        { 
            get
            {
                return _name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(nameof(value), ErrorMessages.EmptyPoolName);
                }
                _name = value;
            }
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
        public int EvictionTimeout { get; set; }

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
