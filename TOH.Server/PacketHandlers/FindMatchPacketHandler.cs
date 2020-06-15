using TOH.Network.Abstractions;
using TOH.Network.Server;
using TOH.Network.Packets;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class FindMatchPacketHandler : PacketHandler<FindMatchPacket>
    {
        private readonly ILogger _logger;
        private readonly MatchLobbyService _matchLobbyService;

        public FindMatchPacketHandler(MatchLobbyService matchLobbyService, IPacketConverter packetConverter, ILogger<PingPacketHandler> logger) : base(packetConverter)
        {
            _matchLobbyService = matchLobbyService;
            _logger = logger;
        }

        public override async Task HandleImp(IConnection connection, FindMatchPacket packet)
        {
            await _matchLobbyService.FindMatch(connection);

            //_logger.LogInformation($"{packet} {packet.PacketId}");

            //await connection.Send(new PongPacket { PingId = packet.PacketId });
        }
    }
}