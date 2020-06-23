using System;
using System.Collections.Generic;
using TOH.Common.Data;

namespace TOH.Common.BattleSystem
{
    public enum BattleState
    {
        Start,
        Waiting,
        Combat,
        TurnEnd,
        End
    }

    public class BattleModel
    {
        public string Id { get; private set; }
        public BattleState State { get; private set; }
        public List<BattlePlayerModel> Players { get; private set; }

        private DateTime StartTime;

        public BattleModel()
        {
            Id = Guid.NewGuid().ToString();
            State = BattleState.Start;
        }

        public void Update()
        {

        }
    }
}
