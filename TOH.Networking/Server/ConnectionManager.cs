using TOH.Network.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace TOH.Network.Server
{
    public class ConnectionManager
    {
        public List<IConnection> Connections { get; }

        private readonly ILogger _logger;

        public ConnectionManager(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConnectionManager>();

            Connections = new List<IConnection>();
        }

        public IConnection GetConnection(string id)
        {
            return Connections.FirstOrDefault(c => c.Id.Equals(id));
        }

        public void AddConnection(IConnection connection)
        {
            var existingConnection = GetConnection(connection.Id);

            if (existingConnection != null)
            {
                RemoveConnection(connection);
            }

            Connections.Add(connection);
        }

        public void RemoveConnection(string id)
        {
            var existing = GetConnection(id);

            if (existing != null)
            {
                Connections.Remove(existing);
            }
        }

        public void RemoveConnection(IConnection connection)
        {
            RemoveConnection(connection.Id);

            if (!connection.IsClosed)
            {
                connection.Close();
            }
        }
    }
}
