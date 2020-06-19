using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public enum SessionDisconnectedCode
    {
        None
    }

    public class SessionDisconnectedPacket : Packet
    {
        public SessionDisconnectedCode Code { get; set; }
    }
}
