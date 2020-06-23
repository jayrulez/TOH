using System.Collections.Generic;
using TOH.Common.BattleSystem;
using TOH.Common.Data;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleReadyPacket : Packet
    {
        public string BattleId { get; set; }

        public List<BattlePlayer> Players { get; set; }
    }
}
