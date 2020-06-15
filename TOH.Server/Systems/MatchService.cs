using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TOH.Network.Abstractions;

namespace TOH.Server.Systems
{
    public class Match
    {
        public string Id { get; private set; }

        public List<IConnection> Connections { get; private set; }

        public Match(IConnection connection1, IConnection connection2)
        {
            Id = Guid.NewGuid().ToString();

            Connections.Add(connection1);
            Connections.Add(connection2);
        }

        public Task Tick()
        {
            return Task.CompletedTask;
        }
    }

    public class MatchService
    {
        private readonly ILogger _logger;

        private ConcurrentDictionary<string, Match> Matches = new ConcurrentDictionary<string, Match>();

        public MatchService(ILogger<MatchService> logger)
        {
            _logger = logger;
        }

        public Task<Match> CreateMatch(IConnection connection1, IConnection connection2)
        {
            var match = new Match(connection1, connection2);

            Matches.TryAdd(match.Id, match);

            return Task.FromResult(match);
        }

        public Task Tick()
        {
            _logger.LogInformation("Match Tick");

            Parallel.ForEach(Matches, match => {
                match.Value.Tick();
            });

            return Task.CompletedTask;
        }
    }
}
