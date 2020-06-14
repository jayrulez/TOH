using System;
using System.Collections.Generic;
using System.Text;
using TOH.Common.ServerData;

namespace ServerHost.ServerData
{
    public class ServerHandler
    {
        public static void Welcomereceived(int fromClient,Packet packet)
        {
            int packetId = packet.ReadInt();
            string userName = packet.ReadString();

            Console.WriteLine($"{ServerSetup.ClientSockets[packetId].Tcp.Socket.Client.RemoteEndPoint} sent a packet and is now {fromClient}");

            if(fromClient != packetId)
            {
                Console.WriteLine($"This should not be printed but if it is clientId:{fromClient} is not packetId: {packetId}");
            }
        }
    }
}
