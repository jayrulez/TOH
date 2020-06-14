using System;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";
            ServerSetup Server = new ServerSetup();
            Server.InitServer();
            Console.ReadKey();
            
        }
    }
}
