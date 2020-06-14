using System.Text;

namespace TOH.Networking.Abstractions
{
    public class Packet
    {
        public string Type { get; set; }

        private byte[] _data;

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