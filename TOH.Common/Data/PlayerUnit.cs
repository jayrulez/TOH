using System;
using System.Collections.Generic;
using System.Linq;

namespace TOH.Common.Data
{
    public class PlayerUnit
    {
        public int Id { get; set; }
        public Unit Unit { get; set; }

        public int Level { get; set; }

        public Dictionary<UnitStatType, int> Stats { get; set; } = new Dictionary<UnitStatType, int>();

        public PlayerUnit()
        {

        }

        public PlayerUnit(int id, int level, Unit unit)
        {
            Id = id;
            Unit = unit;
            Level = level;

            foreach (var statType in Enum.GetValues(typeof(UnitStatType)).Cast<UnitStatType>())
            {
                Stats.Add(statType, Unit.GetBaseStatValue(Level, statType));
            }
        }

        public int GetStatValue(UnitStatType statType)
        {
            return Stats[statType];
        }

        public Skill GetRandomSkill()
        {
            var skillsCount = Unit.Skills.Count();

            var skills = Unit.Skills.Values.ToList();

            //TODO: use random number generator to select skill

            return skills.FirstOrDefault();
        }

        public Skill GetSkill(int skillId)
        {
            return Unit.Skills.Values.ToList().FirstOrDefault(s => s.Id == skillId);
        }
    }
}
