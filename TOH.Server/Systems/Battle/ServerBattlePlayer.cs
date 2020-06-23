using System.Collections.Generic;
using System.Diagnostics;
using TOH.Common.Data;
using TOH.Server.Services;

namespace TOH.Server.Systems.Battle
{
    public class ServerBattlePlayer
    {
        public Session Session { get; private set; }
        public PlayerModel Player { get; private set; }
        public List<ServerBattleUnit> Units { get; set; } = new List<ServerBattleUnit>();

        public Stopwatch SelectUnitsTimer = new Stopwatch();

        public Stopwatch TurnTimer = new Stopwatch();

        public ServerBattlePlayer(Session session, PlayerModel player)
        {
            Session = session;
            Player = player;
        }
    }
}
