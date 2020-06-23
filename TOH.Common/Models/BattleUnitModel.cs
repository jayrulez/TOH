using System;
using System.Collections.Generic;
using System.Linq;
using TOH.Common.Data;

namespace TOH.Common.Data
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
            if (Modifications.ContainsKey(statType))
                return Modifications[statType];
            else
                return new List<StatModification>();
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

    public class BattleUnitModel
    {
        public PlayerUnitModel PlayerUnit { get; set; }
        public BattleUnitState State { get; set; }
        public StatModifier StatModifier { get; set; }

        public int CurrentHP => GetStatValue(UnitStatType.HP);
        public int CurrentSpeed => GetStatValue(UnitStatType.Speed);
        public int CurrentAttack => GetStatValue(UnitStatType.Attack);
        public int CurrentDefense => GetStatValue(UnitStatType.Defense);
        public int Turnbar { get; protected set; }

        public Dictionary<UnitStatType, int> Stats { get; set; } = new Dictionary<UnitStatType, int>();

        public BattleUnitModel()
        {

        }

        public BattleUnitModel(PlayerUnitModel playerUnit)
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

            var modificationValue = 0;

            var modifications = StatModifier.GetModifications(statType);

            foreach (var modification in modifications)
            {
                modificationValue += modification.Value;
            }

            return statValue + modificationValue;
        }

        public SkillModel GetRandomSkill()
        {
            return PlayerUnit.GetRandomSkill();
        }

        public SkillModel GetSkill(int skillId)
        {
            return PlayerUnit.GetSkill(skillId);
        }

        private void DecreaseStat(UnitStatType statType, int amount)
        {
            var reduction = amount;
            var statModifications = StatModifier.GetModifications(statType);

            foreach (var statModification in statModifications)
            {
                if (statModification.Value > reduction)
                {
                    statModification.Value -= reduction;
                    reduction = 0;
                }
                else
                {
                    reduction -= statModification.Value;
                    StatModifier.RemoveModification(statType, statModification.Key);
                }
            }

            if (reduction > 0)
            {
                Stats[statType] -= reduction;
                if (Stats[statType] < 0)
                {
                    Stats[statType] = 0;
                }
            }
        }

        public void TakeDamage(int damage)
        {
            DecreaseStat(UnitStatType.HP, damage);
            var hp = GetStatValue(UnitStatType.HP);

            if(hp <= 0)
            {
                Kill();
                State = BattleUnitState.Dead;
            }
        }

        protected void Kill()
        {
            StatModifier.ClearModifications();
            Stats[UnitStatType.HP] = 0;
        }

        protected virtual void ResetTurnbar()
        {
            Turnbar = 0;
        }
    }
}
