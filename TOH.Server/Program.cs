using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TOH.Server.ServerData;

namespace TOH.Server
{
    public class Program
    {
        

        private static ServerSetup Server;
        static void Main(string[] args)
        {
            Console.Title = "Server";
            Server = new ServerSetup();
            Server.InitServer();
            Console.ReadKey();
        }

       
    }
}
