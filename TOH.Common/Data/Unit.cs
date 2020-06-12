using System;
using System.Collections.Generic;
using System.Linq;

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
        private Dictionary<UnitSkillSlot, Skill> Skills { get; set; }
        private Dictionary<int, Dictionary<UnitStatType, double>> LevelConfig { get; set; }

        public static Unit Load(string data)
        {
            var unit = new Unit();

            // Todo:
            // Set Id
            // Set Type
            // Set Grade
            // Set Element
            // Set Name
            // Set Stats
            // Set Skills
            // Set LevelConfig

            return unit;
        }

        private Unit()
        {
            Stats = new Dictionary<UnitStatType, int>();
            Skills = new Dictionary<UnitSkillSlot, Skill>();
            LevelConfig = new Dictionary<int, Dictionary<UnitStatType, double>>();

            foreach (var stat in Enum.GetValues(typeof(UnitStatType)).Cast<UnitStatType>())
            {
                Stats.Add(stat, 0);
            }
        }

        public int GetStatValue(int level, UnitStatType statType)
        {
            var statValue = Stats[statType];

            if (LevelConfig.ContainsKey(level))
            {
                var levelConfig = LevelConfig[level];

                statValue = (int)(statValue * levelConfig[statType]);
            }

            return statValue;
        }
    }
}
