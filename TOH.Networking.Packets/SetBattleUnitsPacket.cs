﻿using System.Collections.Generic;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class SetBattleUnitsPacket : Packet
    {
        public string BattleId { get; set; }

        public List<int> Units { get; set; }
    }
}
