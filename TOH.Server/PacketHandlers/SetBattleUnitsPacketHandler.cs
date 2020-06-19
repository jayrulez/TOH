using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Services;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class SetBattleUnitsPacketHandler : PacketHandler<SetBattleUnitsPacket>
    {
        private readonly PVPBattleSystemService _battleService;
        private readonly SessionService _sessionService;

        public SetBattleUnitsPacketHandler(SessionService sessionService, PVPBattleSystemService battleService, IPacketConverter packetConverter) : base(packetConverter)
        {
            _battleService = battleService;
            _sessionService = sessionService;
        }

        public override async Task HandleImp(IConnection connection, SetBattleUnitsPacket Packet)
        {
            var session = await _sessionService.GetActiveSession(connection);

            if (session != null)
            {
                await _battleService.SetUnits(Packet.BattleId, session.SessionId, Packet.Units);
            }
            else
            {
                // connection has no session, Kick them?
            }
        }
    }
}