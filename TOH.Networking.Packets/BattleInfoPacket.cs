using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleInfoPacket : Packet
    {
        public string MatchId { get; set; }
    }
}