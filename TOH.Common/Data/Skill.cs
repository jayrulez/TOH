using System.Collections.Generic;

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
        Heal
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

    public class SkillAction
    {
        public SkillActionType ActionType { get; protected set; }
        public SkillActionTarget Target { get; set; }
        public int Cooldown { get; set; }
    }

    public class DamageSkillAction : SkillAction
    {
        public DamageSkillAction()
        {
            ActionType = SkillActionType.Damage;
        }

        public int DamageAmount { get; set; }
        public SkillActionScaleType ScaleType { get; set; }
        public int Accuracy { get; set; }
    }

    public class HealSkillAction : SkillAction
    {
        public HealSkillAction()
        {
            ActionType = SkillActionType.Heal;
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
        public List<SkillAction> Actions { get; set; }
    }
}
