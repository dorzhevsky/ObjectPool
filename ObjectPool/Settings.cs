namespace ConsoleApp1
{
    public class Settings
    {
        public int MaxPoolSize { get; set; } = 100;
        public int WaitTimeout { get; set; } = 3;
        public BackoffStrategy Backoff { get; set;  } = new (2, 50);
    }
}
