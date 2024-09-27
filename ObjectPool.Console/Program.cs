using ClickHouse.Ado;
using ObjectPool;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Diagnostics.Metrics;
using OpenTelemetry.Trace;
using System.Text;
using System.Net;

public class Program
{
    static DbConnectionPool pool = new DbConnectionPool(
        new Settings { MaxPoolSize = 100, WaitingTimeout = 10000, Name = "MyPool11", EvictionInterval = 2000 },
        () =>
        {
            var c = new ClickHouseConnection("SocketTimeout=1000000; ConnectionTimeout=10; Host=192.168.38.109; Port=43477; Database=; User=default; Password=123asdZXC@;");
            return c;
        });     
    static int i = 0;

    public static void Main(string[] args)
    {

        using var provider = Sdk.CreateMeterProviderBuilder()
                                .AddMeter("ObjectPool")
                                //.AddMeter("TestMeter")
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


        //var myMeter = new Meter("TestMeter");
        //var counter = myMeter.CreateCounter<int>("testing1");

        //counter.Add(1);
        //Thread.Sleep(10000);

        //counter.Add(2);
        //Thread.Sleep(10000);

        //counter.Add(1);
        //Thread.Sleep(10000);

        //counter.Add(2);
        //Thread.Sleep(10000);

        //counter.Add(5);
        //Thread.Sleep(10000);

        //counter.Add(4);
      


        //Console.WriteLine("otlp running");
        //// Specify the name of the application of which you want to report the trace data by using OpenTelemetry.
        //var serviceName = "otlp-test";
        //using var tracerProvider = Sdk.CreateTracerProviderBuilder()
        //    .AddSource(serviceName)
        //    .SetResourceBuilder(
        //    ResourceBuilder.CreateDefault().AddService(serviceName))
        //    //.AddConsoleExporter() // Optional. Export data in the console.
        //    .Build();
        //for (int i = 0; i < 10; i++)
        //{
        //    var MyActivitySource = new ActivitySource(serviceName);
        //    using var activity = MyActivitySource.StartActivity("SayHello");
        //    activity?.SetTag("bar", "Hello World");
        //}


        Task t = MainAsync(args);
        t.Wait();
    }

    static async Task<int> Fetch(int e)
    {
        Interlocked.Increment(ref i);

        //Console.WriteLine("Task=" + i);
            
        using var connector = await pool.Get().ConfigureAwait(false);
        //connector.Dispose()
        //pool.Evict(null);
        //using var con1 = await pool.Get();


        //Console.WriteLine("Try execute");

        var command = connector.Object.CreateCommand();

        //Console.WriteLine("EXECCCCCCCCCCCCCCCCCCCCCCCC");

        command.CommandText = "SELECT 1";
        byte result = (byte)command.ExecuteScalar();


        //Console.WriteLine("RESSSSSSSSSSSSSSSSSSS");


        //Console.WriteLine("Processed=" + i);

        return 0;
    }

    static async Task MainAsync(string[] args)
    {
        Stopwatch sw = new Stopwatch();


        //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:9090/api/v1/otlp/v1/metrics");
        //httpWebRequest.ContentType = "application/json";
        //httpWebRequest.Method = "POST";

        //using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        //{
        //    string json = @"{
        //  ""resourceMetrics"": [
        //    {
        //      ""resource"": {
        //        ""attributes"": [
        //          {
        //            ""key"": ""service.name"",
        //            ""value"": {
        //              ""stringValue"": ""my.service""
        //            }
        //          }
        //        ]
        //      },
        //      ""scopeMetrics"": [
        //        {
        //          ""scope"": {
        //            ""name"": ""my.library"",
        //            ""version"": ""1.0.0"",
        //            ""attributes"": [
        //              {
        //                ""key"": ""my.scope.attribute"",
        //                ""value"": {
        //                  ""stringValue"": ""some scope attribute""
        //                }
        //              }
        //            ]
        //          },
        //          ""metrics"": [
        //            {
        //              ""name"": ""counter"",
        //              ""unit"": ""1"",
        //              ""description"": ""I am a Counter"",
        //              ""sum"": {
        //                ""dataPoints"": [
        //                  {
        //                    ""asDouble"": 5,
        //                    ""startTimeUnixNano"": 1544712660300000000,
        //                    ""timeUnixNano"": 1544712660300000000
        //                  }
        //                ]
        //              }
        //            }
        //          ]
        //        }
        //      ]
        //    }
        //  ]
        //}";

        //    streamWriter.Write(json);
        //}

        //try
        //{
        //    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //    {
        //        var result = streamReader.ReadToEnd();
        //    }
        //}
        //catch(Exception e)
        //{

        //}



       

        sw.Start();

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

        //tasks.Add(Task.Run(() =>
        //{
        //    pool?.Dispose();
        //}));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        sw.Stop();

        Console.WriteLine("Done=" + sw.ElapsedMilliseconds);

        Console.ReadLine();

        pool.Dispose();

        Console.ReadLine();
    }
}