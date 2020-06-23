using System;
using System.Collections.Generic;
using System.Linq;

namespace TOH.Common.Data
{
    public class PlayerUnitModel
    {
        public int Id { get; set; }
        public UnitModel Unit { get; set; }

        public int Level { get; set; }

        public Dictionary<UnitStatType, int> Stats { get; set; } = new Dictionary<UnitStatType, int>();

        public PlayerUnitModel()
        {

        }

        public PlayerUnitModel(int id, int level, UnitModel unit)
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

        public SkillModel GetRandomSkill()
        {
            var skillsCount = Unit.Skills.Count();

            var skills = Unit.Skills.Values.ToList();

            //TODO: use random number generator to select skill

            return skills.FirstOrDefault();
        }

        public SkillModel GetSkill(int skillId)
        {
            return Unit.Skills.Values.ToList().FirstOrDefault(s => s.Id == skillId);
        }
    }
}
