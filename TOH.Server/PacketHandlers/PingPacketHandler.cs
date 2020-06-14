using TOH.Network.Abstractions;
using TOH.Network.Server;
using TOH.Network.Packets;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TOH.Server.PacketHandlers
{
    public class PingPacketHandler : PacketHandler<PingPacket>
    {
        private readonly ILogger _logger;

        public PingPacketHandler(ILogger<PingPacketHandler> logger, IPacketConverter packetConverter) : base(packetConverter)
        {
            _logger = logger;
        }

        public override async Task HandleImp(IConnection connection, PingPacket packet)
        {
            _logger.LogInformation($"{packet} from {connection.Id}");

            await connection.Send(new PongPacket { });
        }
    }
}