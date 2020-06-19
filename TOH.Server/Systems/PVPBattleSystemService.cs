using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TOH.Network.Packets;
using TOH.Server.Services;

namespace TOH.Server.Systems
{
    public class PVPBattleSystemService
    {
        private readonly ILogger _logger;
        private readonly SessionService _sessionService;

        private ConcurrentDictionary<string, PVPBattle> Battles = new ConcurrentDictionary<string, PVPBattle>();

        public PVPBattleSystemService(SessionService sessionService, ILogger<PVPBattleSystemService> logger)
        {
            _logger = logger;
            _sessionService = sessionService;
        }

        public Task<PVPBattle> CreateBattle(ActiveSession session1, ActiveSession session2)
        {
            var battle = new PVPBattle(session1, session2, _logger);

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

            Parallel.ForEach(Battles, match =>
            {
                match.Value.Tick();
            });

            return Task.CompletedTask;
        }
    }
}
