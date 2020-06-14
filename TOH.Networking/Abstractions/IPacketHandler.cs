using System.Threading.Tasks;

namespace TOH.Network.Abstractions
{
    public interface IPacketHandler
    {
        Task Handle(IConnection connection, Packet packet);
    }

    public interface IPacketHandler<T> : IPacketHandler where T : Packet
    {
    }
}
