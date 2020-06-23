using System.Collections.Generic;

namespace TOH.Common.Data
{
    public class BattlePlayerModel
    {
        public int Id { get; set; }

        public List<BattleUnitModel> Units { get; set; }
    }
}
