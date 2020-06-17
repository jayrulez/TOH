using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class BattleTurnCommandPacketHandler : PacketHandler<BattleTurnCommandPacket>
    {
        private readonly BattleSystem _matchService;
        public BattleTurnCommandPacketHandler(BattleSystem matchService, IPacketConverter packetConverter) : base(packetConverter)
        {
            _matchService = matchService;
        }

        public override async Task HandleImp(IConnection connection, BattleTurnCommandPacket Packet)
        {
            await _matchService.PushTurnCommand(Packet);
        }
    }
}
