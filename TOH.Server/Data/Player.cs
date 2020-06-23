using System.Collections.Generic;
using TOH.Common.Data;
using TOH.Common.Services;

namespace TOH.Server.Data
{
    public class Player
    {
        public Player()
        {
            Units = new HashSet<PlayerUnit>();
        }

        public int Id { get; set; }
        public int Level { get; set; }
        public string Username { get; set; }

        public virtual ICollection<PlayerUnit> Units { get; set; }
    }

    public static partial class Extensions
    {
        public static PlayerData ToDataModel(this Player source)
        {
            var destination = new PlayerData
            {
                Id = source.Id,
                Username = source.Username,
                Level = source.Level
            };

            return destination;
        }

        public static PlayerModel ToModel(this Player source)
        {
            var destination = new PlayerModel
            {
                Id = source.Id,
                Units = new List<PlayerUnitModel>()
            };

            foreach(var playerUnit in source.Units)
            {
                destination.Units.Add(playerUnit.ToModel());
            }

            return destination;
        }
    }
}
