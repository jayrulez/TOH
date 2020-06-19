using System.Collections.Generic;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleTurnCommandPacket : Packet
    {
        public string BattleId { get; set; }
        public int UnitId { get; set; }
        public int SkillId { get; set; }
        public List<int> TargetUnitId { get; set; }
    }
}
