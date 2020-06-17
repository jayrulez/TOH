using TOH.Network.Abstractions;
using TOH.Network.Server;
using TOH.Network.Packets;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class FindBattlePacketHandler : PacketHandler<FindBattlePacket>
    {
        private readonly ILogger _logger;
        private readonly PVPBattleLobbyService _matchLobbyService;

        public FindBattlePacketHandler(PVPBattleLobbyService matchLobbyService, IPacketConverter packetConverter, ILogger<PingPacketHandler> logger) : base(packetConverter)
        {
            _matchLobbyService = matchLobbyService;
            _logger = logger;
        }

        public override async Task HandleImp(IConnection connection, FindBattlePacket packet)
        {
            await _matchLobbyService.JoinLobbyQueue(connection);
        }
    }
}