using System;

namespace TOH.Network.Abstractions
{
    public class Packet
    {
        private string _packetId;
        private byte[] _data;

        public string PacketId
        {
            get
            {
                if (string.IsNullOrEmpty(_packetId))
                {
                    _packetId = Guid.NewGuid().ToString();
                }

                return _packetId;
            }
        }

        public string Type { get; set; }
        public byte[] GetData()
        {
            return _data;
        }

        public void SetData(byte[] data)
        {
            _data = data;
        }
    }
}
