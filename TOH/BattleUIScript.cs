using Stride.Engine;
using Stride.Engine.Events;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using TOH.Network.Packets;
using TOH.Systems;

namespace TOH
{
    public class BattleUIScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        private UIComponent BattleUI;

        private TextBlock PingRequestTextBlock;
        private TextBlock PingResponseTextBlock;

        private EventReceiver<PongPacket> PongPacketListener = new EventReceiver<PongPacket>(NetworkEvents.PongPacketEventKey);

        public override void Start()
        {
            // Initialization of the script.
            BattleUI = Entity.Get<UIComponent>();

            if (BattleUI != null)
            {
                PingRequestTextBlock = BattleUI.Page.RootElement.FindVisualChildOfType<TextBlock>("PingRequest");
                PingResponseTextBlock = BattleUI.Page.RootElement.FindVisualChildOfType<TextBlock>("PingResponse");

                var game = (TOHGame)Game;

                var netClient = game.GameManager.NetworkClient;

                var loginButton = BattleUI.Page.RootElement.FindVisualChildOfType<Button>();

                if (loginButton != null)
                {
                    loginButton.Click += async (object sender, RoutedEventArgs args) =>
                    {
                        var packet = new PingPacket { };

                        Log.Debug($"Sending Pink Packet '{packet.PacketId}'.");

                        PingRequestTextBlock.Text = $"{packet.PacketId}";

                        await netClient.Connection.Send(packet);
                        //GameEvents.ChangeStateEventKey.Broadcast(GameState.Home);
                    };
                }
            }

        }

        public override void Update()
        {
            if (PongPacketListener.TryReceive(out PongPacket packet))
            {
                PingResponseTextBlock.Text = $"Recevied Pong '{packet.PacketId}' from Ping '{packet.PingId}'.";
            }
            // Do stuff every new frame
        }
    }
}
