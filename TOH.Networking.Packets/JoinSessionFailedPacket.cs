using System;
using System.Collections.Generic;
using System.Text;
using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public enum JoinSessionFailCode
    {
        InvalidSession,
        RetryRequired
    }

    public class JoinSessionFailedPacket : Packet
    {
        public JoinSessionFailCode Code { get; set; }
    }
}
