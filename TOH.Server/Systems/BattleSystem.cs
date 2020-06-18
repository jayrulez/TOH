using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;

namespace TOH.Server.Systems
{

    public class BattleSystem
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private ConcurrentDictionary<string, PVPBattle> Matches = new ConcurrentDictionary<string, PVPBattle>();

        public BattleSystem(ILogger<BattleSystem> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public Task<PVPBattle> CreateBattle(IConnection connection1, IConnection connection2)
        {
            var match = new PVPBattle(connection1, connection2, _loggerFactory);

            Matches.TryAdd(match.Id, match);

            return Task.FromResult(match);
        }

        public Task PushTurnCommand(BattleTurnCommandPacket packet)
        {
            if(Matches.TryGetValue(packet.MatchId, out var match))
            {
                match.SetTurnCommand(packet.UnitId, packet.SkillId, packet.TargetUnitId);
            }

            return Task.CompletedTask;
        }

        public Task SetMatchTeam(string matchId, string connectionId, List<int> units)
        {
            if (Matches.TryGetValue(matchId, out var match))
            {
                match.SetUnits(connectionId, units);
            }

            return Task.CompletedTask;
        }

        public Task Tick()
        {
            //var to = DateTime.Now.AddSeconds(10);
            //var now = DateTime.Now;

            //while(now < to)
            //{
            //    now = DateTime.Now;
            //}

            Parallel.ForEach(Matches, match =>
            {
                match.Value.Tick();
            });

            return Task.CompletedTask;
        }
    }
}
