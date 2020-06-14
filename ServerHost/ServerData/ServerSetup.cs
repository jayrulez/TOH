using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TOH.Common.ServerData;

namespace ServerHost.ServerData
{
    public class ServerSetup
    {
        //private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private TcpListener ServerSocket;
        public static Dictionary<int, ServerTcp> ClientSockets = new Dictionary<int, ServerTcp>();
        public delegate void PacketHandler(int id, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlers;
        private byte[] Buffer = new byte[1024];
        private int PortNumber = 8595;
        public static int MaxConnections = 10;


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

            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived,ServerHandler.Welcomereceived }
            };

            Console.WriteLine("Initialized packets on server");
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
        private Packet ReceivedPacket;


        public TCP(int _id)
        {
            Id = _id;
        }

        public void Conenct(TcpClient socket)
        {
            ReceivedPacket = new Packet();
            Socket = socket;
            Socket.ReceiveBufferSize = ServerTcp.DataBufferSize;
            Socket.SendBufferSize = ServerTcp.DataBufferSize;

            NetStream = Socket.GetStream();
            ReceivedBuffer = new byte[ServerTcp.DataBufferSize];

            NetStream.BeginRead(ReceivedBuffer, 0, ServerTcp.DataBufferSize, ReceivedCallback, null);
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

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = NetStream.EndRead(result);
                if (byteLength <= 0)
                    return;

                byte[] data = new byte[byteLength];
                Array.Copy(ReceivedBuffer, data, byteLength);

                ReceivedPacket.Reset(HandleData(data));
                NetStream.BeginRead(ReceivedBuffer, 0, ServerTcp.DataBufferSize, ReceivedCallback, null);
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
                    ServerSetup.PacketHandlers[packetId](Id,packet);
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
