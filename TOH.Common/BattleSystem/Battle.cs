using System;

namespace TOH.Common.BattleSystem
{
    public enum BattleState
    {
        Start,
        Loading,
        Combat,
        TurnEnd,
        End
    }

    public class Battle
    {
        public string Id { get; private set; }
        public BattleState State { get; private set; }
        public BattlePlayer Player1 { get; private set; }
        public BattlePlayer Player2 { get; private set; }

        private DateTime StartTime;

        public Battle()
        {
            Id = Guid.NewGuid().ToString();
            State = BattleState.Start;
        }
    }
}
