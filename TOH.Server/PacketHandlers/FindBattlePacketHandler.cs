using TOH.Network.Abstractions;
using TOH.Network.Server;
using TOH.Network.Packets;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TOH.Server.Systems;
using TOH.Server.Services;

namespace TOH.Server.PacketHandlers
{
    public class FindBattlePacketHandler : PacketHandler<FindBattlePacket>
    {
        private readonly ILogger _logger;
        private readonly PVPBattleLobbyService _battleLobbyService;
        private readonly SessionService _sessionService;

        public FindBattlePacketHandler(PVPBattleLobbyService battleLobbyService, SessionService sessionService, IPacketConverter packetConverter, ILogger<PingPacketHandler> logger) : base(packetConverter)
        {
            _battleLobbyService = battleLobbyService;
            _sessionService = sessionService;
            _logger = logger;
        }

        public override async Task HandleImp(IConnection connection, FindBattlePacket packet)
        {
            var session = await _sessionService.GetActiveSession(connection);

            if (session != null)
            {
                await _battleLobbyService.JoinQueue(session);
            }
            else
            {
                // connection has no session, Kick them?
            }
        }
    }
}