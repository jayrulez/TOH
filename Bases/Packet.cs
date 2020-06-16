using System;
using System.Collections.Generic;
using System.Text;

namespace Bases
{
    public enum ServerPackets
    {
        welcome = 1
    }

    /// <summary>Sent from client to server.</summary>
    public enum ClientPackets
    {
        welcomeReceived = 1
    }

    public class Packet
    {
        public int Id;

        public byte[] Data { get; set; }

        public int Length { get; set; }

    }
}
