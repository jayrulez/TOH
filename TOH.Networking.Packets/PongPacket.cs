using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class PongPacket : Packet
    {
        public string PingId { get; set; }
    }
}
