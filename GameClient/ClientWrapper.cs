using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GameClient
{
    public class ClientWrapper
    {
        public int BufferSize = 1024;
        public IPAddress IP = IPAddress.Loopback;
        public int PortNumber = 8595;
        public int Id { get; set; }
        public ClientTcp Tcp;

        public static ClientWrapper Instance { get; } = new ClientWrapper();

        private ClientWrapper()
        {
            Tcp = new ClientTcp(BufferSize, IP, PortNumber);
        }

        static ClientWrapper() { }

        public void ConnectToServer()
        {
            Tcp.Connect();
            Tcp.InitializeHandlers();
        }
    }
}
