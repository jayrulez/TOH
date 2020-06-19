using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Services;
using TOH.Server.Systems;

namespace TOH.Server
{
    public class GameServer : AbstractTcpServer
    {
        private readonly PVPBattleLobbyService _matchLobbyService;
        private readonly PVPBattleSystemService _matchService;
        private readonly SessionService _sessionService;

        public GameServer(IHost host) : base(host)
        {
            _matchLobbyService = host.Services.GetRequiredService<PVPBattleLobbyService>();
            _matchService = host.Services.GetRequiredService<PVPBattleSystemService>();
            _sessionService = host.Services.GetRequiredService<SessionService>();

            Logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<GameServer>();
        }

        public async override Task StartAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation($"Loading data...");
            await DataManager.Instance.Initialize("Config");

            while (DataManager.Instance.State != DataManagerState.Initialized)
            {

            }

            Logger.LogInformation($"Loaded data successfully.");

            await base.StartAsync(cancellationToken);
        }

        protected override async Task TickSystems()
        {
            await _matchService.Tick();
            await _matchLobbyService.Tick();
        }

        protected async override Task OnPacketReceived(IConnection connection, Packet packet)
        {
            if (_packetConverter.CanUnwrap<JoinSessionPacket>(packet))
            {
                await _sessionService.JoinSession(connection, _packetConverter.Unwrap<JoinSessionPacket>(packet));
            }
            else
            {
                await base.OnPacketReceived(connection, packet);
            }
        }

        protected async override Task OnDisconnected(IConnection connection)
        {
            await _sessionService.TryDisconnect(connection);
            await base.OnDisconnected(connection);
        }
    }
}
