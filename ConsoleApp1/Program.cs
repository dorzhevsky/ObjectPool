using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Diagnostics.Metrics;

using var provider = Sdk.CreateMeterProviderBuilder()
                        //.AddMeter("ObjectPool")
                        .AddMeter("TestMeter")
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ObjectPool.Console"))
                        .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                        {
                            exporterOptions.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
                            exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                            exporterOptions.ExportProcessorType = ExportProcessorType.Simple;
                            metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;                            
                        })
                        .Build();


var myMeter = new Meter("TestMeter");
var counter = myMeter.CreateUpDownCounter<int>("testing23");

counter.Add(1);
Thread.Sleep(10000);

counter.Add(2);
Thread.Sleep(10000);

counter.Add(1);
Thread.Sleep(10000);

counter.Add(-2);
Thread.Sleep(10000);

counter.Add(5);
Thread.Sleep(10000);

counter.Add(-4);
Thread.Sleep(10000);

Console.WriteLine("Done");

Console.ReadLine();