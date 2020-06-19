using System.Collections.Generic;

namespace TOH.Network.Abstractions
{
    public interface IPacketConverter
    {
        byte[] ToBytes<T>(T packet) where T : Packet;
        T FromBytes<T>(byte[] packetBytes) where T : Packet;
        IAsyncEnumerable<T> StreamFromBytes<T>(byte[] packetBytes) where T : Packet;
        T Unwrap<T>(Packet packet) where T : Packet;
        bool CanUnwrap<T>(Packet packet) where T : Packet;

        bool TryUnwrap<T>(Packet packet, out T unwrappedPacket) where T : Packet;
    }
}