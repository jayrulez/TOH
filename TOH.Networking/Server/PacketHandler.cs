using TOH.Network.Abstractions;
using System.Threading.Tasks;

namespace TOH.Network.Server
{
    public abstract class PacketHandler<T> : IPacketHandler<T> where T : Packet
    {
        private readonly IPacketConverter _packetConverter;

        public PacketHandler(IPacketConverter packetConverter)
        {
            _packetConverter = packetConverter;
        }

        public async Task Handle(IConnection connection, Packet packet)
        {
            var unwrapped = _packetConverter.Unwrap<T>(packet);

            await HandleImp(connection, unwrapped);
        }

        public abstract Task HandleImp(IConnection connection, T Packet);
    }
}
