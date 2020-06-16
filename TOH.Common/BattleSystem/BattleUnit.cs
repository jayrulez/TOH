using System;
using System.Collections.Generic;
using System.Linq;
using TOH.Common.Data;

namespace TOH.Common.BattleSystem
{
    public enum BattleUnitState
    {
        Alive,
        Dead
    }

    public class StatModification
    {
        public string Key { get; set; }
        public int Value { get; set; }

        public StatModification(int value)
        {
            Key = Guid.NewGuid().ToString();
            Value = value;
        }
    }

    public class StatModifier
    {
        private Dictionary<UnitStatType, List<StatModification>> Modifications = new Dictionary<UnitStatType, List<StatModification>>();

        public StatModifier()
        {
            foreach (var stat in Enum.GetValues(typeof(UnitStatType)).Cast<UnitStatType>())
            {
                Modifications.Add(stat, new List<StatModification>());
            }
        }

        public void AddModification(UnitStatType statType, StatModification modification)
        {
            if (Modifications.ContainsKey(statType))
            {
                Modifications[statType].Add(modification);
            }
            else
            {
                Modifications.Add(statType, new List<StatModification>()
                {
                    modification
                });
            }
        }

        public void RemoveModification(UnitStatType statType, string key)
        {
            if (Modifications.ContainsKey(statType))
            {
                var modifications = Modifications[statType];

                var modification = modifications.FirstOrDefault(m => m.Key.Equals(key));

                if (modification != null)
                {
                    modifications.Remove(modification);
                }
            }
        }

        public List<StatModification> GetModifications(UnitStatType statType)
        {
            return Modifications[statType];
        }

        public void ClearModifications()
        {
            Modifications.Clear();
        }
    }

    public class BattleTurnCommand
    {
        public int SkillId { get; }
        public List<string> TargetUnitIds { get; }

        public BattleTurnCommand(int skillId, List<string> targetUnitId)
        {
            SkillId = skillId;
            TargetUnitIds = targetUnitId;
        }
    }

    public class BattleUnit
    {
        public string Id { get; private set; }
        public PlayerUnit PlayerUnit { get; private set; }
        public BattleUnitState State { get; private set; }
        public StatModifier StatModifier { get; private set; }

        public BattleTurnCommand TurnCommand { get; private set; } = null;

        public int CurrentSpeed => GetStatValue(UnitStatType.Speed);

        public int Turnbar { get; private set; }

        private Dictionary<UnitStatType, int> Stats = new Dictionary<UnitStatType, int>();

        public BattleUnit(PlayerUnit playerUnit)
        {
            Id = Guid.NewGuid().ToString();
            PlayerUnit = playerUnit;
            State = BattleUnitState.Alive;
            StatModifier = new StatModifier();

            foreach (var statType in Enum.GetValues(typeof(UnitStatType)).Cast<UnitStatType>())
            {
                Stats.Add(statType, PlayerUnit.GetStatValue(statType));
            }
        }

        public int GetStatValue(UnitStatType statType)
        {
            var statValue = Stats[statType];

            var modifications = StatModifier.GetModifications(statType);

            foreach (var modification in modifications)
            {
                statValue += modification.Value;
            }

            return statValue;
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

        public Skill GetRandomSkill()
        {
            return PlayerUnit.GetRandomSkill();
        }

        public void SetTurnCommand(int skillId, List<string> targetUnitId)
        {
            var skill = PlayerUnit.Unit.Skills.FirstOrDefault(s => s.Value.Id == skillId);

            if(skill.Value.Id == skillId)
            {
                TurnCommand = new BattleTurnCommand(skill.Value.Id, targetUnitId);
            }
        }

        public Skill GetSkill(int skillId)
        {
            return PlayerUnit.GetSkill(skillId);
        }
    }
}
