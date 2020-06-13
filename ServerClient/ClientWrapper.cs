using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TOH.Common.ServerData;

namespace ServerClient
{
    public sealed class ClientWrapper
    {
        private static readonly ClientWrapper instance = new ClientWrapper();
        public int BufferSize = 1024;
        public IPAddress IP = IPAddress.Loopback;
        public int PortNumber = 8595;
        private int Id { get; set; }
        private ClientTcp Tcp;

        public static ClientWrapper Instance
        {
            get
            {
                return instance;
            }
        }

        private ClientWrapper()
        {
            Tcp = new ClientTcp(BufferSize,IP,PortNumber);
        }

        static ClientWrapper() { }




        public void ConnectToServer()
        {
            Tcp.Connect();
            
        }
    }


 
   
}
