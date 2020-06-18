using Stride.Core;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Games;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TOH.Common.Data;
using TOH.Network;
using TOH.Network.Client;
using TOH.Network.Packets;

namespace TOH.Systems
{
    public enum GameState
    {
        None,
        InitializeData,
        ConnectToServer,
        CheckSession,
        Login,
        Home,
        Battle
    }

    public class GameEvents
    {
        public static EventKey<GameState> ChangeStateEventKey = new EventKey<GameState>();
    }

    public class NetworkEvents
    {
        public static EventKey<PongPacket> PongPacketEventKey = new EventKey<PongPacket>();
        public static EventKey<BattleInfoPacket> BattleInfoPacketEventKey = new EventKey<BattleInfoPacket>();
        public static EventKey<BattleReadyPacket> BattleReadyPacketEventKey = new EventKey<BattleReadyPacket>();
        public static EventKey<BattleTurnInfoPacket> BattleTurnInfoPacketPacketEventKey = new EventKey<BattleTurnInfoPacket>();
        public static EventKey<BattleUnitTurnPacket> BattleUnitTurnPacketPacketEventKey = new EventKey<BattleUnitTurnPacket>();
    }

    public class GameManager : GameSystemBase
    {
        public GameTcpClient NetworkClient { get; private set; }
        public GameServiceClient ServiceClient { get; private set; }

        public GameState CurrentGameState { get; private set; }
        public GameState NextGameState { get; private set; }

        private Task NetworkTask;

        private CancellationToken NetworkTaskCancellationToken = new CancellationToken();

        private Scene StateScene = null;

        private EventReceiver<GameState> OnGameStateChangedEventListener = new EventReceiver<GameState>(GameEvents.ChangeStateEventKey);

        public GameManager(IServiceRegistry registry) : base(registry)
        {
            CurrentGameState = GameState.None;
            NextGameState = GameState.InitializeData;
        }

        public override void Initialize()
        {
            base.Initialize();

            Enabled = true;
            Visible = false;

            if (Game != null)
            {
                Game.Activated += OnApplicationResumed;
                Game.Deactivated += OnApplicationPaused;
            }
        }

        protected override void Destroy()
        {
            if (Game != null)
            {
                Game.Activated -= OnApplicationResumed;
                Game.Deactivated -= OnApplicationPaused;
            }

            // ensure that OnApplicationPaused is called before destruction, when Game.Deactivated event is not triggered.
            OnApplicationPaused(this, EventArgs.Empty);

            base.Destroy();
        }

        private static void OnApplicationPaused(object sender, EventArgs e)
        {
        }

        private static void OnApplicationResumed(object sender, EventArgs e)
        {
            // revert the state of the edit text here?
        }

        private void LoadGameState(GameState state)
        {
            if (NextGameState == CurrentGameState)
            {
                return;
            }

            var game = Game as TOHGame;

            if (StateScene != null)
            {
                StateScene.Parent = null;
                game.Content.Unload(StateScene);
            }

            switch (state)
            {
                case GameState.Login:
                    StateScene = game.Content.Load<Scene>("Scenes/LoginScene");
                    break;
                case GameState.Home:
                    StateScene = game.Content.Load<Scene>("Scenes/HomeScene");
                    break;
                case GameState.Battle:
                    StateScene = game.Content.Load<Scene>("Scenes/BattleScene");
                    break;
            }

            if (StateScene != null)
                StateScene.Parent = game.SceneSystem.SceneInstance.RootScene;

            CurrentGameState = NextGameState;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (CurrentGameState == GameState.InitializeData)
            {
                InitializeData();
            }

            if (CurrentGameState == GameState.ConnectToServer)
            {
                ConnectToServer();
            }

            if(CurrentGameState == GameState.CheckSession)
            {
                CheckSession();
            }

            if (OnGameStateChangedEventListener.TryReceive(out GameState gameState))
            {
                NextGameState = gameState;
            }

            if (CurrentGameState != NextGameState)
            {
                LoadGameState(NextGameState);
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
                    }
                    catch (Exception ex)
                    {
                        // TODO: Present this to player in a user friendly way
                        Console.WriteLine(ex.Message);
                    }
                });
            }
            else if (DataManager.Instance.State == DataManagerState.Initialized)
            {
                GameEvents.ChangeStateEventKey.Broadcast(GameState.ConnectToServer);
            }
            else
            {
                //TODO: initializing, report progress
            }
        }

        private void ConnectToServer()
        {
            if (ServiceClient == null)
            {
                ServiceClient = new GameServiceClient(new GameServiceClientOptions
                {
                    Protocol = DataManager.Instance.ServerConfig.ServiceProtocol,
                    Host = DataManager.Instance.ServerConfig.ServiceHost,
                    Port = DataManager.Instance.ServerConfig.ServicePort
                });
            }

            if (NetworkClient == null)
            {
                NetworkClient = new GameTcpClient(new TcpClientOptions
                {
                    Host = DataManager.Instance.ServerConfig.TcpServerHost,
                    Port = DataManager.Instance.ServerConfig.TcpServerPort
                });
            }

            if (NetworkClient.State == TcpClientState.None)
            {
                try
                {
                    NetworkClient.Connect();

                    NetworkTask = Task.Factory.StartNew(async () =>
                    {
                        while (!NetworkClient.Connection.IsClosed)
                        {
                            await foreach (var wrappedPacket in NetworkClient.Connection.GetPackets())
                            {
                                if (wrappedPacket.Type.Equals(typeof(PongPacket).FullName))
                                {
                                    var packet = NetworkClient.Connection.Unwrap<PongPacket>(wrappedPacket);

                                    NetworkEvents.PongPacketEventKey.Broadcast(packet);
                                }
                                else if (wrappedPacket.Type.Equals(typeof(BattleInfoPacket).FullName))
                                {
                                    var packet = NetworkClient.Connection.Unwrap<BattleInfoPacket>(wrappedPacket);

                                    NetworkEvents.BattleInfoPacketEventKey.Broadcast(packet);
                                }
                                else if (wrappedPacket.Type.Equals(typeof(BattleReadyPacket).FullName))
                                {
                                    var packet = NetworkClient.Connection.Unwrap<BattleReadyPacket>(wrappedPacket);

                                    NetworkEvents.BattleReadyPacketEventKey.Broadcast(packet);
                                }
                                else if (wrappedPacket.Type.Equals(typeof(BattleTurnInfoPacket).FullName))
                                {
                                    var packet = NetworkClient.Connection.Unwrap<BattleTurnInfoPacket>(wrappedPacket);

                                    NetworkEvents.BattleTurnInfoPacketPacketEventKey.Broadcast(packet);
                                }
                                else if (wrappedPacket.Type.Equals(typeof(BattleUnitTurnPacket).FullName))
                                {
                                    var packet = NetworkClient.Connection.Unwrap<BattleUnitTurnPacket>(wrappedPacket);

                                    NetworkEvents.BattleUnitTurnPacketPacketEventKey.Broadcast(packet);
                                }
                                else
                                {

                                }
                            }
                        }
                    }, NetworkTaskCancellationToken);
                }
                catch (Exception)
                {
                    // Network connection error
                }
            }

            if (NetworkClient.State == TcpClientState.Connected)
            {
                GameEvents.ChangeStateEventKey.Broadcast(GameState.Login);
            }
        }

        private void CheckSession()
        {
            // read session file
            // if file not exist then go to login
            // if file exist, validate session with server
            // if session invalid then go to login
            // if session valid then load player data to local cache then go to home state
        }
    }
}
