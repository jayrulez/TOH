using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TOH.Common.BattleSystem;
using TOH.Common.Data;
using TOH.Network.Abstractions;
using TOH.Network.Packets;

namespace TOH.Server.Systems
{
    public class Match
    {
        public class MatchPlayer
        {
            public IConnection Connection { get; private set; }
            public List<BattleUnit> Units { get; set; } = new List<BattleUnit>();

            public Stopwatch SelectUnitsTimer = new Stopwatch();

            public Stopwatch TurnTimer = new Stopwatch();

            public MatchPlayer(IConnection connection)
            {
                Connection = connection;
            }
        }

        public enum MatchState
        {
            None,
            SelectUnits,
            Ready,
            Combat,
            Ended
        };

        public enum CombatState
        {
            None,
            TurnStarted,
            TurnWait,
            TurnEnded,
        }

        private const long SetUnitsTimeout = 1000 * 30; // seconds
        private const long TurnTimeout = 1000 * 10; // seconds

        public void SetTurnCommand(string unitId, int skillId, List<string> targetUnitId)
        {
            var nextBattleUnit = GetCurrentBattleUnit();

            if (nextBattleUnit.Id.Equals(unitId))
            {
                nextBattleUnit.SetTurnCommand(skillId, targetUnitId);
            }
        }

        private const int MatchTeamSize = 3;

        private readonly ILogger _logger;

        public string Id { get; private set; }

        public MatchState State { get; private set; }
        public CombatState CurrentCombatState { get; private set; }

        public List<MatchPlayer> Players { get; private set; } = new List<MatchPlayer>();
        public List<BattleUnit> Units { get; private set; } = new List<BattleUnit>();

        public Match(IConnection connection1, IConnection connection2, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Match>();
            Id = Guid.NewGuid().ToString();

            Players.Add(new MatchPlayer(connection1));
            Players.Add(new MatchPlayer(connection2));

            State = MatchState.None;
            CurrentCombatState = CombatState.None;
        }

        public void SetTeam(string connectionId, List<int> unitIds)
        {
            var player = Players.FirstOrDefault(p => p.Connection.Id.Equals(connectionId));

            if (player != null)
            {
                foreach (var unitId in unitIds)
                {
                    var unit = Unit.Load(unitId);

                    if (unit != null)
                    {
                        var playerUnit = new PlayerUnit(1, 10, unit);
                        var battleUnit = new BattleUnit(playerUnit);
                        player.Units.Add(battleUnit);

                        Units.Add(battleUnit);
                    }
                    else
                    {
                        _logger.LogInformation($"No unit with Id='{unitId}' was found in the data store.");
                    }
                }

                _logger.LogInformation($"Player '{player.Connection.Id}' team set. ");
            }
        }

        private void Broadcast<T>(T packet) where T : Packet
        {
            foreach (var player in Players)
            {
                player.Connection.Send(packet);
            }
        }

        private void SetTeamsState()
        {
            if (Players.All(p => p.Units.Count == MatchTeamSize))
            {
                _logger.LogInformation($"All players have selected their units.");

                State = MatchState.Ready;
            }
            else
            {
                foreach (var player in Players)
                {
                    if (player.Units.Count < MatchTeamSize)
                    {
                        if (player.SelectUnitsTimer.ElapsedMilliseconds >= SetUnitsTimeout)
                        {
                            _logger.LogInformation($"Set units timer for player '{player.Connection.Id}' has exceeded '{SetUnitsTimeout}' seconds.");

                            //TODO: Auto-set team and go to MatchReadyState
                            //
                            SetTeam(player.Connection.Id, new List<int> { 4, 5, 6 });
                        }
                    }
                }
            }
        }

        private void NoneState()
        {
            State = MatchState.SelectUnits;

            foreach (var player in Players)
            {
                player.SelectUnitsTimer.Start();
            }

            _logger.LogInformation($"Waiting for players to select their units.");
        }

        public void MatchReadyState()
        {
            Broadcast(new MatchReadyPacket { MatchId = Id });

            State = MatchState.Combat;
        }

        private void PushMatchStatePacket()
        {
            Broadcast(new MatchStatePacket()
            {

            });
        }

        private void MatchCombatState()
        {
            if (CurrentCombatState == CombatState.None)
            {
                foreach (var battleUnit in Units)
                {
                    battleUnit.OnBattleStart();
                }

                // Advance turnbar of all units at the start of the battle
                AdvanceTurnbars();

                // Start the turn for the *fastest* unit
                StartTurn();
            }

            if (CurrentCombatState == CombatState.TurnWait)
            {
                TurnWait();
            }

            // Whenever a unit turn ends, start a turn for the next unit with the highest turnbar
            if (CurrentCombatState == CombatState.TurnEnded)
            {
                StartTurn();
            }
        }

        private List<BattleUnit> GetAllEnemies(BattleUnit nextBattleUnit)
        {
            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.Id.Equals(nextBattleUnit.Id)));

            var battleUnits = Units.Where(u => !unitPlayer.Units.Any(p => p.Id.Equals(u.Id))).ToList();

            return battleUnits;
        }

        private List<BattleUnit> GetAllAllies(BattleUnit nextBattleUnit)
        {
            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.Id.Equals(nextBattleUnit.Id)));

            var battleUnits = Units.Where(u => unitPlayer.Units.Any(p => p.Id.Equals(u.Id))).ToList();

            return battleUnits;
        }

        private BattleUnit GetRandomEnemy(BattleUnit nextBattleUnit)
        {
            var all = GetAllEnemies(nextBattleUnit);

            return all.FirstOrDefault();
        }

        private BattleUnit GetRandomAlly(BattleUnit nextBattleUnit)
        {
            var all = GetAllAllies(nextBattleUnit);

            return all.FirstOrDefault();
        }

        private void PerformTurnCommand(BattleUnit currentBattleUnit, Skill skill, List<BattleUnit> targets)
        {
            //TODO: execute skill logic here

            //TODO: Push packet so client can update UI

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' performed skill '{skill.Name}' on targets '{string.Join(", ", targets.Select(t => t.PlayerUnit.Unit.Name).ToList())}'.");

            // Ends the turn after performing the turn command
            EndTurn();
        }

        private List<BattleUnit> GetBattleUnits(List<string> targetUnitIds)
        {
            var battleUnits = new List<BattleUnit>();

            foreach (var battleUnit in Units)
            {
                if (targetUnitIds.Contains(battleUnit.Id))
                {
                    battleUnits.Add(battleUnit);
                }
            }

            return battleUnits;
        }

        #region Battle logic

        private int MaxTurnbar { get { return GetCurrentBattleUnit().Turnbar; } }

        /// <summary>
        /// Gets the BattleUnit that should take a turn based on the turnbars
        /// </summary>
        /// <returns></returns>
        private BattleUnit GetCurrentBattleUnit()
        {
            var unit = Units.Where(u => u.State != BattleUnitState.Dead).OrderByDescending(u => u.Turnbar).FirstOrDefault();

            return unit;
        }

        private void StartTurn()
        {
            // Update clients with match state
            PushMatchStatePacket();

            var currentBattleUnit = GetCurrentBattleUnit();

            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.Id.Equals(currentBattleUnit.Id)));

            // resets the turn timer at the start of a turn
            unitPlayer.TurnTimer.Restart();

            currentBattleUnit.OnTurnStart();

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' has started a turn.");

            // Go to the turn wait state to wait for a turn command
            CurrentCombatState = CombatState.TurnWait;

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' is waiting on a TurnCommand.");
        }

        private void TurnWait()
        {
            var currentBattleUnit = GetCurrentBattleUnit();

            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.Id.Equals(currentBattleUnit.Id)));

            if (unitPlayer.TurnTimer.ElapsedMilliseconds >= TurnTimeout)
            {
                // auto-skill
                var skill = currentBattleUnit.GetRandomSkill();

                List<BattleUnit> targets = null;

                // auto-targeting
                switch (skill.Target)
                {
                    case SkillTarget.AllAllies:
                        targets = GetAllAllies(currentBattleUnit);
                        break;
                    case SkillTarget.AllEnemies:
                        targets = GetAllEnemies(currentBattleUnit);
                        break;
                    case SkillTarget.SingleAlly:
                        targets = new List<BattleUnit> { GetRandomAlly(currentBattleUnit) };
                        break;
                    case SkillTarget.SingleEnemy:
                        targets = new List<BattleUnit> { GetRandomEnemy(currentBattleUnit) };
                        break;
                }

                PerformTurnCommand(currentBattleUnit, skill, targets);

                _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' performed an auto TurnCommand.");
            }
            else
            {
                if (currentBattleUnit.TurnCommand != null)
                {
                    var skill = currentBattleUnit.GetSkill(currentBattleUnit.TurnCommand.SkillId);
                    var targets = GetBattleUnits(currentBattleUnit.TurnCommand.TargetUnitIds);

                    PerformTurnCommand(currentBattleUnit, skill, targets);

                    _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' performed a TurnCommand.");

                }
            }
        }

        private void EndTurn()
        {
            var currentBattleUnit = GetCurrentBattleUnit();

            currentBattleUnit.OnTurnEnd();

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' has ended a turn.");

            CurrentCombatState = CombatState.TurnEnded;

            // Advance the turnbar of all units after a unit ends a turn
            AdvanceTurnbars();

            PushMatchStatePacket();
        }

        private void AdvanceTurnbars()
        {
            foreach (var battleUnit in Units)
            {
                battleUnit.AdvanceTurnBar();
            }
        }

        #endregion

        public Task Tick()
        {
            switch (State)
            {
                case MatchState.None:
                    NoneState();
                    break;

                case MatchState.SelectUnits:
                    SetTeamsState();
                    break;

                case MatchState.Ready:
                    MatchReadyState();
                    break;

                case MatchState.Combat:
                    MatchCombatState();
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
