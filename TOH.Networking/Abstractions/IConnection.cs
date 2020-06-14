using System.Threading.Tasks;

namespace TOH.Networking.Abstractions
{
    public interface IConnection<TPacket> where TPacket : Packet
    {
        string Id { get; }
        bool IsClosed { get; }
        Task Send(TPacket packet);
        Task<Packet> GetPacket();
        void Close();
    }
}
