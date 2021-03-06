﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TOH.Network.Packets;
using TOH.Server.Data;
using TOH.Server.Services;
using TOH.Server.Systems.Battle;

namespace TOH.Server.Systems
{
    public class PVPBattleSystemService
    {
        private readonly ILogger _logger;
        private readonly SessionService _sessionService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private ConcurrentDictionary<string, PVPBattle> Battles = new ConcurrentDictionary<string, PVPBattle>();

        public PVPBattleSystemService(IServiceScopeFactory serviceScopeFactory, SessionService sessionService, ILogger<PVPBattleSystemService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _sessionService = sessionService;
        }

        public Task<PVPBattle> CreateBattle(Session session1, Session session2)
        {
            var serviceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
            var playerManager = serviceProvider.GetRequiredService<PlayerManager>();

            var battlePlayers = new List<ServerBattlePlayer>();

            var player1 = playerManager.GetPlayerById(session1.PlayerId);
            var player2 = playerManager.GetPlayerById(session2.PlayerId);

            battlePlayers.Add(new ServerBattlePlayer(session1, player1.ToModel()));
            battlePlayers.Add(new ServerBattlePlayer(session2, player2.ToModel()));

            var battle = new PVPBattle(_serviceScopeFactory, battlePlayers);

            Battles.TryAdd(battle.Id, battle);

            return Task.FromResult(battle);
        }

        public Task PushTurnCommand(BattleTurnCommandPacket packet)
        {
            if (Battles.TryGetValue(packet.BattleId, out var battle))
            {
                battle.SetTurnCommand(packet.UnitId, packet.SkillId, packet.TargetUnitId);
            }

            return Task.CompletedTask;
        }

        public Task SetUnits(string battleId, string sessionId, List<int> units)
        {
            if (Battles.TryGetValue(battleId, out var battle))
            {
                //TODO: ensure session is in battle
                battle.SetUnits(sessionId, units);
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

            Parallel.ForEach(Battles, battle =>
            {
                //TODO: Ensure match players have valid sessions, end battle if not
                //foreach(var serverBattlePlayer in battle.Value.Players)
                //{

                //}

                if (battle.Value.State == PVPBattle.PVPBattleState.Dispose)
                {
                    Battles.Remove(battle.Key, out var removedBattle);

                    _logger.LogInformation($"Removed battle '{removedBattle.Id}' with State='{removedBattle.State}'.");
                }
                else
                {
                    battle.Value.Tick();
                }
            });

            return Task.CompletedTask;
        }
    }
}
