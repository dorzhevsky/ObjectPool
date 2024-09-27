namespace ObjectPool
{
    public interface ITelemetryListener
    {
        void WriteActivatedEvent();
        void WriteActivateErrorEvent();
        void WriteCancellationErrorEvent();
        void WriteDeactivatedEvent();
        void WriteDeactivateErrorEvent();
        void WriteEvictEvent();
    }
}
