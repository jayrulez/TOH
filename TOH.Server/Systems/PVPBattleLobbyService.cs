using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOH.Network.Packets;
using TOH.Server.Services;

namespace TOH.Server.Systems
{
    public class PVPBattleLobbyService
    {
        private readonly ILogger _logger;
        private readonly PVPBattleSystemService _battleSystemService;
        private readonly SessionService _sessionService;

        public List<ActiveSession> Sessions { get; private set; } = new List<ActiveSession>();

        public PVPBattleLobbyService(PVPBattleSystemService battleSystemService, SessionService sessionService, ILogger<PVPBattleLobbyService> logger)
        {
            _battleSystemService = battleSystemService;
            _sessionService = sessionService;
            _logger = logger;
        }

        public Task JoinQueue(ActiveSession session)
        {
            if (!Sessions.Any(c => c.SessionId.Equals(session.SessionId)))
            {
                Sessions.Add(session);
            }

            return Task.CompletedTask;
        }

        private async Task CreateBattle(ActiveSession session1, ActiveSession session2)
        {
            var battle = await _battleSystemService.CreateBattle(session1, session2);

            var matchInfoPacket = new BattleInfoPacket
            {
                BattleId = battle.Id
            };

            await session1.Connection.Send(matchInfoPacket);
            await session2.Connection.Send(matchInfoPacket);

            _logger.LogInformation($"Battle created: {battle.Id} with players '{session1.PlayerId}' and '{session2.PlayerId}'.");
        }

        public async Task Tick()
        {
            // Remove dead sessions from lobby
            Sessions.ForEach(async session =>
            {
                var activeSession = await _sessionService.GetActiveSession(session.Connection);

                if (activeSession == null)
                {
                    Sessions.Remove(session);
                }
            });

            if (Sessions.Count > 0)
            {
                var matches = Sessions.Take(2).ToList();

                Sessions.RemoveAll(session => matches.Any(m => m.SessionId.Equals(session.SessionId)));

                await CreateBattle(matches[0], matches[1]);
            }
        }
    }
}
