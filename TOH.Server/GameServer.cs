using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network.Abstractions;
using TOH.Network.Server;
using TOH.Server.Systems;

namespace TOH.Server
{
    public class GameServer : AbstractTcpServer
    {
        private readonly PVPBattleLobbyService _matchLobbyService;
        private readonly BattleSystem _matchService;

        public GameServer(IHost host) : base(host)
        {
            _matchLobbyService = host.Services.GetRequiredService<PVPBattleLobbyService>();
            _matchService = host.Services.GetRequiredService<BattleSystem>();
            Logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<GameServer>();
        }

        public async override Task StartAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation($"Loading data...");
            await DataManager.Instance.Initialize("Config");
            while(DataManager.Instance.State != DataManagerState.Initialized)
            {

            }
            Logger.LogInformation($"Loaded data successfully.");

            await base.StartAsync(cancellationToken);
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
