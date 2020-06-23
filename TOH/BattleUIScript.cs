using Stride.Engine;
using Stride.Engine.Events;
using Stride.UI;
using Stride.UI.Controls;
using System.Linq;
using TOH.Network.Packets;
using TOH.Systems;

namespace TOH
{
    public class BattleUIScript : SyncScript
    {
        private UIComponent BattleInfoUI;
        private TextBlock BattleInfoText;

        private enum AssetLoadState
        {
            None,
            Loading,
            Loaded
        }

        private enum BattleUIState
        {
            None,
            ShowSkillList
        }

        private AssetLoadState LoadAssetState = AssetLoadState.None;
        private BattleUIState UIState = BattleUIState.None;

        private EventReceiver<BattleTurnInfoPacket> BattleTurnInfoEventListener = new EventReceiver<BattleTurnInfoPacket>(NetworkEvents.BattleTurnInfoPacketEventKey);
        private EventReceiver<BattleUnitTurnPacket> BattleUnitTurnEventListener = new EventReceiver<BattleUnitTurnPacket>(NetworkEvents.BattleUnitTurnPacketEventKey);
        private EventReceiver<BattleResultPacket> BattleResultEventListener = new EventReceiver<BattleResultPacket>(NetworkEvents.BattleResultPacketEventKey);

        public override void Start()
        {
            BattleInfoUI = Entity.Get<UIComponent>();

            if (BattleInfoUI != null)
            {
                BattleInfoText = BattleInfoUI.Page.RootElement.FindVisualChildOfType<TextBlock>();
            }
        }

        private void LoadBattleAssets()
        {
            LoadAssetState = AssetLoadState.Loading;

            // load assets here

            LoadAssetState = AssetLoadState.Loaded;
        }

        public override void Update()
        {
            if (ClientPVPBattleManager.Instance.Battle.State == ClientPVPBattle.BattleState.None)
            {
                ClientPVPBattleManager.Instance.Battle.SetState(ClientPVPBattle.BattleState.LoadAssets);
            }
            else if (ClientPVPBattleManager.Instance.Battle.State == ClientPVPBattle.BattleState.LoadAssets)
            {
                if (LoadAssetState == AssetLoadState.None)
                    LoadBattleAssets();
                if (LoadAssetState == AssetLoadState.Loading)
                {
                    // update UI with progress
                }

                if (LoadAssetState == AssetLoadState.Loaded)
                {
                    ClientPVPBattleManager.Instance.Battle.SetState(ClientPVPBattle.BattleState.Update);
                }
            }
            else if (ClientPVPBattleManager.Instance.Battle.State == ClientPVPBattle.BattleState.Update)
            {
                // update battle ui state here

                if (BattleUnitTurnEventListener.TryReceive(out BattleUnitTurnPacket battleUnitTurnPacket))
                {
                    var unit = ClientPVPBattleManager.Instance.Battle.Units.FirstOrDefault(u => u.PlayerUnit.Id == battleUnitTurnPacket.UnitId);

                    ClientPVPBattleManager.Instance.Battle.ActiveUnit = unit;

                    var session = GameDatabase.Instance.GetSession();

                    var myUnits = ClientPVPBattleManager.Instance.Battle.Players.FirstOrDefault(p => p.Id == session.PlayerId)?.Units;

                    var currentUnit = myUnits.FirstOrDefault(u => u.PlayerUnit.Id == unit.PlayerUnit.Id);

                    if (currentUnit != null)
                    {
                        // This is my unit
                        BattleInfoText.Text += $"My Unit '{unit.PlayerUnit.Unit.Name}' is taking a turn.\n";

                        // Update skill list UI
                        UIState = BattleUIState.ShowSkillList;
                    }
                    else
                    {
                        // This is the opponent's unit.
                        BattleInfoText.Text += $"Opponent's Unit '{unit.PlayerUnit.Unit.Name}' is taking a turn.\n";

                        //Hide skill list UI
                        UIState = BattleUIState.None;
                    }
                }
                else if (BattleTurnInfoEventListener.TryReceive(out BattleTurnInfoPacket battleTurnInfoPacket))
                {
                    if (BattleInfoText != null)
                    {
                        BattleInfoText.Text += $"'{battleTurnInfoPacket.Unit.PlayerUnit.Unit.Name}' used '{battleTurnInfoPacket.SkillId}' on '{string.Join(", ", battleTurnInfoPacket.Targets.Select(t => t.PlayerUnit.Unit.Name).ToList())}'\n";
                    }
                }
                else if (BattleResultEventListener.TryReceive(out BattleResultPacket battleResultPacket))
                {
                    ClientPVPBattleManager.Instance.Battle.Win = battleResultPacket.Status == BattleResultStatus.Win;
                    ClientPVPBattleManager.Instance.Battle.SetState(ClientPVPBattle.BattleState.Result);
                }

                if (UIState == BattleUIState.None)
                {

                }
                else if (UIState == BattleUIState.ShowSkillList)
                {
                    if(ClientPVPBattleManager.Instance.Battle.ActiveUnit != null)
                    {
                        PopulateSkillsList(ClientPVPBattleManager.Instance.Battle.ActiveUnit);
                    }
                }
                else
                {

                }
            }
            else if (ClientPVPBattleManager.Instance.Battle.State == ClientPVPBattle.BattleState.Result)
            {
                BattleInfoText.Text = ClientPVPBattleManager.Instance.Battle.Win ? "Victory" : "Defeated";
            }
        }

        private void PopulateSkillsList(ClientBattleUnit unit)
        {
            var skills = unit.PlayerUnit.Unit.Skills;
        }
    }
}
