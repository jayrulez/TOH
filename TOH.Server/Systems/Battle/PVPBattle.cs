using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network.Abstractions;
using TOH.Network.Packets;
using TOH.Server.Systems.Battle;

namespace TOH.Server.Systems
{
    public class PVPBattle
    {
        public enum PVPBattleState
        {
            Countdown,
            SelectUnits,
            Ready,
            Combat,
            Ended,
            Dispose
        };

        public enum PVPCombatState
        {
            None,
            TurnStarted,
            TurnWait,
            TurnEnded,
        }

        private const int BattleTeamSize = 3;

        private const long SetUnitsTimeout = 1000 * 5; // seconds
        private const long TurnTimeout = 1000 * 10; // seconds

        private const long CountdownStateTimeout = 1000 * 10; // seconds

        public Stopwatch CountdownStateTimer = new Stopwatch();


        private readonly ILogger _logger;

        public string Id { get; private set; }

        public PVPBattleState State { get; private set; }
        public PVPCombatState CurrentCombatState { get; private set; }

        public List<ServerBattlePlayer> Players { get; private set; } = new List<ServerBattlePlayer>();
        public List<ServerBattleUnit> Units { get; private set; } = new List<ServerBattleUnit>();

        private readonly IServiceProvider _serviceProvider;

        public PVPBattle(IServiceScopeFactory serviceScopeFactory, List<ServerBattlePlayer> players)
        {
            _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;

            _logger = _serviceProvider.GetRequiredService<ILogger<PVPBattle>>();

            Id = Guid.NewGuid().ToString();

            Players = players;

            State = PVPBattleState.Countdown;
        }

        private void Broadcast<T>(T packet) where T : Packet
        {
            foreach (var player in Players)
            {
                player.Session.Connection.Send(packet);
            }
        }

        public void SetUnits(string sessionId, List<int> unitIds)
        {
            var player = Players.FirstOrDefault(p => p.Session.SessionId.Equals(sessionId));

            if (player != null)
            {
                foreach (var unitId in unitIds)
                {
                    var unit = player.Player.Units.FirstOrDefault(u => u.Id == unitId);

                    if (unit != null)
                    {
                        var serverBattleUnit = new ServerBattleUnit(unit);
                        player.Units.Add(serverBattleUnit);

                        Units.Add(serverBattleUnit);
                    }
                    else
                    {
                        _logger.LogInformation($"No unit with Id='{unitId}' was found in the data store.");
                    }
                }

                _logger.LogInformation($"Player '{player.Session.PlayerId}' team set. ");
            }
        }

        public void SetTurnCommand(int unitId, int skillId, List<int> targetUnitId)
        {
            var nextBattleUnit = GetCurrentBattleUnit();

            if (nextBattleUnit.PlayerUnit.Id == unitId)
            {
                nextBattleUnit.SetTurnCommand(skillId, targetUnitId);
            }
        }

        private void PushTurnInfoPacket(BattleTurnInfoPacket packet)
        {
            Broadcast(packet);
        }
        private void PushUnitTurnPacket(ServerBattleUnit unit)
        {
            Broadcast(new BattleUnitTurnPacket()
            {
                UnitId = unit.PlayerUnit.Id
            });
        }

        #region Battle States
        private void SelectUnitsState()
        {
            if (Players.All(p => p.Units.Count == BattleTeamSize))
            {
                _logger.LogInformation($"All players have selected their units.");

                State = PVPBattleState.Ready;
            }
            else
            {
                foreach (var player in Players)
                {
                    if (player.Units.Count < BattleTeamSize)
                    {
                        if (player.SelectUnitsTimer.ElapsedMilliseconds >= SetUnitsTimeout)
                        {
                            _logger.LogInformation($"Set units timer for player '{player.Session.PlayerId}' has exceeded '{SetUnitsTimeout}' seconds.");

                            //TODO: Auto-set team and go to BattleReadyState

                            var unitIds = player.Player.Units.Select(u => u.Id).Take(BattleTeamSize).ToList();

                            if (unitIds.Count < BattleTeamSize)
                            {
                                // TODO: cannot continue battle because player has less then required number of units
                            }

                            SetUnits(player.Session.SessionId, unitIds);
                        }
                    }
                }
            }
        }

        private void CountdownState()
        {
            if (!CountdownStateTimer.IsRunning)
            {
                CountdownStateTimer.Start();

                return;
            }

            if (CountdownStateTimer.ElapsedMilliseconds < CountdownStateTimeout)
            {
                var countdownPacket = new BattleCountdownPacket
                {
                    Count = (int)(CountdownStateTimeout - CountdownStateTimer.ElapsedMilliseconds) / 1000
                };

                Broadcast(countdownPacket);

                //_logger.LogInformation($"Countdown: {countdownPacket.Count}");

                return;
            }
            else
            {
                CountdownStateTimer.Stop();
            }

            State = PVPBattleState.SelectUnits;

            // inform the client that unit selection state is ready
            Broadcast(new BattleUnitSelectionReadyPacket());

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
                Players = new List<BattlePlayerModel>()
            };

            foreach (var player in Players)
            {
                var battlePlayer = new BattlePlayerModel()
                {
                    Id = player.Session.PlayerId,
                    Units = player.Units.Select(u =>
                    {
                        return new BattleUnitModel(u.PlayerUnit);
                    }).ToList()
                };

                battleReadyPacket.Players.Add(battlePlayer);
            }

            Broadcast(battleReadyPacket);

            State = PVPBattleState.Combat;
            CurrentCombatState = PVPCombatState.None;
        }

        private void BattleCombatState()
        {
            foreach (var player in Players)
            {
                if (player.Units.All(u => u.State == BattleUnitState.Dead))
                {
                    State = PVPBattleState.Ended;
                    return;
                }
            }

            if (CurrentCombatState == PVPCombatState.None)
            {
                foreach (var battleUnit in Units)
                {
                    battleUnit.OnBattleStart();
                }

                // Start the turn for the *fastest* unit
                StartTurn();
            }
            else if (CurrentCombatState == PVPCombatState.TurnWait)
            {
                TurnWait();
            }
            else if (CurrentCombatState == PVPCombatState.TurnEnded)
            {
                // Whenever a unit turn ends, start a turn for the next unit with the highest turnbar
                StartTurn();
            }
        }

        private void BattleEndedState()
        {
            foreach (var player in Players)
            {
                if (player.Units.All(u => u.State == BattleUnitState.Dead))
                {
                    player.Session.Connection.Send(new BattleResultPacket
                    {
                        BattleId = Id,
                        Status = BattleResultStatus.Lose
                    });
                }
                else
                {
                    player.Session.Connection.Send(new BattleResultPacket
                    {
                        BattleId = Id,
                        Status = BattleResultStatus.Win
                    });
                }
            }

            State = PVPBattleState.Dispose;
        }

        #endregion


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
            // Advance turnbar of all units at the start of each turn
            AdvanceTurnbars();

            var currentBattleUnit = GetCurrentBattleUnit();

            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.PlayerUnit.Id == currentBattleUnit.PlayerUnit.Id));


            currentBattleUnit.OnTurnStart();

            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' has started a turn.");

            // Go to the turn wait state to wait for a turn command
            CurrentCombatState = PVPCombatState.TurnWait;

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

            CurrentCombatState = PVPCombatState.TurnEnded;
        }

        private void AdvanceTurnbars()
        {
            foreach (var battleUnit in Units)
            {
                battleUnit.AdvanceTurnBar();
            }
        }


        private List<ServerBattleUnit> GetAllEnemies(ServerBattleUnit nextBattleUnit)
        {
            var unitPlayer = Players.FirstOrDefault(p => p.Units.Any(u => u.PlayerUnit.Id == nextBattleUnit.PlayerUnit.Id));

            var battleUnits = Units.Where(u => !unitPlayer.Units.Any(p => p.PlayerUnit.Id == u.PlayerUnit.Id)).ToList();

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

            return all.FirstOrDefault(e => e.State == BattleUnitState.Alive);
        }

        private ServerBattleUnit GetRandomAlly(ServerBattleUnit nextBattleUnit)
        {
            var all = GetAllAllies(nextBattleUnit);

            return all.FirstOrDefault();
        }

        private void PerformHealSkillAction(ServerBattleUnit currentBattleUnit, HealSkillAction skillAction, ref List<ServerBattleUnit> targets)
        {

        }

        private void PerformDamageSkillAction(ServerBattleUnit currentBattleUnit, DamageSkillAction skillAction, ref List<ServerBattleUnit> targets)
        {
            foreach (var target in targets)
            {
                if (target.State == BattleUnitState.Alive)
                {
                    var attack = currentBattleUnit.CurrentAttack;
                    var multiplier = (float)skillAction.DamageAmount / 100.0f;
                    var damage = attack * multiplier;
                    damage -= target.CurrentDefense / 2;

                    target.TakeDamage((int)damage);
                }
            }
        }

        private void PerformSkillAction(ServerBattleUnit currentBattleUnit, SkillAction skillAction, ref List<ServerBattleUnit> targets)
        {
            var actionTargets = new List<ServerBattleUnit>();

            switch (skillAction.Target)
            {
                case SkillActionTarget.Caster:
                    actionTargets = new List<ServerBattleUnit>() { currentBattleUnit };
                    break;
                case SkillActionTarget.AllEnemies:
                    actionTargets = GetAllEnemies(currentBattleUnit);
                    break;
                case SkillActionTarget.AllAllies:
                    actionTargets = GetAllAllies(currentBattleUnit);
                    break;
                case SkillActionTarget.RandomEnemy:
                    actionTargets = new List<ServerBattleUnit>() { GetRandomEnemy(currentBattleUnit) };
                    break;
                case SkillActionTarget.RandomAlly:
                    actionTargets = new List<ServerBattleUnit>() { GetRandomAlly(currentBattleUnit) };
                    break;
                case SkillActionTarget.SkillTarget:
                default:
                    actionTargets = targets;
                    break;
            }

            switch (skillAction.Type)
            {
                case SkillActionType.Damage:
                    var damageSkillAction = skillAction as DamageSkillAction;
                    if (damageSkillAction != null)
                    {
                        PerformDamageSkillAction(currentBattleUnit, damageSkillAction, ref actionTargets);
                    }
                    break;
                case SkillActionType.Heal:
                    var healSkillAction = skillAction as HealSkillAction;
                    if (healSkillAction != null)
                    {
                        PerformHealSkillAction(currentBattleUnit, healSkillAction, ref actionTargets);
                    }
                    break;
            }
        }

        private void PerformTurnCommand(ServerBattleUnit currentBattleUnit, SkillModel skill, List<ServerBattleUnit> targets)
        {
            //TODO: execute skill logic here
            foreach (var action in skill.Actions)
            {
                if (targets.Any(t => t.State == BattleUnitState.Alive))
                    PerformSkillAction(currentBattleUnit, action, ref targets);
            }


            //TODO: Push packet so client can update UI


            _logger.LogInformation($"Unit '{currentBattleUnit.PlayerUnit.Unit.Name}' performed skill '{skill.Name}' on targets '{string.Join(", ", targets.Select(t => t.PlayerUnit.Unit.Name).ToList())}'.");

            var battleTurnInfoPacket = new BattleTurnInfoPacket()
            {
                Unit = currentBattleUnit,
                SkillId = skill.Id,
                Targets = targets.Select(t => (BattleUnitModel)t).ToList()
            };

            PushTurnInfoPacket(battleTurnInfoPacket);

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

        #endregion

        public Task Tick()
        {
            switch (State)
            {
                case PVPBattleState.Countdown:
                    CountdownState();
                    break;

                case PVPBattleState.SelectUnits:
                    SelectUnitsState();
                    break;

                case PVPBattleState.Ready:
                    BattleReadyState();
                    break;

                case PVPBattleState.Combat:
                    BattleCombatState();
                    break;

                case PVPBattleState.Ended:
                    BattleEndedState();
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
