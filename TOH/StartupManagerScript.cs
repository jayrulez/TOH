using Stride.Core;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network.Client;
using TOH.Systems;

namespace TOH
{
    public class StartupManagerScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio

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
            CheckSucceeded
        }

        private StartupState State = StartupState.None;
        private CheckSessionState SessionState;
        private UIComponent StartupUI;
        private TextBlock StartupStatusText;

        private GameManager GameManager;

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

                        Thread.Sleep(5000); // simulate long op
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

                var sessionId = GameDatabase.Instance.GetSessionId();

                if (string.IsNullOrEmpty(sessionId))
                {
                    SessionState = CheckSessionState.CheckFailed;
                }
                else
                {
                    /*
                    var response = ServiceClient.PlayerService.GetPlayerSessionById(new IdentifierData<string> { Identifier = sessionId });

                    if (!response.IsSuccessful || response.Data.IsExpired)
                    {
                        SessionState = SessionState.CheckFailed;
                    }
                    else
                    {
                        SessionState = SessionState.CheckSucceeded;
                    }
                    */


                    SessionState = CheckSessionState.CheckFailed;// remove this when using service
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
            else if (SessionState == CheckSessionState.CheckSucceeded)
            {
                GameEvents.ChangeStateEventKey.Broadcast(GameState.Home);
            }
        }
    }
}
