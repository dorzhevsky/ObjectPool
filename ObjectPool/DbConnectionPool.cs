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

        protected override DbConnection Create()
        {
            var connection = _connectionFactory();
            return connection;
        }

        protected override Task Activate(DbConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                OnActivated();
            }
            return Task.FromResult(connection);
        }

        protected override void Deactivate(DbConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
                OnDeactivated();
            }
        }
    }
}