using Stride.Core;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.UI;
using Stride.UI.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network.Client;
using TOH.Network.Packets;
using TOH.Systems;

namespace TOH
{
    public class StartupManagerScript : SyncScript
    {
        public enum StartupState
        {
            None,
            InitializeData,
            ConnectToServer,
            CheckSession
        }

        public enum CheckSessionState
        {
            None,
            Checking,
            CheckFailed,
            Joining,
            JoinFailed,
            JoinSucceeded
        }

        private StartupState State = StartupState.None;
        private CheckSessionState SessionState;
        private UIComponent StartupUI;
        private TextBlock StartupStatusText;

        private GameManager GameManager;


        private EventReceiver<JoinSessionSuccessPacket> JoinSessionSuccessEventListener = new EventReceiver<JoinSessionSuccessPacket>(NetworkEvents.JoinSessionSuccessPacketEventKey);
        private EventReceiver<JoinSessionFailedPacket> JoinSessionFailedEventListener = new EventReceiver<JoinSessionFailedPacket>(NetworkEvents.JoinSessionFailedPacketEventKey);

        public override void Start()
        {
            StartupUI = Entity.Get<UIComponent>();

            if (StartupUI != null)
            {
                StartupStatusText = StartupUI.Page.RootElement.FindVisualChildOfType<TextBlock>();
            }

            var game = (TOHGame)Game;

            GameManager = game.GameManager;
        }

        public override void Update()
        {
            if (State == StartupState.None)
            {
                State = StartupState.InitializeData;
            }
            else if (State == StartupState.InitializeData)
            {
                InitializeData();
            }
            else if (State == StartupState.ConnectToServer)
            {
                ConnectToServer();
            }
            else if (State == StartupState.CheckSession)
            {
                CheckSession();
            }
        }

        private void InitializeData()
        {
            if (DataManager.Instance.State == DataManagerState.None)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var dataPath = Path.Combine(PlatformFolders.ApplicationDataDirectory, "Config");

                        await DataManager.Instance.Initialize(dataPath);

                        Thread.Sleep(5000); // simulate long data load
                    }
                    catch (Exception ex)
                    {
                        StartupStatusText.Text = "Failed to initialize game data.";

                        Log.Error(ex.Message);
                    }
                });
            }
            else if (DataManager.Instance.State == DataManagerState.Initialized)
            {
                State = StartupState.ConnectToServer;
            }
            else
            {
                StartupStatusText.Text = "Initializing game data.";
            }
        }

        private void ConnectToServer()
        {
            if (GameManager.NetworkClient != null)
            {
                if (GameManager.NetworkClient.State == TcpClientState.None)
                {
                    try
                    {
                        GameManager.StartNetworkTask();
                    }
                    catch (Exception)
                    {
                        StartupStatusText.Text = "An error occured while connecting to the network.";
                    }
                }
                else if (GameManager.NetworkClient.State == TcpClientState.Connected)
                {
                    SessionState = CheckSessionState.None;

                    State = StartupState.CheckSession;
                }
                else
                {
                    StartupStatusText.Text = "Connecting to server.";
                }
            }
        }

        private void CheckSession()
        {
            if (SessionState == CheckSessionState.None)
            {
                SessionState = CheckSessionState.Checking;

                //var sessionId = GameDatabase.Instance.GetSessionId();
                var sessionId = StaticConfig.SessionId;

                if (string.IsNullOrEmpty(sessionId))
                {
                    SessionState = CheckSessionState.CheckFailed;
                }
                else
                {
                    /*
                    var response = ServiceClient.PlayerService.GetPlayerSessionById(new IdentifierData<string> { Identifier = sessionId });
                    */

                    if (false/*!response.IsSuccessful || response.Data.IsExpired*/)
                    {
                        SessionState = CheckSessionState.CheckFailed;
                    }
                    else
                    {
                        GameManager.NetworkClient.Connection.Send(new JoinSessionPacket
                        {
                            Token = sessionId
                        });


                        StartupStatusText.Text = "Joining session.";

                        SessionState = CheckSessionState.Joining;
                    }
                }
            }
            else if (SessionState == CheckSessionState.Checking)
            {
                StartupStatusText.Text = "Checking player data.";
            }
            else if (SessionState == CheckSessionState.CheckFailed)
            {
                GameEvents.ChangeStateEventKey.Broadcast(GameState.Login);
            }
            else if (SessionState == CheckSessionState.Joining)
            {
                if (JoinSessionFailedEventListener.TryReceive(out JoinSessionFailedPacket joinSessionFailedPacket))
                {
                    SessionState = CheckSessionState.JoinFailed;

                    if (joinSessionFailedPacket.Code == JoinSessionFailCode.InvalidSession)
                    {
                        GameDatabase.Instance.RemoveSession();

                        // Kick back to startup
                        GameEvents.ChangeStateEventKey.Broadcast(GameState.Startup);
                    }
                    else
                    {
                        StartupStatusText.Text = "Failed to join session on server.";

                        //TODO: Setup retry UI
                    }
                }

                if (JoinSessionSuccessEventListener.TryReceive(out JoinSessionSuccessPacket joinSessionSuccessPacket))
                {
                    SessionState = CheckSessionState.JoinSucceeded;
                }
            }
            else if (SessionState == CheckSessionState.JoinSucceeded)
            {
                GameEvents.ChangeStateEventKey.Broadcast(GameState.Home);
            }
            else if (SessionState == CheckSessionState.JoinFailed)
            {
                //
            }
        }
    }
}
