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
        public List<int> TargetUnitIds { get; }

        public BattleTurnCommand(int skillId, List<int> targetUnitId)
        {
            SkillId = skillId;
            TargetUnitIds = targetUnitId;
        }
    }

    public class BattleUnit
    {
        public PlayerUnit PlayerUnit { get; set; }
        public BattleUnitState State { get; set; }
        public StatModifier StatModifier { get; set; }

        public int CurrentSpeed => GetStatValue(UnitStatType.Speed);

        public Dictionary<UnitStatType, int> Stats { get; set; } = new Dictionary<UnitStatType, int>();

        public BattleUnit()
        {

        }

        public BattleUnit(PlayerUnit playerUnit)
        {
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

        public Skill GetRandomSkill()
        {
            return PlayerUnit.GetRandomSkill();
        }

        public Skill GetSkill(int skillId)
        {
            return PlayerUnit.GetSkill(skillId);
        }
    }
}
