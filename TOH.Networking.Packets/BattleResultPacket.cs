using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public enum BattleResultStatus
    {
        Win,
        Lose
    }

    public class BattleResultPacket : Packet
    {
        public string BattleId { get; set; }
        public BattleResultStatus Status { get; set; }
    }
}
