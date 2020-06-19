using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class JoinSessionPacket : Packet
    {
        public string Token { get; set; }
    }
}
