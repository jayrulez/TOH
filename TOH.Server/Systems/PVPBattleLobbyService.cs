using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;

namespace TOH.Server.Systems
{
    public class PVPBattleLobbyService
    {
        private readonly ILogger _logger;
        private readonly BattleSystem _matchService;

        public List<IConnection> Connections { get; private set; } = new List<IConnection>();

        public PVPBattleLobbyService(BattleSystem matchService, ILogger<PVPBattleLobbyService> logger)
        {
            _matchService = matchService;
            _logger = logger;
        }

        public Task JoinLobbyQueue(IConnection connection)
        {
            if (!Connections.Any(c => c.Id.Equals(connection.Id)))
            {
                Connections.Add(connection);
            }

            return Task.CompletedTask;
        }

        private async Task CreateBattle(IConnection connection1, IConnection connection2)
        {
            var match = await _matchService.CreateBattle(connection1, connection2);

            var matchInfoPacket = new BattleInfoPacket
            {
                MatchId = match.Id
            };

            await connection1.Send(matchInfoPacket);
            await connection2.Send(matchInfoPacket);

            _logger.LogInformation($"Match created: {match.Id} with clients '{connection1.Id}' and '{connection2.Id}'.");
        }

        public async Task Tick()
        {
            if (Connections.Count > 1)
            {
                var players = Connections.Take(2).ToList();

                Connections.RemoveAll(c => players.Any(p => p.Id.Equals(c.Id)));

                await CreateBattle(players[0], players[1]);
            }
        }
    }
}
