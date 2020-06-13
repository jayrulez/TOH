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
        public int PortNumber = 8593;
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
            Tcp = new ClientTcp();
        }

        static ClientWrapper() { }




        public void ConnectToServer()
        {
            Tcp.Connect();
            
        }
    }


    public class ClientTcp
    {
        public TcpClient Socket;
        private NetworkStream NetStream;
        private byte[] Buffer;


        public void Connect()
        {
            Socket = new TcpClient()
            {
                ReceiveBufferSize = ClientWrapper.Instance.BufferSize,
                SendBufferSize = ClientWrapper.Instance.BufferSize
            };

            Buffer = new byte[ClientWrapper.Instance.BufferSize];
            Socket.BeginConnect(ClientWrapper.Instance.IP, ClientWrapper.Instance.PortNumber, ConnectCallback, Socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            Socket.EndConnect(result);
            if (!Socket.Connected)
            {
                return;
            }

            NetStream = Socket.GetStream();

            NetStream.BeginRead(Buffer, 0, ClientWrapper.Instance.BufferSize, ReceivedCallback, null);
        }

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = NetStream.EndRead(result);
                if (byteLength <= 0)
                    return;

                byte[] data = new byte[byteLength];
                Array.Copy(Buffer, data, byteLength);

                NetStream.BeginRead(Buffer, 0, ServerTcp.DataBufferSize, ReceivedCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
   
}
