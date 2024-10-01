using ClickHouse.Ado;
using Microsoft.Extensions.Configuration;
using ObjectPool;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public class Program
{
    static DbConnectionPool? pool;

    public static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);

        var config = builder.Build();

        pool = new(new Settings { MaxPoolSize = 100, WaitingTimeout = 10000, Name = config["PoolName"]!, EvictionInterval = 2000 },
        () => { return new ClickHouseConnection(config["Clickhouse"]); });

        using var provider  = 
        Sdk.CreateMeterProviderBuilder()
           .AddMeter("ObjectPool")
           .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ObjectPool.Console"))
           .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
           {
               exporterOptions.Endpoint = new Uri(config["Prometheus"]!);
               exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
               exporterOptions.ExportProcessorType = ExportProcessorType.Simple;
               metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
            
           })
           .AddConsoleExporter()
           .Build();

        Task t = MainAsync(args);
        t.Wait();
    }

    static async Task<int> Fetch()
    {            
        using var connector = await pool!.Get().ConfigureAwait(false);
        var command = connector.Object.CreateCommand();
        command.CommandText = "SELECT 1";
        byte? result = (byte?)command.ExecuteScalar();
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
                await Fetch();
            });
            return thread;
        }).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        Console.ReadLine();

        pool.Dispose();

        Console.ReadLine();
    }
}