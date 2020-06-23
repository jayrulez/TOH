using System.Collections.Generic;
using System.Linq;
using TOH.Common.Data;

namespace TOH.Server.Systems.Battle
{
    public class ServerBattleUnit : BattleUnitModel
    {
        public BattleTurnCommand TurnCommand { get; set; } = null;

        public ServerBattleUnit(PlayerUnitModel playerUnit) : base(playerUnit)
        {
        }

        public void OnBattleStart()
        {

        }

        public void OnTurnStart()
        {

        }

        public void OnTurnEnd()
        {
            ResetTurnbar();
            TurnCommand = null;
        }

        public void AdvanceTurnBar()
        {
            Turnbar += CurrentSpeed * 2;
        }

        public void SetTurnCommand(int skillId, List<int> targetUnitId)
        {
            var skill = PlayerUnit.Unit.Skills.FirstOrDefault(s => s.Value.Id == skillId);

            if (skill.Value.Id == skillId)
            {
                TurnCommand = new BattleTurnCommand(skill.Value.Id, targetUnitId);
            }
        }
    }
}
