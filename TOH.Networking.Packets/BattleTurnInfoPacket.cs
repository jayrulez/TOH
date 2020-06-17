using System.Collections.Generic;
using TOH.Common.BattleSystem;
using TOH.Common.Data;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleTurnInfoPacket : Packet
    {
        public BattleUnit Unit { get; set; }
        public int SkillId { get; set; }

        public List<BattleUnit> Targets { get; set; }
    }
}
