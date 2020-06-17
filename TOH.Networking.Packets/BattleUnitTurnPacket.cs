using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleUnitTurnPacket : Packet
    {
        public int UnitId { get; set; }
    }
}
