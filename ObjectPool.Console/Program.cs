using ClickHouse.Ado;
using ObjectPool;
using System.Diagnostics;


public class Program
{
    static DbConnectionPool pool = new DbConnectionPool(new Settings { MaxPoolSize = 1, WaitingTimeout = 5000 },
        () => new ClickHouseConnection("SocketTimeout=1000000; ConnectionTimeout=10; Host=192.168.38.109; Port=44071; Database=; User=default; Password=123asdZXC@;"));
    static int i = 0;

    public static void Main(string[] args)
    {
        Task t = MainAsync(args);
        t.Wait();
    }

    static async Task<int> Fetch(int e)
    {
        Interlocked.Increment(ref i);

        Console.WriteLine("Task=" + i);

        using var connector = await pool.Get();

        var command = connector.Object.CreateCommand();
        command.CommandText = "SELECT 1";

        Stopwatch sw = new Stopwatch();
        sw.Start();

        byte result = (byte)command.ExecuteScalar();

        sw.Stop();

        Console.WriteLine("Processed=" + sw.ElapsedMilliseconds);

        return 0;
    }

    static async Task MainAsync(string[] args)
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();

        var tasks = Enumerable.Range(0, 50).Select(ser =>
        {
            Task thread = Task.Run(async () =>
            {
                await Fetch(0);
            });
            return thread;
        }).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        sw.Stop();

        Console.WriteLine("Done=" + sw.ElapsedMilliseconds);
    }
}