using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class MatchReadyPacket : Packet
    {
        public string MatchId { get; set; }
    }
}
