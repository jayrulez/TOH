using Stride.Engine;
using Stride.Engine.Events;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using TOH.Network.Packets;
using TOH.Systems;

namespace TOH
{
    public enum HomeState
    {
        None,
        Matching,
        MatchFound,
        Countdown,
        SelectUnits,
        MatchReadyWait
    }

    public class BattleInfo
    {
        public string BattleId { get; set; }
    }

    public class HomeUIScript : SyncScript
    {
        private EventReceiver<BattleCountdownPacket> BattleCountdownEventListener = new EventReceiver<BattleCountdownPacket>(NetworkEvents.BattleCountdownPacketEventKey);
        private EventReceiver<BattleUnitSelectionReadyPacket> BattleUnitSelectionReadyEventListener = new EventReceiver<BattleUnitSelectionReadyPacket>(NetworkEvents.BattleUnitSelectionReadyPacketEventKey);
        private EventReceiver<BattleInfoPacket> BattleInfoEventListener = new EventReceiver<BattleInfoPacket>(NetworkEvents.BattleInfoPacketEventKey);
        private EventReceiver<BattleReadyPacket> BattleReadyEventListener = new EventReceiver<BattleReadyPacket>(NetworkEvents.BattleReadyPacketEventKey);

        private HomeState HomeState;
        private GameManager GameManager;
        private Button MatchButton;
        private TextBlock CountdownText;
        private BattleInfo BattleInfo;

        // Declared public member fields and properties will show in the game studio
        private UIComponent HomeUI;

        public override void Start()
        {
            HomeState = HomeState.None;
            var game = (TOHGame)Game;

            GameManager = game.GameManager;

            // Initialization of the script.
            HomeUI = Entity.Get<UIComponent>();

            if (HomeUI != null)
            {
                MatchButton = HomeUI.Page.RootElement.FindVisualChildOfType<Button>();
                CountdownText = HomeUI.Page.RootElement.FindVisualChildOfType<TextBlock>("Countdown");

                if (MatchButton != null)
                {
                    MatchButton.Click += (object sender, RoutedEventArgs args) =>
                    {
                        if (HomeState == HomeState.None)
                        {
                            GameManager.NetworkClient.Connection.Send(new FindBattlePacket());

                            HomeState = HomeState.Matching;

                            MatchButton.IsEnabled = false;
                        }
                    };
                }
            }
        }

        public override void Update()
        {

            if (BattleInfoEventListener.TryReceive(out var battleInfoPacket))
            {
                BattleInfo = new BattleInfo
                {
                    BattleId = battleInfoPacket.BattleId
                };

                HomeState = HomeState.MatchFound;
            }
            else if (BattleCountdownEventListener.TryReceive(out var battleCountdownPacket))
            {
                if (HomeState != HomeState.Countdown)
                    HomeState = HomeState.Countdown;

                CountdownText.Text = $"Battle will start in {battleCountdownPacket.Count}";
            }
            else if (BattleUnitSelectionReadyEventListener.TryReceive(out var battleUnitSelectionReadyPacket))
            {
                HomeState = HomeState.SelectUnits;
            }

            if (HomeState != HomeState.Countdown)
            {
                CountdownText.Visibility = Visibility.Collapsed;
            }

            if (HomeState == HomeState.Matching)
            {
                var buttonText = MatchButton.FindVisualChildOfType<TextBlock>();
                buttonText.Text = "Matching";
            }
            else if (HomeState == HomeState.MatchFound)
            {
                MatchButton.Visibility = Visibility.Collapsed;
                HomeState = HomeState.Countdown;
            }
            else if (HomeState == HomeState.Countdown)
            {
                CountdownText.Visibility = Visibility.Visible;
            }
            else if (HomeState == HomeState.SelectUnits)
            {
                //if(MatchInfo == null)
                //TODO: Send back to state before find match

                //TODO, listen for packet that shows opponents units and update UI

                GameManager.NetworkClient.Connection.Send(new SetBattleUnitsPacket()
                {
                    BattleId = BattleInfo.BattleId,
                    Units = new System.Collections.Generic.List<int> { 1, 2, 3 }
                });

                HomeState = HomeState.MatchReadyWait;
            }
            else if (HomeState == HomeState.MatchReadyWait)
            {
                if (BattleReadyEventListener.TryReceive(out BattleReadyPacket battleReadyPacket))
                {
                    var battle = new ClientPVPBattle(battleReadyPacket.BattleId, battleReadyPacket.Players);

                    ClientPVPBattleManager.Instance.SetBattle(battle);

                    GameEvents.ChangeStateEventKey.Broadcast(GameState.Battle);
                }
            }
        }
    }
}
