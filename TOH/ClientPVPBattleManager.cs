﻿using System;
using System.Collections.Generic;
using TOH.Common.BattleSystem;
using TOH.Common.Data;

namespace TOH
{
    public class ClientBattleUnit : BattleUnitModel
    {
        public ClientBattleUnit(PlayerUnitModel playerUnit) : base(playerUnit)
        {
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
            Update,
            Result
        }

        public string BattleId { get; private set; }

        public BattleState State { get; private set; }
        public List<ClientBattlePlayer> Players { get; private set; }
        public List<ClientBattleUnit> Units { get; private set; }

        public ClientBattleUnit ActiveUnit { get; set; }
        public SkillModel SelectedSkill { get; set; }
        public ClientBattleUnit SelectedTarget { get; set; }

        public bool Win { get; set; }

        public ClientPVPBattle(string battleId, List<BattlePlayerModel> players)
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
                    var clientBattleUnit = new ClientBattleUnit(battleUnit.PlayerUnit);
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
