using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class FindBattlePacket : Packet
    {
        public string SessionId { get; set; }
    }
}
