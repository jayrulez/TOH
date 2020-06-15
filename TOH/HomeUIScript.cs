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
        Matching
    }

    public class HomeUIScript : SyncScript
    {
        private EventReceiver<MatchInfoPacket> MatchInfoEventListener = new EventReceiver<MatchInfoPacket>(NetworkEvents.MatchInfoPacketEventKey);

        private HomeState HomeState;
        private GameManagerSystem GameManager;
        private Button MatchButton;

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

                if (MatchButton != null)
                {
                    MatchButton.Click += (object sender, RoutedEventArgs args) =>
                    {
                        if(HomeState == HomeState.None)
                        {
                            GameManager.NetworkClient.Connection.Send(new FindMatchPacket());

                            HomeState = HomeState.Matching;

                            MatchButton.IsEnabled = false;
                        }
                    };
                }
            }
        }

        public override void Update()
        {
            if(HomeState == HomeState.Matching)
            {
                var buttonText = MatchButton.FindVisualChildOfType<TextBlock>();
                buttonText.Text = "Matching";
            }


            if (MatchInfoEventListener.TryReceive(out MatchInfoPacket packet))
            {
                GameEvents.ChangeStateEventKey.Broadcast(GameState.Battle);
                //PingResponseTextBlock.Text = $"Recevied Pong '{packet.PacketId}' from Ping '{packet.PingId}'.";
            }
        }
    }
}
