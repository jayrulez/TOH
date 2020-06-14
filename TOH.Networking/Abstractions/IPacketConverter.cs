namespace TOH.Networking.Abstractions
{
    public interface IPacketConverter<TPacket> where TPacket : Packet
    {
        byte[] ToBytes<T>(T packet) where T : TPacket;

        TPacket FromBytes(byte[] packetBytes);

        T Unwrap<T>(TPacket packet) where T : TPacket;
    }
}