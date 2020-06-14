using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TOH.Networking.Common;
using TOH.Networking.Packets;
using TOH.Networking.Server;

namespace TOH.Server.Services
{
    class PingHandler : IPacketHandler<PingPacket>
    {
        private readonly ILogger _logger;

        public PingHandler(ILogger<PingHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(TcpConnection connection, PingPacket packet)
        {
            _logger.LogInformation($"Ping Packet received with Timestamp='{packet.Timestamp}'.");

            await connection.Send(new PongPacket
            {
                Message = $"Ping: '{packet.Timestamp}' Handled successfully."
            });
        }
    }
}
