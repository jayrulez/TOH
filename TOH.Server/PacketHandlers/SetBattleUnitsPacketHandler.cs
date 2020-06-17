using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class SetBattleUnitsPacketHandler : PacketHandler<SetBattleUnitsPacket>
    {
        private readonly BattleSystem _matchService;

        public SetBattleUnitsPacketHandler(BattleSystem matchService, IPacketConverter packetConverter) : base(packetConverter)
        {
            _matchService = matchService;
        }

        public override async Task HandleImp(IConnection connection, SetBattleUnitsPacket Packet)
        {
            await _matchService.SetMatchTeam(Packet.MatchId, connection.Id, Packet.Units);
        }
    }
}