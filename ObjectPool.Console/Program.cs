using ClickHouse.Ado;
using ObjectPool;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public class Program
{
    static readonly DbConnectionPool pool = new(
        new Settings
        {
            MaxPoolSize = 100,
            WaitingTimeout = 10000,
            Name = "MyPool16",
            EvictionInterval = 2000
        },
        () =>
        {
            var c = new ClickHouseConnection("SocketTimeout=1000000; ConnectionTimeout=10; Host=192.168.38.109; Port=43477; Database=; User=default; Password=123asdZXC@;");
            return c;
        });     

    public static void Main(string[] args)
    {

        using var provider = Sdk.CreateMeterProviderBuilder()
                                .AddMeter("ObjectPool")
                                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ObjectPool.Console"))
                                .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                                {
                                    exporterOptions.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
                                    exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                                    exporterOptions.ExportProcessorType = ExportProcessorType.Simple;
                                    metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;

                                })
                                .AddConsoleExporter()
                                .Build();

        Task t = MainAsync(args);
        t.Wait();
    }

    static async Task<int> Fetch(int e)
    {            
        using var connector = await pool.Get().ConfigureAwait(false);
        var command = connector.Object.CreateCommand();
        command.CommandText = "SELECT 1";
        byte result = (byte)command.ExecuteScalar();
        return 0;
    }

    static async Task MainAsync(string[] args)
    {
        var r = new Random();

        var tasks = Enumerable.Range(0, 500).Select(ser =>
        {
            Task thread = Task.Run(async () =>
            {
                var d = r.Next(10000);
                await Task.Delay(d);
                await Fetch(0);
            });
            return thread;
        }).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        Console.ReadLine();

        pool.Dispose();

        Console.ReadLine();
    }
}