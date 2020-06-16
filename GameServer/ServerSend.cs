using System;
using System.Collections.Generic;
using System.Text;
using TOH.Common.ServerData;

namespace GameServer
{
   public class ServerSend
    {
        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTcpToAll( packet);
            }
        }

        public static void SendTcpData(int client, Packet packet)
        {
            packet.WriteLength();
            ServerSetup.ClientSockets[client].SendPacket(packet);
        }

        public static void SendTcpToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 0; i < ServerSetup.MaxConnections; i++)
            {
                var sock = ServerSetup.ClientSockets[i];
                if (sock.Socket != null && sock.Socket.Connected)
                    sock.SendPacket(packet);
            }
        }

        public static void SendTcpToAllExcept(int excludeSocket, Packet packet)
        {
            packet.WriteLength();
            for (int i = 0; i < ServerSetup.MaxConnections; i++)
            {
                if (i != excludeSocket)
                    ServerSetup.ClientSockets[i].SendPacket(packet);
            }
        }
    }
}
