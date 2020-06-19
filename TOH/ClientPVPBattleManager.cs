using System;
using System.Collections.Generic;
using TOH.Common.BattleSystem;

namespace TOH
{
    public class ClientBattleUnit : BattleUnit
    {
        public BattleUnit BattleUnit { get; set; }
        public ClientBattleUnit(BattleUnit battleUnit) : base(battleUnit.PlayerUnit)
        {
            BattleUnit = battleUnit;
        }
    }

    public class ClientBattlePlayer
    {
        public int Id { get; set; }
        public List<ClientBattleUnit> Units { get; set; }
    }

    public class ClientPVPBattle
    {
        public enum BattleState
        {
            None,
            LoadAssets,
            Update
        }

        public string BattleId { get; private set; }

        public BattleState State { get; private set; }
        public List<ClientBattlePlayer> Players { get; private set; }
        public List<ClientBattleUnit> Units { get; private set; }

        public ClientPVPBattle(string battleId, List<BattlePlayer> players)
        {
            BattleId = battleId;
            Players = new List<ClientBattlePlayer>();
            Units = new List<ClientBattleUnit>();

            foreach (var player in players)
            {
                var clientBattlePlayer = new ClientBattlePlayer
                {
                    Id = player.Id,
                    Units = new List<ClientBattleUnit>()
                };

                foreach (var battleUnit in player.Units)
                {
                    var clientBattleUnit = new ClientBattleUnit(battleUnit);
                    clientBattlePlayer.Units.Add(clientBattleUnit);

                    Units.Add(clientBattleUnit);
                }

                Players.Add(clientBattlePlayer);

                State = BattleState.None;
            }
        }

        public void SetState(BattleState state)
        {
            State = state;
        }
    }

    public class ClientPVPBattleManager
    {
        private static readonly Lazy<ClientPVPBattleManager> lazy = new Lazy<ClientPVPBattleManager>(() => new ClientPVPBattleManager(), true);

        public static ClientPVPBattleManager Instance { get { return lazy.Value; } }

        public ClientPVPBattle Battle { get; private set; }

        public void SetBattle(ClientPVPBattle battle)
        {
            Battle = battle;
        }
    }
}
