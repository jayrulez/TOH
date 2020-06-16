using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TOH.Network.Abstractions;
using TOH.Network.Packets;

namespace TOH.Server.Systems
{

    public class MatchService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private ConcurrentDictionary<string, Match> Matches = new ConcurrentDictionary<string, Match>();

        public MatchService(ILogger<MatchService> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public Task<Match> CreateMatch(IConnection connection1, IConnection connection2)
        {
            var match = new Match(connection1, connection2, _loggerFactory);

            Matches.TryAdd(match.Id, match);

            return Task.FromResult(match);
        }

        public Task PushTurnCommand(MatchTurnCommandPacket packet)
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
                match.SetTeam(connectionId, units);
            }

            return Task.CompletedTask;
        }

        public Task Tick()
        {
            Parallel.ForEach(Matches, match =>
            {
                match.Value.Tick();
            });

            return Task.CompletedTask;
        }
    }
}
