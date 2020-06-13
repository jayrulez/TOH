using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TOH.Common.ServerData;

namespace TOH.Server.ServerData
{
    public class ServerSetup
    {
        //private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private TcpListener ServerSocket;
        public Dictionary<int, ServerTcp> ClientSockets = new Dictionary<int, ServerTcp>();
        private byte[] Buffer = new byte[1024];
        private int PortNumber = 8593;
        private int MaxConnections = 10;


        public ServerSetup() { }

        public void InitServer()
        {
            Console.WriteLine("Setting up server for TOH...");
            SetInitialSlots();
            ServerSocket = new TcpListener(IPAddress.Any, PortNumber);
            ServerSocket.Start();
            ServerSocket.BeginAcceptTcpClient(new AsyncCallback(AcceptedCallBack),null);
            Console.WriteLine($"Listening on {PortNumber}");
        }


        private void AcceptedCallBack(IAsyncResult result)
        {
            TcpClient client = ServerSocket.EndAcceptTcpClient(result);
            ServerSocket.BeginAcceptTcpClient(new AsyncCallback(AcceptedCallBack),null);

            Console.WriteLine($"Listening on {client.Client.RemoteEndPoint}...");

            for (int i = 1; i < MaxConnections; i++)
            {
                if(ClientSockets[i].Tcp.Socket == null)
                {
                    ClientSockets[i].Tcp.Conenct(client);
                    return;

                }
            }

            Console.WriteLine("Server full...");
        }

        private void SetInitialSlots()
        {
            for(int i=1;i< MaxConnections; i++)
            {
                ClientSockets.Add(i, new ServerTcp(i));
            }
        }

        private void ReceivedCallback(IAsyncResult result)
        {
            Socket sock = (Socket)result.AsyncState;

            if (sock == null)
                return;

            int received = sock.EndReceive(result);  

            byte[] dataBuffer = new byte[received];
            Array.Copy(Buffer, dataBuffer, received);

            string text = Encoding.ASCII.GetString(dataBuffer);

            if (text.ToLower().Equals("test"))
            {
                SendText(sock, "testing received");
            }
            else
            {
                SendText(sock, "Invalid");
            }
        }

        private void SendText(Socket sock, string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            sock.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), sock);
            sock.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), sock);
        }


        private void SendCallback(IAsyncResult result)
        {
            Socket sock = (Socket)result.AsyncState;
            sock.EndSend(result);
            Console.WriteLine("Send complete!");
        }
    }
}
