using System;
using System.Collections.Generic;
using System.Text;
using TOH.Common.ServerData;

namespace ServerHost.ServerData
{
    public class ServerSend
    {

        public static void Welcome(int toClient, string msg)
        {
            using(Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTcpData(toClient, packet);
            }
        }

        public static void SendTcpData(int client, Packet packet)
        {
            packet.WriteLength();
            ServerSetup.ClientSockets[client].Tcp.SendPacket(packet);
        }

        public static void SendTcpToAll(Packet packet)
        {
            packet.WriteLength();
            for(int i=0;i<= ServerSetup.MaxConnections; i++)
            {
                ServerSetup.ClientSockets[i].Tcp.SendPacket(packet);
            }
        }

        public static void SendTcpToAllExcept(int excludeSocket, Packet packet)
        {
            packet.WriteLength();
            for (int i = 0; i <= ServerSetup.MaxConnections; i++)
            {
                if(i != excludeSocket)
                    ServerSetup.ClientSockets[i].Tcp.SendPacket(packet);
            }
        }
    }
}
