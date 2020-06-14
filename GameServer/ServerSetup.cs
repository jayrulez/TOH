using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TOH.Common.ServerData;

namespace GameServer
{
    public class ServerSetup
    {
        private TcpListener ServerSocket;
        public static Dictionary<int, ServerTcp> ClientSockets = new Dictionary<int, ServerTcp>();
        private int BufferSize = 4096;
        private byte[] Buffer;
        private int PortNumber = 8595;
        public static int MaxConnections = 10;
        public delegate void PacketHandler(int id, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlers;

        public ServerSetup()
        {
            InitializeConnections();
        }

        public void InitServer()
        {

            Console.WriteLine("Setting up Server");
            Buffer = new byte[BufferSize];
            ServerSocket = new TcpListener(IPAddress.Any, PortNumber);
            ServerSocket.Start();
            ServerSocket.BeginAcceptTcpClient(new AsyncCallback(AcceptedCallBack), null);

            Console.WriteLine($"Listening on {PortNumber}");
        }

        private void AcceptedCallBack(IAsyncResult result)
        {
            TcpClient socket = ServerSocket.EndAcceptTcpClient(result);
            ServerSocket.BeginAcceptTcpClient(new AsyncCallback(AcceptedCallBack), null);

            Console.WriteLine($"Listening on {socket.Client.RemoteEndPoint}...");

            for (int i = 1; i < MaxConnections; i++)
            {
                if (ClientSockets[i].Socket == null)
                {
                    ClientSockets[i].Connect(socket);
                    ServerSend.Welcome(1, "Welcome to the server");
                    return;

                }
            }

            Console.WriteLine("Server full...");
        }

        private void InitializeConnections()
        {
            for(int i = 0; i < MaxConnections; i++)
            {
                ClientSockets.Add(i, new ServerTcp(i, BufferSize));
            }

            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived,ServerHandler.Welcomereceived }
            };

            Console.WriteLine("Initialized packets on server");
        }
    }
}
