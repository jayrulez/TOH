﻿using Bases;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameClient
{
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

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = NetStream.EndRead(result);
                if (byteLength <= 0)
                    return;

                byte[] data = new byte[byteLength];
                Array.Copy(Buffer, data, byteLength);


                NetStream.BeginRead(Buffer, 0, BufferSize, ReceivedCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        

        public void SendData(Packet packet)
        {
            try
            {
                if (Socket != null)
                {
                    NetStream.BeginWrite(packet.Data, 0, packet.Data.Length, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
