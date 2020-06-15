using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using TOH.Systems;
using TOH.Network.Packets;
using TOH.Network.Client;

namespace TOH
{
    public class BattleUIScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        private UIComponent BattleUI;

        private TextBlock PingRequestTextBlock;
        private TextBlock PingResponseTextBlock;

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

                Task.Factory.StartNew( async () =>
                {
                    while(netClient.State == TcpClientState.Connected)
                    {
                        await foreach (var packet in netClient.Connection.GetPackets())
                        {
                            if(packet.Type.Equals(typeof(PongPacket).FullName))
                            {
                                var pongPacket = netClient.Connection.Unwrap<PongPacket>(packet);

                                Log.Debug($"Recevied packet '{pongPacket.PacketId}' with type '{pongPacket.Type}' and PingId='{pongPacket.PingId}'.");
                                PingResponseTextBlock.Text = $"Recevied Pong '{packet.PacketId}' from Ping '{pongPacket.PingId}'.";
                            }
                        }
                    }
                });
            }

        }

        public override void Update()
        {
            // Do stuff every new frame
        }
    }
}
