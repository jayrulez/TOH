using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TOH.Server.Data;

namespace TOH.Server.Services
{
    public class PlayerManager
    {
        private readonly GameDbContext _dbContext;
        private readonly ILogger _logger;

        public PlayerManager(GameDbContext dbContext, ILogger<PlayerManager> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Player CreatePlayer(string username)
        {
            username = username.Trim();

            var player = _dbContext.Players.FirstOrDefault(p => p.Username.ToLower().Equals(username.ToLower()));

            if (player != null)
            {
                throw new EntityExistException();
            }

            player = new Player
            {
                Username = username,
                Level = 1
            };

            _dbContext.Players.Add(player);

            _dbContext.SaveChanges();

            return player;
        }

        public Player GetPlayerById(int id)
        {
            return _dbContext.Players.FirstOrDefault(p => p.Id == id);
        }

        public Player GetPlayerByUsername(string username)
        {
            username = username.Trim();

            return _dbContext.Players.FirstOrDefault(p => p.Username.ToLower().Equals(username.ToLower()));
        }

        public PlayerUnit CreatePlayerUnit(int playerId, int unitId, int level)
        {
            var playerUnit = new PlayerUnit
            {
                PlayerId = playerId,
                UnitId = unitId,
                Level = level
            };

            _dbContext.PlayerUnits.Add(playerUnit);
            _dbContext.SaveChanges();

            return playerUnit;
        }

        public PlayerUnit GetPlayerUnitById(int id)
        {
            return _dbContext.PlayerUnits.FirstOrDefault(p => p.Id == id);
        }

        public List<PlayerUnit> GetPlayerUnitsByPlayerId(int playerId)
        {
            return _dbContext.PlayerUnits.Where(p => p.PlayerId == playerId).ToList();
        }

        public PlayerSession CreatePlayerSession(string username)
        {
            var player = GetPlayerByUsername(username);

            if (player == null)
            {
                throw new EntityNotFoundException();
            }

            var activeSessions = _dbContext.PlayerSessions.Where(p => p.PlayerId == player.Id).ToList();

            foreach (var activeSession in activeSessions)
            {
                _dbContext.Remove(activeSession);
            }

            var session = new PlayerSession
            {
                Id = Guid.NewGuid().ToString(),
                PlayerId = player.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            _dbContext.PlayerSessions.Add(session);
            _dbContext.SaveChanges();

            return session;
        }

        public PlayerSession GetPlayerSessionById(string id)
        {
            return _dbContext.PlayerSessions.FirstOrDefault(s => s.Id.Equals(id));
        }
    }
}
