using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerClient
{
    public class Program
    {

        


        static void Main(string[] args)
        {
            Console.Title = "Client";
            ClientWrapper.Instance.ConnectToServer();
            Console.ReadLine();
        }


    }
}
