using Newtonsoft.Json;
using System;
using System.Text;

namespace Bases
{
    public class Welcome 
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public Welcome() { }

        public Welcome(byte[] data)
        {
            Id = BitConverter.ToInt32(data, 0);
            Name = Encoding.ASCII.GetString(data);
        }


       
    }
}
