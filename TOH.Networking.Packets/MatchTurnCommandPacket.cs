using System.Collections.Generic;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class MatchTurnCommandPacket : Packet
    {
        public string MatchId { get; set; }
        public string UnitId { get; set; }
        public int SkillId { get; set; }
        public List<string> TargetUnitId { get; set; }
    }
}
