using System;
using System.Collections.Generic;
using System.Text;
using TOH.Common.ServerData;

namespace ServerClient
{
    public class ClientHandler
    {
        public static void Welcome(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();

            Console.WriteLine($"Server says {msg}");
            ClientWrapper.Instance.Id = id;
            ClientSend.WelcomeReceived();
        }
    }
}
