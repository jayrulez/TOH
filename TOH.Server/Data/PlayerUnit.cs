using System.Collections.Generic;
using TOH.Common.Data;
using TOH.Common.Services;

namespace TOH.Server.Data
{
    public class PlayerUnit
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int UnitId { get; set; }
        public int Level { get; set; }

        public virtual Player Player { get; set; }
    }

    public static partial class Extensions
    {
        public static PlayerUnitData ToDataModel(this PlayerUnit source)
        {
            var destination = new PlayerUnitData
            {
                Id = source.Id,
                PlayerId = source.PlayerId,
                UnitId = source.UnitId,
                Level = source.Level
            };

            return destination;
        }

        public static List<PlayerUnitData> ToDataModel(this List<PlayerUnit> source)
        {
            var destination = new List<PlayerUnitData>();

            foreach (var item in source)
            {
                destination.Add(item.ToDataModel());
            }

            return destination;
        }


        public static PlayerUnitModel ToModel(this PlayerUnit source)
        {
            var destination = new PlayerUnitModel(source.Id, source.Level, ConfigManager.Instance.GetUnit(source.UnitId));

            return destination;
        }
    }
}
