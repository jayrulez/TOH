using TOH.Network.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TOH.Network.Common
{
    public class TcpConnection : IConnection
    {
        const int BufferSize = 4096 * 2;

        //private byte[] ReceiveBuffer;

        public string Id { get; }

        public bool IsClosed => !_socket.Connected;

        private readonly Socket _socket;
        private NetworkStream _stream;
        private readonly IPacketConverter _packetConverter;

        public TcpConnection(Socket socket, IPacketConverter packetConverter)
        {
            Id = Guid.NewGuid().ToString();

            _socket = socket;
            _stream = new NetworkStream(_socket, false);
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
            var packetBytes = _packetConverter.ToBytes(packet);

            var packetLengthSize = sizeof(int);

            var packetSize = packetBytes.Length;

            var packetSizeBytes = BitConverter.GetBytes(packetSize);

            //if (packetBytes.Length > BufferSize)
            //{
            //    throw new Exception($"Serialized packet is larger than buffer size of '{BufferSize}' bytes.");
            //}

            var packetBuffer = new byte[packetSize + packetLengthSize];

            packetSizeBytes.CopyTo(packetBuffer, 0);
            packetBytes.CopyTo(packetBuffer, packetLengthSize);



            //await _socket.SendAsync(buffer: new ArraySegment<byte>(array: packetBuffer, offset: 0, count: packetBuffer.Length), socketFlags: SocketFlags.None);
            await _socket.SendAsync(buffer: new ArraySegment<byte>(array: packetBuffer, offset: 0, count: packetBuffer.Length), socketFlags: SocketFlags.None);
            //await _socket.SendAsync(buffer: new ArraySegment<byte>(array: serializedPacketBuffer, offset: 0, count: serializedPacketBuffer.Length), socketFlags: SocketFlags.None);
        }

        public async IAsyncEnumerable<Packet> GetPackets()
        {
            //while (_stream.DataAvailable)
            //{

            var packetSizeBytes = new byte[sizeof(int)];

            var sizeBytesRead = await _stream.ReadAsync(packetSizeBytes, 0, sizeof(int));

            if (sizeBytesRead == sizeof(int))
            {
                var packetSize = BitConverter.ToInt32(packetSizeBytes, 0);

                //if (packetSize > BufferSize)
                //{
                //    throw new Exception($"Packet size is larger than buffer size.");
                //}

                //byte[] streamBuffer = new byte[BufferSize];
                byte[] streamBuffer = new byte[packetSize];

                var streamSize = await _stream.ReadAsync(streamBuffer, 0, packetSize);

                if (streamSize > 0)
                {
                    //Array.Resize(ref streamBuffer, streamSize);

                    await foreach (var packet in _packetConverter.StreamFromBytes<Packet>(streamBuffer))
                    {
                        yield return packet;
                    }
                }
            }
            else
            {

            }

            if (sizeBytesRead == sizeof(int))
            {
            }
            //}

            //await Task.Yield();
        }

        public T Unwrap<T>(Packet packet) where T : Packet
        {
            return _packetConverter.Unwrap<T>(packet);
        }

        public bool CanUnwrap<T>(Packet packet) where T : Packet
        {
            return _packetConverter.CanUnwrap<T>(packet);
        }

        public bool TryUnwrap<T>(Packet packet, out T unwrappedPacket) where T : Packet
        {
            return _packetConverter.TryUnwrap(packet, out unwrappedPacket);
        }
    }
}
