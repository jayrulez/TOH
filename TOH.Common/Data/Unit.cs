using System.Collections.Generic;

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

    public class UnitStats
    {
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
    }

    public class Unit
    {
        public int Id { get; set; }
        public UnitType Type { get; set; }
        public UnitGrade Grade { get; set; }
        public UnitElement Element { get; set; }
        public string Name { get; set; }
        public UnitStats BaseStats { get; set; }
        public List<Skill> Skills { get; set; }
    }
}
