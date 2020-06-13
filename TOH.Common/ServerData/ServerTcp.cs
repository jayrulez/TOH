using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TOH.Common.ServerData
{
   public class ServerTcp
    { 
        public int Id { get; set; }
        public TCP Tcp { get; set; }
        public static int DataBufferSize = 1024;


        public ServerTcp(int _id)
        {
            Id = _id;
            Tcp = new TCP(_id);
        }
    }


    public class TCP
    {
        public TcpClient Socket { get; set; }
        private readonly int Id;
        private byte[] ReceivedBuffer;
        private NetworkStream NetStream;



        public TCP(int _id)
        {
            Id = _id;
        }

        public void Conenct(TcpClient socket)
        {
            Socket = socket;
            Socket.ReceiveBufferSize = ServerTcp.DataBufferSize;
            Socket.SendBufferSize = ServerTcp.DataBufferSize;

            NetStream = Socket.GetStream();
            ReceivedBuffer = new byte[ServerTcp.DataBufferSize];

            NetStream.BeginRead(ReceivedBuffer, 0, ServerTcp.DataBufferSize, ReceivedCallback, null);
        }

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = NetStream.EndRead(result);
                if (byteLength <= 0)
                    return;

                byte[] data = new byte[byteLength];
                Array.Copy(ReceivedBuffer,data, byteLength);

                NetStream.BeginRead(ReceivedBuffer, 0, ServerTcp.DataBufferSize, ReceivedCallback, null);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
}
