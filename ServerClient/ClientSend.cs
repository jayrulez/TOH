using System;
using System.Collections.Generic;
using System.Text;
using TOH.Common.ServerData;

namespace ServerClient
{
    public class ClientSend
    {
        private static void SendTcpData(Packet packet)
        {
            packet.WriteLength();
            ClientWrapper.Instance.Tcp.SendData(packet);
        }

        public static void WelcomeReceived()
        {
            using(Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(ClientWrapper.Instance.Id);
                packet.Write("Client boss");

                SendTcpData(packet);
            }
        }

    }
}
