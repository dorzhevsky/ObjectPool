using System.Data.Common;

namespace ObjectPool
{
    public class DbConnectionPool : ObjectPool<DbConnection>
    {
        public DbConnectionPool(Settings settings, Func<DbConnection> connectionFactory)
            : base(settings, connectionFactory)
        {
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