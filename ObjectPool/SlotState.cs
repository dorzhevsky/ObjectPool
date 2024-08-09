namespace ObjectPool
{
    internal static class SlotState
    {
        public const int Free = 0;
        public const int Busy = 1;
        public const int Disposed = 2;
    }
}
