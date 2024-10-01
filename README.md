Simple object pool with OpenTelemetry support 

## NuGet Packages

| **Package** | **Latest Version** | **About** |
|:--|:--|:--|
| `ODA.ObjectPool` | [![NuGet](https://img.shields.io/nuget/v/ODA.ObjectPool?logo=nuget&label=NuGet&color=blue)](https://www.nuget.org/packages/ODA.ObjectPool/ "Download ODA.ObjectPool from NuGet.org") | Object pool implementation |

## Documentation

### Simple object pooling

### Database connection pooling

To create database connection pool, you must create an instance of **DbConnectionPool** class passing a bunch on configuration settings and a factory which returns **System.Data.Common.DbConnection**


<!-- snippet: quick-start -->
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
<!-- endSnippet -->

To rent a connection from pool use **Get** method as follows:

<!-- snippet: quick-start -->
```cs
using var connector = await pool.Get().ConfigureAwait(false);
```
<!-- endSnippet -->

**Get** method returns a so called connector object. When done, you must dispose connector object to return rented connection to connection pool. You can use its **Object** property to gain acccess to underlying connection and execute database queries:

<!-- snippet: quick-start -->
```cs
var command = connector.Object.CreateCommand();
command.CommandText = "SELECT 1";
byte? result = (byte?)command.ExecuteScalar();
```
<!-- endSnippet -->

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



