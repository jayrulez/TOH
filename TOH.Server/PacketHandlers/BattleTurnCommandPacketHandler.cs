using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class BattleTurnCommandPacketHandler : PacketHandler<BattleTurnCommandPacket>
    {
        private readonly PVPBattleSystemService _matchService;
        public BattleTurnCommandPacketHandler(PVPBattleSystemService matchService, IPacketConverter packetConverter) : base(packetConverter)
        {
            _matchService = matchService;
        }

        public override async Task HandleImp(IConnection connection, BattleTurnCommandPacket Packet)
        {
            await _matchService.PushTurnCommand(Packet);
        }
    }
}
