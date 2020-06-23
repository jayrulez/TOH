using System.Collections.Generic;

namespace TOH.Common.Data
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public List<PlayerUnitModel> Units { get; set; }
    }
}
