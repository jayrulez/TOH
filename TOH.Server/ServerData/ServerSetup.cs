using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TOH.Server.ServerData
{
    public class ServerSetup
    {
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private List<Socket> ClientSocks = new List<Socket>();
        private byte[] Buffer = new byte[1024];

        public ServerSetup() { }

        public void InitServer()
        {
            Console.WriteLine("Setting up server for TOH...");
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 8083));
            ServerSocket.Listen(5);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptedCallBack), null);
        }


        private void AcceptedCallBack(IAsyncResult result)
        {
            Socket sock = ServerSocket.EndAccept(result);
            ClientSocks.Add(sock);
            
            sock.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), sock);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptedCallBack), sock);
            Console.WriteLine("Client connected...");
        }

        private void ReceivedCallback(IAsyncResult result)
        {
            Socket sock = (Socket)result.AsyncState;
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
            Console.WriteLine("testing received");
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
