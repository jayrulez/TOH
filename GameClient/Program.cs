using System;
using TOH.Common.ServerData;

namespace GameClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";
            string command = "";
            const int CLIENT_ID = 1;
            ClientWrapper.Instance.ConnectToServer();
            while (!command.ToLower().Equals("exit"))
            {
                switch (command)
                {
                    case "clear":
                            Console.Clear();
                        break;
                    case "msg":
                            string message = Console.ReadLine();
                            ClientSend.WelcomeReceived();
                       
                        break;
                }

                Console.WriteLine($"Enter new command");
                command = Console.ReadLine().ToLower();
                Console.WriteLine($"Command is {command}");

            }
            Console.ReadLine();
        }
    }
}
