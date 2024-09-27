namespace ObjectPool
{
    internal interface ITelemetryListener
    {
        void WriteActivatedEvent();
        void WriteActivateErrorEvent();
        void WriteCancellationErrorEvent();
        void WriteDeactivatedEvent();
        void WriteDeactivateErrorEvent();
        void WriteEvictEvent();
    }
}
