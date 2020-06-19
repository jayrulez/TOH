using System;
using System.Collections.Generic;
using System.Text;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class BattleCountdownPacket : Packet
    {
        public int Count { get; set; }
    }
}
