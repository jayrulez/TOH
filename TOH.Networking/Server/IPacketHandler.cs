using System.Threading.Tasks;
using TOH.Networking.Abstractions;
using TOH.Networking.Common;

namespace TOH.Networking.Server
{
    public interface IPacketHandler<TPacket> where TPacket : Packet
    {
        Task Handle(TcpConnection connection, TPacket packet);
    }
}
