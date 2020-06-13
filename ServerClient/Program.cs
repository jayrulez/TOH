using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerClient
{
    public class Program
    {

        private static Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        static void Main(string[] args)
        {
            Console.Title = "Client";
            ClientWrapper.Instance.ConnectToServer();
            Console.ReadLine();
        }

        private static void TryConnect()
        {
            int attempts = 0;
            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    ClientSocket.Connect(IPAddress.Loopback, 8083);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Attemps: {attempts}");
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
           
        }


        private static void SendCommands()
        {
            while (true)
            {
                Console.WriteLine("Enter command...");
                string request = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(request);
                ClientSocket.Send(buffer);

                byte[] receivedBuffer = new byte[1024];
                int received = ClientSocket.Receive(receivedBuffer);
                byte[] receviedFromServer = new byte[received];
                Array.Copy(receivedBuffer, receviedFromServer, received);
                string response = Encoding.ASCII.GetString(receviedFromServer);

                Console.WriteLine($"Server says: {response}");
            }
        }
    }
}
