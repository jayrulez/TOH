using Bases;
using System;
using System.Collections.Generic;
using System.Text;


namespace GameClient
{
    public class ClientHandler
    {
        public static void Welcome(Packet packet)
        {


            Console.WriteLine($"Server says {packet}");
        }
    }
}
