using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Text;
using TOH.Common.ServerData;

namespace GameServer
{
    public class ServerTcp
    {
        private int Id { get; set; }
        public TcpClient Socket;
        private NetworkStream NetStream;
        private byte[] ReceivedBuffer;
        private int DataBufferSize;
        private Packet ReceivedPacket;


        public ServerTcp(int id,int bufferSize)
        {
            Id = id;
            DataBufferSize = bufferSize;
        }

        public void SendPacket(Packet packet)
        {
            try
            {
                if (Socket != null)
                {
                    NetStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Connect(TcpClient socket)
        {
            ReceivedPacket = new Packet();
            Socket = socket;
            Socket.ReceiveBufferSize = DataBufferSize;
            Socket.SendBufferSize = DataBufferSize;
            NetStream = Socket.GetStream();

            ReceivedBuffer = new byte[DataBufferSize];
            NetStream.BeginRead(ReceivedBuffer, 0, DataBufferSize, ReceivedCallback, null);
        }

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = NetStream.EndRead(result);
                if (byteLength <= 0)
                    return;

                byte[] data = new byte[byteLength];
                Array.Copy(data, ReceivedBuffer, byteLength);

                Console.WriteLine(data);
                NetStream.BeginRead(ReceivedBuffer, 0, DataBufferSize, ReceivedCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
               
            }
        }


        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            ReceivedPacket.SetBytes(data);

            if (ReceivedPacket.UnreadLength() >= 4)
            {
                packetLength = ReceivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength < ReceivedPacket.UnreadLength())
            {
                byte[] packetBytes = ReceivedPacket.ReadBytes(packetLength);
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    ServerSetup.PacketHandlers[packetId](Id, packet);
                }

                packetLength = 0;
                if (ReceivedPacket.UnreadLength() >= 4)
                {
                    packetLength = ReceivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            return (packetLength <= 1);
        }



    }
}
