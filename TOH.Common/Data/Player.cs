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
}
