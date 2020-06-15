using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;

namespace TOH.Server.Systems
{
    public class MatchLobbyService
    {
        private readonly ILogger _logger;
        private readonly MatchService _matchService;

        public List<IConnection> Connections { get; private set; } = new List<IConnection>();

        public MatchLobbyService(MatchService matchService, ILogger<MatchLobbyService> logger)
        {
            _matchService = matchService;
            _logger = logger;
        }

        public Task FindMatch(IConnection connection)
        {
            if (!Connections.Any(c => c.Id.Equals(connection.Id)))
            {
                Connections.Add(connection);
            }

            return Task.CompletedTask;
        }

        private async Task CreateMatch(IConnection connection1, IConnection connection2)
        {
            var match = await _matchService.CreateMatch(connection1, connection2);

            var matchInfoPacket = new MatchInfoPacket
            {
                MatchId = match.Id
            };

            await connection1.Send(matchInfoPacket);
            await connection2.Send(matchInfoPacket);
        }

        public async Task Tick()
        {
            _logger.LogInformation("Lobby Tick");

            if (Connections.Count > 1)
            {
                var players = Connections.Take(2).ToList();

                Connections.RemoveAll(c => players.Any(p => p.Id.Equals(c.Id)));

                await CreateMatch(players[0], players[1]);
            }
        }
    }
}
