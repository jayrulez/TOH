using System.Net.Sockets;
using TOH.Network.Abstractions;
using TOH.Network.Common;

namespace TOH.Network.Client
{
    public enum TcpClientState
    {
        None,
        Connecting,
        Connected
    }

    public abstract class AbstractTcpClient
    {
        private readonly TcpClientOptions _clientOptions;

        public TcpClientState State;

        public AbstractTcpClient(TcpClientOptions clientOptions)
        {
            _clientOptions = clientOptions;
            State = TcpClientState.None;
        }

        public IConnection Connection { get; private set; }

        public IConnection Connect()
        {
            State = TcpClientState.Connecting;

            if (Connection == null || Connection.IsClosed)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(_clientOptions.Host, _clientOptions.Port);

                Connection = new TcpConnection(socket, new JsonPacketConverter());
            }

            State = TcpClientState.Connected;

            return Connection;
        }
    }
}
