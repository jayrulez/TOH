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

    public class LeaderSkillAction : SkillAction
    {
        public LeaderSkillAction()
        {
            Type = SkillActionType.Leader;
        }

        public LeaderSkillArea Area { get; set; }
        public UnitStatType StatType { get; set; }
        public int StatIncreaseAmount { get; set; }
    }

    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillTarget Target { get; set; }
        public int Cooldown { get; set; }
        public List<SkillAction> Actions { get; set; }
    }
}
