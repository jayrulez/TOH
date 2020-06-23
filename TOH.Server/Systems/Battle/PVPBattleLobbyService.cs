using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network.Packets;
using TOH.Server.Services;

namespace TOH.Server.Systems
{
    public class PVPBattleLobbyService
    {
        private readonly ILogger _logger;
        private readonly PVPBattleSystemService _battleSystemService;
        private readonly SessionService _sessionService;
        private readonly PlayerManager _playerManager;

        public List<Session> Sessions { get; private set; } = new List<Session>();

        public PVPBattleLobbyService(PVPBattleSystemService battleSystemService, PlayerManager playerManager, SessionService sessionService, ILogger<PVPBattleLobbyService> logger)
        {
            _battleSystemService = battleSystemService;
            _playerManager = playerManager;
            _sessionService = sessionService;
            _logger = logger;
        }

        public Task JoinQueue(Session session)
        {
            if (!Sessions.Any(c => c.SessionId.Equals(session.SessionId)))
            {
                Sessions.Add(session);
            }

            return Task.CompletedTask;
        }

        //public Player GetPlayerFromSession(string sessionId)
        //{
        //    if (string.IsNullOrEmpty(sessionId))
        //        return null;

        //    var session = ActiveSessions[sessionId];
        //    if (session == null)
        //        return null;

        //    var playerManager = _serviceProvider.GetRequiredService<PlayerManager>();

        //    var playerData = playerManager.GetPlayerById(session.PlayerId);

        //    if (playerData == null)
        //        return null;

        //    var player = new Player
        //    {
        //        Id = playerData.Id,
        //        Units = new List<PlayerUnit>()
        //    };

        //    foreach (var playerUnitData in playerData.Units)
        //    {
        //        player.Units.Add(new PlayerUnit(playerUnitData.Id, playerUnitData.Level, DataManager.Instance.GetUnit(playerUnitData.Id)));
        //    }

        //    return player;
        //}

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

            if (Sessions.Count > 1)
            {
                var matches = Sessions.Take(2).ToList();

                if(matches.Count == 2)
                {
                    Sessions.RemoveAll(session => matches.Any(m => m.SessionId.Equals(session.SessionId)));

                    var session1 = matches[0];
                    var session2 = matches[1];

                    var battle = await _battleSystemService.CreateBattle(session1, session2);

                    var matchInfoPacket = new BattleInfoPacket
                    {
                        BattleId = battle.Id,
                        Players = new List<PlayerModel>()
                    };

                    foreach(var player in battle.Players)
                    {
                        matchInfoPacket.Players.Add(player.Player);
                    }

                    await session1.Connection.Send(matchInfoPacket);
                    await session2.Connection.Send(matchInfoPacket);

                    _logger.LogInformation($"Battle created: {battle.Id} with players '{session1.PlayerId}' and '{session2.PlayerId}'.");
                }
            }
        }
    }
}
