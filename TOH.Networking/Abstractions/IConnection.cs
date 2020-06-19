using System.Collections.Generic;
using System.Threading.Tasks;

namespace TOH.Network.Abstractions
{
    public interface IConnection
    {
        string Id { get; }
        bool IsClosed { get; }
        Task Send<T>(T packet) where T : Packet;
        IAsyncEnumerable<Packet> GetPackets();

        T Unwrap<T>(Packet packet) where T : Packet;
        bool CanUnwrap<T>(Packet packet) where T : Packet;
        bool TryUnwrap<T>(Packet packet, out T unwrappedPacket) where T : Packet;

        void Close();
    }
}