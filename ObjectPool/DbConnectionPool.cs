using System.Data.Common;

namespace ObjectPool
{
    public class DbConnectionPool : ObjectPool<DbConnection>
    {
        private readonly Func<DbConnection> _connectionFactory;

        public DbConnectionPool(Settings settings, Func<DbConnection> connectionFactory)
            : base(settings)
        {
            _connectionFactory = connectionFactory;
        }

        protected override Task<DbConnection> CreatePooledObject()
        {
            var connection = _connectionFactory();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.OpenAsync();
            }
            return Task.FromResult(connection);
        }

        protected override void ReleasePooledObject(DbConnection connection)
        {
            if (connection?.State != System.Data.ConnectionState.Closed)
            {
                connection?.Close();
            }
        }
    }
}
