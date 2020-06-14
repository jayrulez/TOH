using TOH.Network.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TOH.Network.Common
{
    public class TcpConnection : IConnection
    {
        const int BufferSize = 4096;

        public string Id { get; }

        public bool IsClosed => !_socket.Connected;

        private readonly Socket _socket;
        private readonly IPacketConverter _packetConverter;

        public TcpConnection(Socket socket, IPacketConverter packetConverter)
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

        public async Task Send<T>(T packet) where T : Packet
        {
            var packetBuffer = new byte[BufferSize];

            var packetBytes = _packetConverter.ToBytes(packet);

            if (packetBytes.Length > BufferSize)
            {
                throw new Exception($"Serialized packet is larger than buffer size of '{BufferSize}' bytes.");
            }

            packetBytes.CopyTo(packetBuffer, 0);

            //await _socket.SendAsync(buffer: new ArraySegment<byte>(array: packetBuffer, offset: 0, count: packetBuffer.Length), socketFlags: SocketFlags.None);
            await _socket.SendAsync(buffer: new ArraySegment<byte>(array: packetBytes, offset: 0, count: packetBytes.Length), socketFlags: SocketFlags.None);
            //await _socket.SendAsync(buffer: new ArraySegment<byte>(array: serializedPacketBuffer, offset: 0, count: serializedPacketBuffer.Length), socketFlags: SocketFlags.None);
        }

        public async IAsyncEnumerable<Packet> GetPackets()
        {
            var networkStream = new NetworkStream(_socket, false);
            byte[] streamBuffer = new byte[BufferSize];

            var streamSize = await networkStream.ReadAsync(streamBuffer, 0, streamBuffer.Length);

            if (streamSize > 0)
            {
                Array.Resize(ref streamBuffer, streamSize);

                await foreach (var packet in _packetConverter.StreamFromBytes<Packet>(streamBuffer))
                {
                    yield return packet;
                }
            }

            await Task.Yield();
        }
    }
}
