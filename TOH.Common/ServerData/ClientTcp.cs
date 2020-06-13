using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TOH.Common.ServerData
{
    public class ClientTcp
    {
        public TcpClient Socket;
        private NetworkStream NetStream;
        private byte[] Buffer;
        private int BufferSize;
        private IPAddress IP;
        private int Port;


        public ClientTcp(int bufferSize,IPAddress ip, int port)
        {
            BufferSize = bufferSize;
            IP = ip;
            Port = port;
        }

        public void Connect()
        {
            Socket = new TcpClient()
            {
                ReceiveBufferSize = BufferSize,
                SendBufferSize = BufferSize
            };

            Buffer = new byte[BufferSize];
            Socket.BeginConnect(IP, Port, ConnectCallback, Socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            Socket.EndConnect(result);
            if (!Socket.Connected)
            {
                return;
            }

            NetStream = Socket.GetStream();

            NetStream.BeginRead(Buffer, 0, BufferSize, ReceivedCallback, null);
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
