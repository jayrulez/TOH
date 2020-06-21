using System.Collections.Generic;
using System.Linq;
using static TOH.Common.Data.DataManager;

namespace TOH.Common.Data
{
    public enum SkillTarget
    {
        SingleEnemy,
        SingleAlly,
        AllEnemies,
        AllAllies
    }

    public enum SkillActionTarget
    {
        SkillTarget,
        Caster,
        RandomAlly,
        RandomEnemy,
        AllAllies,
        AllEnemies
    }

    public enum SkillActionType
    {
        Damage,
        Heal,
        Leader
    }

    public enum SkillActionScaleType
    {
        CasterHP,
        CasterAttack,
        CasterDefense,
        CasterSpeed,
        TargetHP,
        TargetAttack,
        TargetDefense,
        TargetSpeed
    }

    public enum LeaderSkillArea
    {
        Arena,
        Scenario,
        Dungeon
    }

    public class SkillAction
    {
        public SkillActionType Type { get; protected set; }
        public SkillActionTarget Target { get; set; }
    }

    public class DamageSkillAction : SkillAction
    {
        public DamageSkillAction()
        {
            Type = SkillActionType.Damage;
        }

        public int DamageAmount { get; set; }
        public SkillActionScaleType ScaleType { get; set; }
        public int Accuracy { get; set; }
    }

    public class HealSkillAction : SkillAction
    {
        public HealSkillAction()
        {
            Type = SkillActionType.Heal;
        }

        public int HealAmount { get; set; }
        public SkillActionScaleType ScaleType { get; set; }
    }

    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillTarget Target { get; set; }
        public int Cooldown { get; set; }
        public List<SkillAction> Actions { get; set; } = new List<SkillAction>();

        public static Skill Load(int id)
        {
            return Instance.Skills.FirstOrDefault(s => s.Id == id);
        }

        public static Skill Create(SkillConfig skillConfig, List<SkillAction> skillActions)
        {
            return new Skill(skillConfig, skillActions);
        }

        public Skill()
        {

        }

        private Skill(SkillConfig skillConfig, List<SkillAction> skillActions)
        {
            Id = skillConfig.Id;
            Name = skillConfig.Name;
            Description = skillConfig.Description;
            Target = skillConfig.Target;
            Cooldown = skillConfig.Cooldown;
            Actions = skillActions;
        }
    }
}
