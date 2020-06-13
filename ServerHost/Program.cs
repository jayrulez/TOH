using ServerHost.ServerData;
using System;

namespace ServerHost
{
    class Program
    {

        

        static void Main(string[] args)
        {
            ServerSetup Server = new ServerSetup();
            Server.InitServer();
            Console.ReadLine();
        }
    }
}
