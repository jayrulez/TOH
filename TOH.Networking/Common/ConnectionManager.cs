using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TOH.Networking.Abstractions;

namespace TOH.Networking.Common
{
    public class ConnectionManager<TConnection, TPacket> where TConnection : IConnection<TPacket> where TPacket : Packet
    {
        public ConcurrentDictionary<string, TConnection> Connections { get; }

        private readonly ILogger _logger;

        public ConnectionManager(ILogger<ConnectionManager<TConnection, TPacket>> logger)
        {
            _logger = logger;

            Connections = new ConcurrentDictionary<string, TConnection>();
        }

        public void AddConnection(TConnection connection)
        {
            var existingConnection = GetConnection(connection.Id);

            if (existingConnection != null)
            {
                RemoveConnection(connection);
            }

            Connections.TryAdd(connection.Id, connection);
        }

        public TConnection GetConnection(string id)
        {
            return Connections[id];
        }

        public void RemoveConnection(string id)
        {
            Connections.TryRemove(id, out _);
        }

        public void RemoveConnection(TConnection connection)
        {
            RemoveConnection(connection.Id);

            if (!connection.IsClosed)
            {
                connection.Close();
            }
        }
    }

    public class ConnectionManager : ConnectionManager<TcpConnection, Packet>
    {
        public ConnectionManager(ILogger<ConnectionManager<TcpConnection, Packet>> logger) : base(logger)
        {
        }
    }
}
