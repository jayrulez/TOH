using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleInfoPacket : Packet
    {
        public string BattleId { get; set; }

    }
}