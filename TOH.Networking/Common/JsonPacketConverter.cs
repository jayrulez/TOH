using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using TOH.Networking.Abstractions;

namespace TOH.Networking.Common
{
    public class JsonPacketConverter : IPacketConverter<Packet>
    {
        public Packet FromBytes(byte[] packetBytes)
        {
            try
            {
                var data = Encoding.UTF8.GetString(packetBytes).Trim('\0');

                var packet = JsonConvert.DeserializeObject<Packet>(data);

                packet.SetData(packetBytes);

                return packet;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] ToBytes<T>(T packet) where T : Packet
        {
            packet.Type = packet.GetType().FullName;

            var data = JsonConvert.SerializeObject(packet);

            return Encoding.UTF8.GetBytes(data);
        }

        public T Unwrap<T>(Packet packet) where T : Packet
        {
            var packetBytes = packet.GetData();

            var data = Encoding.UTF8.GetString(packetBytes).Trim('\0');

            var unwrappedPacket = JsonConvert.DeserializeObject(data) as JObject;

            return unwrappedPacket.ToObject<T>();
        }

        public Packet Unwrap(Packet packet)
        {
            var packetBytes = packet.GetData();

            var data = Encoding.UTF8.GetString(packetBytes).Trim('\0');

            var unwrappedPacket = JsonConvert.DeserializeObject(data) as JObject;

            return unwrappedPacket.ToObject(Type.GetType(packet.Type)) as Packet;
        }
    }
}
