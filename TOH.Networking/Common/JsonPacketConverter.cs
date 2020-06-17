using TOH.Network.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TOH.Network.Common
{
    public class JsonPacketConverter : IPacketConverter
    {
        private const string PacketTerminator = "::+**+::";
        public T FromBytes<T>(byte[] packetBytes) where T : Packet
        {
            try
            {
                var data = Encoding.UTF8.GetString(packetBytes).Trim('\0');

                var packet = JsonConvert.DeserializeObject<T>(data);

                packet.SetData(packetBytes);

                return packet;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public async IAsyncEnumerable<T> StreamFromBytes<T>(byte[] streamBytes) where T : Packet
        {
            var packets = new List<Packet>();
            var data = Encoding.UTF8.GetString(streamBytes).Trim('\0');

            var packetsData = data.Split(new string[] { PacketTerminator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var packetData in packetsData)
            {
                if (string.IsNullOrEmpty(packetData))
                    continue;

                var packet = JsonConvert.DeserializeObject<T>(packetData);
                var packetBytes = Encoding.UTF8.GetBytes(packetData);
                packet.SetData(packetBytes);
                packets.Add(packet);

                yield return packet;
            }

            //await Task.Yield();
        }

        public byte[] ToBytes<T>(T packet) where T : Packet
        {
            packet.Type = packet.GetType().FullName;

            var data = JsonConvert.SerializeObject(packet);

            data += PacketTerminator;

            return Encoding.UTF8.GetBytes(data);
        }

        public T Unwrap<T>(Packet packet) where T : Packet
        {
            try
            {
                var packetBytes = packet.GetData();

                var data = Encoding.UTF8.GetString(packetBytes).Trim('\0');

                var unwrappedPacket = JsonConvert.DeserializeObject(data) as JObject;

                return unwrappedPacket.ToObject<T>();
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }
    }
}
