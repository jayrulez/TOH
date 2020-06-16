

using Bases;
using System;

namespace GameClient
{
    public class ClientSend
    {
        private static void SendTcpData(Packet packet)
        {
            
            ClientWrapper.Instance.Tcp.SendData(packet);
        }

        public static void SendData()
        {
            Packet p = new Packet();
            p.Data = BitConverter.GetBytes(334);
            SendTcpData(p);
        }

        public static void WelcomeReceived()
        {
          
        }

    }
}
