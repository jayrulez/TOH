using System.Collections.Generic;
using TOH.Common.Data;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleInfoPacket : Packet
    {
        public string BattleId { get; set; }
        public List<PlayerModel> Players { get; set; }
    }
}