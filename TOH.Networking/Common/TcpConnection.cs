using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using TOH.Networking.Abstractions;

namespace TOH.Networking.Common
{
    public class TcpConnection<TPacket> : IConnection<TPacket> where TPacket : Packet
    {
        const int BufferSize = 4096;

        public string Id { get; }

        public bool IsClosed => !_socket.Connected;

        private readonly Socket _socket;
        private readonly IPacketConverter<TPacket> _packetConverter;

        public TcpConnection(Socket socket, IPacketConverter<TPacket> packetConverter)
        {
            Id = Guid.NewGuid().ToString();

            _socket = socket;
            _packetConverter = packetConverter;
        }

        public void Close()
        {
            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
        }

        public async Task<Packet> GetPacket()
        {
            var networkStream = new NetworkStream(_socket, false);

            byte[] buffer = new byte[BufferSize];

            await networkStream.ReadAsync(buffer, 0, buffer.Length);

            var packet = _packetConverter.FromBytes(buffer);

            return packet;
        }

        public async Task Send(TPacket packet)
        {
            var packetBuffer = new byte[BufferSize];
            var packetBytes = _packetConverter.ToBytes(packet);
            if (packetBytes.Length > BufferSize)
            {
                throw new Exception($"Serialized packet is larger than buffer size of '{BufferSize}' bytes.");
            }

            packetBytes.CopyTo(packetBuffer, 0);

            await _socket.SendAsync(buffer: new ArraySegment<byte>(array: packetBuffer, offset: 0, count: packetBuffer.Length), socketFlags: SocketFlags.None);
        }
    }

    public class TcpConnection : TcpConnection<Packet>
    {
        public TcpConnection(Socket socket, IPacketConverter<Packet> packetConverter) : base(socket, packetConverter)
        {
        }
    }
}
