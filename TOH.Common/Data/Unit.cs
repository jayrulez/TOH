using System;
using System.Collections.Generic;
using System.Linq;
using static TOH.Common.Data.DataManager;

namespace TOH.Common.Data
{
    public enum UnitType
    {
        DPS,
        Tank,
        Support
    }

    public enum UnitElement
    {
        Fire,
        Water,
        Earth,
        Light,
        Dark
    }

    public enum UnitGrade
    {
        Fodder,
        Normal,
        Rare,
        Hero,
        Legendary
    }

    public enum UnitStatType
    {
        HP,
        Attack,
        Defense,
        Speed
    }

    public enum UnitSkillSlot
    {
        Default,
        Second,
        Third,
        Leader
    }

    public class Unit
    {
        public int Id { get; private set; }
        public UnitType Type { get; private set; }
        public UnitGrade Grade { get; private set; }
        public UnitElement Element { get; private set; }
        public string Name { get; private set; }
        private Dictionary<UnitStatType, int> Stats { get; set; }
        public Dictionary<UnitSkillSlot, Skill> Skills { get; private set; }

        public static Unit Load(int id)
        {
            return Instance.Units.FirstOrDefault(u => u.Id == id);
        }

        public static Unit Create(UnitConfig unitConfig, Dictionary<UnitSkillSlot, Skill> skills)
        {
            return new Unit(unitConfig, skills);
        }

        private Unit(UnitConfig unitConfig, Dictionary<UnitSkillSlot, Skill> skills)
        {
            Id = unitConfig.Id;
            Type = unitConfig.Type;
            Grade = unitConfig.Grade;
            Element = unitConfig.Element;
            Name = unitConfig.Name;

            Stats = unitConfig.Stats;
            foreach (var stat in Enum.GetValues(typeof(UnitStatType)).Cast<UnitStatType>())
            {
                if (!Stats.ContainsKey(stat))
                    Stats.Add(stat, 0);
            }

            Skills = skills;
            foreach (var skillSlot in Enum.GetValues(typeof(UnitSkillSlot)).Cast<UnitSkillSlot>())
            {
                if (!Skills.ContainsKey(skillSlot))
                    Skills.Add(skillSlot, null);
            }
        }

        public int GetStatValue(int level, UnitStatType statType)
        {
            var statValue = Stats[statType];

            if (Instance.LevelConfig.ContainsKey(level))
            {
                var levelConfig = Instance.LevelConfig[level];

                statValue = (int)(statValue * levelConfig[statType]);
            }

            return statValue;
        }
    }
}
