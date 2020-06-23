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

    public class Battle
    {
        public string Id { get; private set; }
        public BattleState State { get; private set; }
        public List<BattlePlayer> Players { get; private set; }

        private DateTime StartTime;

        public Battle()
        {
            Id = Guid.NewGuid().ToString();
            State = BattleState.Start;
        }

        public void Update()
        {

        }
    }
}
