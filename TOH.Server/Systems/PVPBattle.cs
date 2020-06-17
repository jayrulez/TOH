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
    public class PVPBattle
    {
        public class ServerBattleUnit : BattleUnit
        {
            public BattleUnit BattleUnit { get; private set; }
            public BattleTurnCommand TurnCommand { get; set; } = null;
            public int Turnbar { get; private set; }

            public ServerBattleUnit(BattleUnit battleUnit) : base(battleUnit.PlayerUnit)
            {
                BattleUnit = battleUnit;
            }

            private void ResetTurnbar()
            {
                Turnbar = 0;
            }

            public void OnBattleStart()
            {

            }

            public void OnTurnStart()
            {

            }

            public void OnTurnEnd()
            {
                ResetTurnbar();
                TurnCommand = null;
            }

            public void Kill()
            {

            }

            public void UpdateTurn()
            {

            }

            public void AdvanceTurnBar()
            {
                Turnbar += CurrentSpeed * 2;
            }

            public void SetTurnCommand(int skillId, List<int> targetUnitId)
            {
                var skill = PlayerUnit.Unit.Skills.FirstOrDefault(s => s.Value.Id == skillId);

                if (skill.Value.Id == skillId)
                {
                    TurnCommand = new BattleTurnCommand(skill.Value.Id, targetUnitId);
                }
            }
        }

        public class ServerBattlePlayer
        {
            public IConnection Connection { get; private set; }
            public List<ServerBattleUnit> Units { get; set; } = new List<ServerBattleUnit>();

            public Stopwatch SelectUnitsTimer = new Stopwatch();

            public Stopwatch TurnTimer = new Stopwatch();

            public ServerBattlePlayer(IConnection connection)
            {
                Connection = connection;
            }
        }

        public enum BattleState
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

        private const long SetUnitsTimeout = 1000 * 5; // seconds
        private const long TurnTimeout = 1000 * 10; // seconds

        public void SetTurnCommand(int unitId, int skillId, List<int> targetUnitId)
        {
            var nextBattleUnit = GetCurrentBattleUnit();

            if (nextBattleUnit.PlayerUnit.Id == unitId)
            {
                nextBattleUnit.SetTurnCommand(skillId, targetUnitId);
            }
        }

        private const int BattleTeamSize = 3;
        private int DummyPlayerId = 0;

        private readonly ILogger _logger;

        public string Id { get; private set; }

        public BattleState State { get; private set; }
        public CombatState CurrentCombatState { get; private set; }

        public List<ServerBattlePlayer> Players { get; private set; } = new List<ServerBattlePlayer>();
        public List<ServerBattleUnit> Units { get; private set; } = new List<ServerBattleUnit>();

        public PVPBattle(IConnection connection1, IConnection connection2, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PVPBattle>();
            Id = Guid.NewGuid().ToString();

            Players.Add(new ServerBattlePlayer(connection1));
            Players.Add(new ServerBattlePlayer(connection2));

            State = BattleState.None;
        }

        public void SetUnits(string connectionId, List<int> unitIds)
        {
            var player = Players.FirstOrDefault(p => p.Connection.Id.Equals(connectionId));

            if (player != null)
            {
                foreach (var unitId in unitIds)
                {
                    var unit = Unit.Load(unitId);

                    if (unit != null)
                    {
                        var playerUnit = new PlayerUnit(++DummyPlayerId, 10, unit);
                        var battleUnit = new BattleUnit(playerUnit);
                        var serverBattleUnit = new ServerBattleUnit(battleUnit);
                        player.Units.Add(serverBattleUnit);

                        Units.Add(serverBattleUnit);
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

        private void SelectUnitsState()
        {
            if (Players.All(p => p.Units.Count == BattleTeamSize))
            {
                _logger.LogInformation($"All players have selected their units.");

                State = BattleState.Ready;
            }
            else
            {
                foreach (var player in Players)
                {
                    if (player.Units.Count < BattleTeamSize)
                    {
                        if (player.SelectUnitsTimer.ElapsedMilliseconds >= SetUnitsTimeout)
                        {
                            _logger.LogInformation($"Set units timer for player '{player.Connection.Id}' has exceeded '{SetUnitsTimeout}' seconds.");

                            //TODO: Auto-set team and go to BattleReadyState
                            SetUnits(player.Connection.Id, new List<int> { 4, 5, 6 });
                        }
                    }
                }
            }
        }

        private void NoneState()
        {
            State = BattleState.SelectUnits;

            foreach (var player in Players)
            {
                player.SelectUnitsTimer.Start();
            }

            _logger.LogInformation($"Waiting for players to select their units.");
        }

        public void BattleReadyState()
        {
            var battleReadyPacket = new BattleReadyPacket
            {
                BattleId = Id,
                Players = new List<BattlePlayer>()
            };

            foreach (var player in Players)
            {
                var battlePlayer = new BattlePlayer()
                {
                    Id = player.Connection.Id,// TODO: We will need the actual player id at some point instead of the connection id
                    Units = player.Units.Select(u =>
                    {
                        return new BattleUnit(u.PlayerUnit);
                    }).ToList()
                };

                battleReadyPacket.Players.Add(battlePlayer);
            }

            Broadcast(battleReadyPacket);

            State = BattleState.Combat;
            CurrentCombatState = CombatState.None;
        }

        private void PushTurnInfoPacket(BattleTurnInfoPacket packet)
        {
            Broadcast(packet);
        }

        private void BattleCombatState()
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

        private List<ServerBattleUnit> GetAllEnemies(ServerBattleUnit nextBattleUnit)
        {
            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.BattleUnit.PlayerUnit.Id == nextBattleUnit.PlayerUnit.Id));

            var battleUnits = Units.Where(u => !unitPlayer.Units.Any(p => p.BattleUnit.PlayerUnit.Id == u.BattleUnit.PlayerUnit.Id)).ToList();

            return battleUnits;
        }

        private List<ServerBattleUnit> GetAllAllies(ServerBattleUnit nextBattleUnit)
        {
            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.PlayerUnit.Id == nextBattleUnit.PlayerUnit.Id));

            var battleUnits = Units.Where(u => unitPlayer.Units.Any(p => p.PlayerUnit.Id == u.PlayerUnit.Id)).ToList();

            return battleUnits;
        }

        private ServerBattleUnit GetRandomEnemy(ServerBattleUnit nextBattleUnit)
        {
            var all = GetAllEnemies(nextBattleUnit);

            return all.FirstOrDefault();
        }

        private ServerBattleUnit GetRandomAlly(ServerBattleUnit nextBattleUnit)
        {
            var all = GetAllAllies(nextBattleUnit);

            return all.FirstOrDefault();
        }

        private void PerformTurnCommand(ServerBattleUnit currentBattleUnit, Skill skill, List<ServerBattleUnit> targets)
        {
            //TODO: execute skill logic here

            //TODO: Push packet so client can update UI

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' performed skill '{skill.Name}' on targets '{string.Join(", ", targets.Select(t => t.PlayerUnit.Unit.Name).ToList())}'.");

            PushTurnInfoPacket(new BattleTurnInfoPacket()
            {
                Unit = currentBattleUnit.BattleUnit,
                SkillId = skill.Id,
                Targets = targets.Select(t => t.BattleUnit).ToList()
            });

            // Ends the turn after performing the turn command
            EndTurn();
        }

        private List<ServerBattleUnit> GetBattleUnits(List<int> targetUnitIds)
        {
            var battleUnits = new List<ServerBattleUnit>();

            foreach (var battleUnit in Units)
            {
                if (targetUnitIds.Contains(battleUnit.PlayerUnit.Id))
                {
                    battleUnits.Add(battleUnit);
                }
            }

            return battleUnits;
        }

        private void PushUnitTurnPacket(ServerBattleUnit unit)
        {
            Broadcast(new BattleUnitTurnPacket()
            {
                UnitId = unit.PlayerUnit.Id
            });
        }

        #region Battle logic

        private int MaxTurnbar { get { return GetCurrentBattleUnit().Turnbar; } }

        /// <summary>
        /// Gets the ServerBattleUnit that should take a turn based on the turnbars
        /// </summary>
        /// <returns></returns>
        private ServerBattleUnit GetCurrentBattleUnit()
        {
            var unit = Units.Where(u => u.State != BattleUnitState.Dead).OrderByDescending(u => u.Turnbar).FirstOrDefault();

            return unit;
        }

        private void StartTurn()
        {
            var currentBattleUnit = GetCurrentBattleUnit();

            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.PlayerUnit.Id == currentBattleUnit.PlayerUnit.Id));


            currentBattleUnit.OnTurnStart();

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' has started a turn.");

            // Go to the turn wait state to wait for a turn command
            CurrentCombatState = CombatState.TurnWait;

            PushUnitTurnPacket(currentBattleUnit);

            // resets the turn timer at the start of a turn
            unitPlayer.TurnTimer.Restart();

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' is waiting on a TurnCommand.");
        }

        private void TurnWait()
        {
            var currentBattleUnit = GetCurrentBattleUnit();

            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.PlayerUnit.Id == currentBattleUnit.PlayerUnit.Id));

            if (unitPlayer.TurnTimer.ElapsedMilliseconds >= TurnTimeout)
            {
                // auto-skill
                var skill = currentBattleUnit.GetRandomSkill();

                List<ServerBattleUnit> targets = null;

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
                        targets = new List<ServerBattleUnit> { GetRandomAlly(currentBattleUnit) };
                        break;
                    case SkillTarget.SingleEnemy:
                        targets = new List<ServerBattleUnit> { GetRandomEnemy(currentBattleUnit) };
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
                case BattleState.None:
                    NoneState();
                    break;

                case BattleState.SelectUnits:
                    SelectUnitsState();
                    break;

                case BattleState.Ready:
                    BattleReadyState();
                    break;

                case BattleState.Combat:
                    BattleCombatState();
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
