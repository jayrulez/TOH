using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Server;
using TOH.Server.Systems;

namespace TOH.Server
{
    public class GameServer : AbstractTcpServer
    {
        private readonly MatchLobbyService _matchLobbyService;
        private readonly MatchService _matchService;

        public GameServer(IHost host) : base(host)
        {
            _matchLobbyService = host.Services.GetRequiredService<MatchLobbyService>();
            _matchService = host.Services.GetRequiredService<MatchService>();
            _logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<GameServer>();
        }

        protected override async Task TickSystems()
        {
            await _matchLobbyService.Tick();
            await _matchService.Tick();
        }


        public bool StartSession(IConnection connection, string packetKey, byte[] packetBuffer)
        {
            return false;
        }

        /*
        public async override Task OnConnected(IConnection connection, CancellationToken cancellationToken)
        {
            var packet = await connection.GetPacket();

            var authenticationPackets = new List<string>()
            {
                typeof(LoginRequestPacket).Name,
                typeof(ReconnectRequestPacket).Name
            };

            if (authenticationPackets.Contains(packet.Packet.PacketType))
            {
                if (StartSession(connection, packet.Packet.PacketType, packet.PacketBytes))
                {
                    await base.OnConnected(connection, cancellationToken);
                }
                else
                {
                    await connection.Send(new ErrorResponsePacket
                    {
                        ErrorCode = (int)ErrorCode.AuthenticationFailed,
                        ErrorDescription = $"Authentication failed."
                    });

                    connection.Close();
                }
            }
            else
            {
                await connection.Send(new ErrorResponsePacket
                {
                    ErrorCode = (int)ErrorCode.LoginRequired,
                    ErrorDescription = $"Unexpected packet type received. Please establish a session with the server."
                });

                connection.Close();
            }
        }
        */
    }
}
