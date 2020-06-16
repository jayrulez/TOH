using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Systems;

namespace TOH.Server.PacketHandlers
{
    public class SetMatchTeamPacketHandler : PacketHandler<SetMatchTeamPacket>
    {
        private readonly MatchService _matchService;

        public SetMatchTeamPacketHandler(MatchService matchService, IPacketConverter packetConverter) : base(packetConverter)
        {
            _matchService = matchService;
        }

        public override async Task HandleImp(IConnection connection, SetMatchTeamPacket Packet)
        {
            await _matchService.SetMatchTeam(Packet.MatchId, connection.Id, Packet.Units);
        }
    }
}