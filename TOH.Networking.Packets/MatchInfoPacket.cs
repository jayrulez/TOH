using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class MatchInfoPacket : Packet
    {
        public string MatchId { get; set; }
    }
}