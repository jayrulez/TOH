using System;
using System.Collections.Generic;
using System.Linq;
using static TOH.Common.Data.ConfigManager;

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

    public class UnitModel
    {
        public int UnitId { get; set; }
        public UnitType Type { get; set; }
        public UnitGrade Grade { get; set; }
        public UnitElement Element { get; set; }
        public string Name { get; set; }
        public Dictionary<UnitStatType, int> Stats { get; set; } = new Dictionary<UnitStatType, int>();
        public Dictionary<UnitSkillSlot, SkillModel> Skills { get; set; } = new Dictionary<UnitSkillSlot, SkillModel>();

        public UnitModel()
        {
            foreach (var stat in Enum.GetValues(typeof(UnitStatType)).Cast<UnitStatType>())
            {
                Stats.Add(stat, 0);
            }

            foreach (var skillSlot in Enum.GetValues(typeof(UnitSkillSlot)).Cast<UnitSkillSlot>())
            {
                Skills.Add(skillSlot, null);
            }
        }

        public int GetBaseStatValue(int level, UnitStatType statType)
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
