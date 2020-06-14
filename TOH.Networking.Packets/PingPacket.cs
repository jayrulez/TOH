using System;
using TOH.Networking.Abstractions;

namespace TOH.Networking.Packets
{
    public class PingPacket : Packet
    {
        public DateTime Timestamp { get; set; }
    }
}
