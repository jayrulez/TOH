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
        public int PortNumber = 8595;
        public int Id { get; set; }
        public ClientTcp Tcp;

        public static ClientWrapper Instance
        {
            get
            {
                return instance;
            }
        }

        private ClientWrapper()
        {
            Tcp = new ClientTcp(BufferSize,IP,PortNumber);
        }

        static ClientWrapper() { }




        public void ConnectToServer()
        {
            Tcp.Connect();
            Tcp.InitializeHandlers();
        }

    }

    public class ClientTcp
    {
        public TcpClient Socket;
        private NetworkStream NetStream;
        private byte[] Buffer;
        private int BufferSize;
        private IPAddress IP;
        private int Port;
        private Packet ReceivedData;
        private delegate void PacketHandler(Packet packet);
        private static Dictionary<int, PacketHandler> PacketHandlers;


        public ClientTcp(int bufferSize, IPAddress ip, int port)
        {
            BufferSize = bufferSize;
            IP = ip;
            Port = port;
        }

        public void Connect()
        {
            Console.WriteLine("Connecting to server");
            Socket = new TcpClient()
            {
                ReceiveBufferSize = BufferSize,
                SendBufferSize = BufferSize
            };
            ReceivedData = new Packet();
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
            Console.WriteLine("Connected to server");
        }

        public void SendData(Packet packet)
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

        private void SendPacket(Packet packet)
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

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = NetStream.EndRead(result);
                if (byteLength <= 0)
                    return;

                byte[] data = new byte[byteLength];
                Array.Copy(Buffer, data, byteLength);

                ReceivedData.Reset(HandleData(data));
                NetStream.BeginRead(Buffer, 0, 1024, ReceivedCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            ReceivedData.SetBytes(data);

            if (ReceivedData.UnreadLength() >= 4)
            {
                packetLength = ReceivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength < ReceivedData.UnreadLength())
            {
                byte[] packetBytes = ReceivedData.ReadBytes(packetLength);
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    PacketHandlers[packetId](packet);
                }

                packetLength = 0;
                if (ReceivedData.UnreadLength() >= 4)
                {
                    packetLength = ReceivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            return (packetLength <= 1);
        }


        public void InitializeHandlers()
        {
            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.welcome,ClientHandler.Welcome }
            };

            Console.WriteLine("Initialized packets");
        }
    }


}
