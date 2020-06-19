using TOH.Network.Abstractions;

namespace TOH.Network.Packets
{
    public enum JoinSessionSuccessCode
    {
        Added,
        Updated
    }

    public class JoinSessionSuccessPacket : Packet
    {
        public JoinSessionSuccessCode Code { get; set; }
    }
}
