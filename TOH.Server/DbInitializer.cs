using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using TOH.Common.Services;
using TOH.Server.Data;
using TOH.Server.Services;

namespace TOH.Server
{
    public class DBInitializer
    {
        private readonly ILogger _logger;
        private readonly GameDbContext _dbContext;
        private readonly PlayerManager _playerManager;
        private readonly PlayerService _playerService;

        public DBInitializer(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider;

            _logger = serviceProvider.GetRequiredService<ILogger<DBInitializer>>();
            _dbContext = serviceProvider.GetRequiredService<GameDbContext>();
            _playerManager = serviceProvider.GetRequiredService<PlayerManager>();
            _playerService = serviceProvider.GetRequiredService<PlayerService>();
        }

        public void Run()
        {
            _logger.LogInformation("Starting Data Initialization");


            var players = _dbContext.Players.ToList();

            if(!players.Any())
            {
                _logger.LogInformation($"Creating Players");
                var p1 = _playerService.CreatePlayer(new IdentifierData<string> { Identifier = "robert" }, default);
                var p2 = _playerService.CreatePlayer(new IdentifierData<string> { Identifier = "evon" }, default);
            }

            var sessions = _dbContext.PlayerSessions.ToList();

            if(!sessions.Any())
            {
                _logger.LogInformation($"Creating PlayerSessions");
                var s1 = _playerService.CreatePlayerSession(new IdentifierData<string> { Identifier = "robert" }, default);
                var s2 = _playerService.CreatePlayerSession(new IdentifierData<string> { Identifier = "evon" }, default);
            }

            _logger.LogInformation("Data Initialization Completed");
        }
    }
}