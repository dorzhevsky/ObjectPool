using System.Diagnostics.Metrics;

namespace ObjectPool
{
    public class TelemetryListener: ITelemetryListener
    {
        private static readonly Meter meter = new("ObjectPool");
        private readonly KeyValuePair<string, object?> _tags;
        private readonly UpDownCounter<int> _activeItems;
        private readonly Counter<int> _evictins;
        private readonly Counter<int> _activateErrors;
        private readonly Counter<int> _connectionCloseErrors;
        private readonly Counter<int> _cancellations;

        public TelemetryListener(string name)
        {
            _tags = new KeyValuePair<string, object?>("object_pool", name);
            _activeItems = meter.CreateUpDownCounter<int>("pool_activeitems");
            _evictins = meter.CreateCounter<int>("pool_evictions");
            _activateErrors = meter.CreateCounter<int>("pool_activate_errors");
            _connectionCloseErrors = meter.CreateCounter<int>("pool_deactivate_errors");
            _cancellations = meter.CreateCounter<int>("pool_cancellations");
        }

        public void WriteActivatedEvent() => _activeItems.Add(1, _tags);
        public void WriteDeactivatedEvent() => _activeItems.Add(-1, _tags);
        public void WriteEvictEvent() => _evictins.Add(1, _tags);
        public void WriteActivateErrorEvent() => _activateErrors.Add(1, _tags);
        public void WriteDeactivateErrorEvent() => _connectionCloseErrors.Add(1, _tags);
        public void WriteCancellationErrorEvent() => _cancellations.Add(1, _tags);
    }
}
