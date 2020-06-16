using System;
using System.Collections.Generic;
using System.Text;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public class SetMatchTeamPacket : Packet
    {
        public string MatchId { get; set; }

        public List<int> Units { get; set; }
    }
}
