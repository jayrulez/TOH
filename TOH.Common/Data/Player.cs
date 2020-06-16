using System;
using System.Collections.Generic;
using System.Linq;

namespace TOH.Common.Data
{
    public class Player
    {
        public int Id { get; set; }
        public List<PlayerUnit> Units { get; set; }
    }

    public class PlayerUnit
    {
        public int Id { get; private set; }
        public Unit Unit { get; private set; }

        public int Level { get; private set; }

        private Dictionary<UnitStatType, int> Stats = new Dictionary<UnitStatType, int>();

        public PlayerUnit(int id, int level, Unit unit)
        {
            Id = id;
            Unit = unit;
            Level = level;

            foreach (var statType in Enum.GetValues(typeof(UnitStatType)).Cast<UnitStatType>())
            {
                Stats.Add(statType, Unit.GetStatValue(Level, statType));
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
