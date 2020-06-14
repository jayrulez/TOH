using System;
using System.Collections.Generic;
using System.Text;
using TOH.Networking.Abstractions;

namespace TOH.Networking.Packets
{
    public class PongPacket : Packet
    {
        public string Message { get; set; }
    }
}
