using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class MatchTurnCommandPacketHandler : PacketHandler<MatchTurnCommandPacket>
    {
        private readonly MatchService _matchService;
        public MatchTurnCommandPacketHandler(MatchService matchService, IPacketConverter packetConverter) : base(packetConverter)
        {
            _matchService = matchService;
        }

        public override async Task HandleImp(IConnection connection, MatchTurnCommandPacket Packet)
        {
            await _matchService.PushTurnCommand(Packet);
        }
    }
}
