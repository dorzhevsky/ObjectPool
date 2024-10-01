namespace ObjectPool.Tests.Utils
{
    internal class FakeTelemetryListener : ITelemetryListener
    {
        public void WriteActivatedEvent()
        {
            WriteActivatedEventCalled = true;
        }

        public void WriteActivateErrorEvent()
        {
            WriteActivateErrorEventCalled = true;
        }

        public void WriteCancellationErrorEvent()
        {
            WriteCancellationErrorEventCalled = true;
        }

        public void WriteDeactivatedEvent()
        {
            WriteDeactivatedEventCalled = true;
        }

        public void WriteDeactivateErrorEvent()
        {
            WriteDeactivateErrorEventCalled = true;
        }

        public void WriteEvictEvent()
        {
            WriteEvictEventCalled = true;
        }

        public bool WriteActivatedEventCalled { get; private set; }
        public bool WriteActivateErrorEventCalled { get; private set; }
        public bool WriteCancellationErrorEventCalled { get; private set; }
        public bool WriteDeactivatedEventCalled { get; private set; }
        public bool WriteDeactivateErrorEventCalled { get; private set; }
        public bool WriteEvictEventCalled { get; private set; }
    }
}
