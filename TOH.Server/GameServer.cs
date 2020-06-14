using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using TOH.Networking.Server;

namespace TOH.Server
{
    class GameServer : AbstractTcpServer
    {
        public GameServer(IHost host) : base(host)
        {
        }
    }
}
