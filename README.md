Simple object pool with OpenTelemetry support 

## NuGet Packages

| **Package** | **Latest Version** | **About** |
|:--|:--|:--|
| `ODA.ObjectPool` | [![NuGet](https://img.shields.io/nuget/v/ODA.ObjectPool?logo=nuget&label=NuGet&color=blue)](https://www.nuget.org/packages/ODA.ObjectPool/ "Download ODA.ObjectPool from NuGet.org") | Object pool implementation |

## Documentation

### Simple object pooling

You can use **DefaultObjectPool** class to create a pool of objects.
**DefaultObjectPool** takes a bunch of settings and a factory

```cs
DefaultObjectPool<StringBuilder> pool = new(new Settings 
{ 
    MaxPoolSize = 100, 
    WaitingTimeout = 10000, 
    Name = config["PoolName"]!, 
    EvictionInterval = 2000 
},
() => new StringBuilder());
```

| **Setting** | **About** |
|:--|:--|
| `Name` | Pool name. It is used for tagging telemetry metrics |
| `MaxPoolSize` | Pool size |
| `WaitingTimeout` | Number of milliseconds to wait for object renting from pool |
| `EvictionInterval` | Object pool periodically (once per EvictionInterval milliseconds) evicts items from pool.  |
| `EvictionTimeout` | If object from pool is not used for at least EvictionTimeout milliseconds it is considered as unusable and will be evicted|
| `ConcurrencyFactor` | Setting for internal semaphore to control the number of concurrent rent tries|
| `BackoffDelayMilliseconds` | |
| `BackoffMaxDelayMilliseconds` | |

### Database connection pooling

To create database connection pool, you must create an instance of **DbConnectionPool**. It is responsible for opening and closing database conections


```cs
DbConnectionPool pool = new(
  new Settings { 
    MaxPoolSize = 100, 
    WaitingTimeout = 10000, 
    Name = config["PoolName"]!, 
    EvictionInterval = 2000 
  },
  () => { return new ClickHouseConnection(config["Clickhouse"]); });
```

To rent a connection from pool use **Get** method as follows:

```cs
using var connector = await pool.Get().ConfigureAwait(false);
```

**Get** method returns a so called connector object. When done, you must dispose connector object to return rented connection to connection pool. You can use its **Object** property to gain acccess to underlying connection and execute database queries:

```cs
var command = connector.Object.CreateCommand();
command.CommandText = "SELECT 1";
byte? result = (byte?)command.ExecuteScalar();
```

When connection pool is no longer needed, please destroy it with **Dispose** method to clean up resources

### OpenTelemetry metrics

Object pool uses metrics from **System.Diagnostics** which are usable for monitoring 

| **Metric** | **About** |
|:--|:--|
| `pool_activeitems` | Number of allocated items in pool |
| `pool_evictions` | Object pool periodically evicts not usable items from pool. This metric is in essence the number of times eviction was invoked|
| `pool_activate_errors` | Number of activation errors |
| `pool_deactivate_errors` | Number of deactivation errors |
| `pool_cancellations` | Number of cancellations |

All metrics are tagged with the name of the pool



