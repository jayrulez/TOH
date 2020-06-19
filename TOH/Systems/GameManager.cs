using Stride.Core;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Games;
using System;
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
        PickSession,// Temporary, remove when we have signup+login
        Startup,
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

        #region PVPBattle
        public static EventKey<BattleInfoPacket> BattleInfoPacketEventKey = new EventKey<BattleInfoPacket>();
        public static EventKey<BattleReadyPacket> BattleReadyPacketEventKey = new EventKey<BattleReadyPacket>();
        public static EventKey<BattleTurnInfoPacket> BattleTurnInfoPacketEventKey = new EventKey<BattleTurnInfoPacket>();
        public static EventKey<BattleUnitTurnPacket> BattleUnitTurnPacketEventKey = new EventKey<BattleUnitTurnPacket>();
        #endregion

        #region Session
        public static EventKey<JoinSessionFailedPacket> JoinSessionFailedPacketEventKey = new EventKey<JoinSessionFailedPacket>();
        public static EventKey<JoinSessionSuccessPacket> JoinSessionSuccessPacketEventKey = new EventKey<JoinSessionSuccessPacket>();
        public static EventKey<SessionDisconnectedPacket> SessionDisconnectedPacketEventKey = new EventKey<SessionDisconnectedPacket>();
        #endregion
    }

    public class GameManager : GameSystemBase
    {
        public GameTcpClient NetworkClient { get; private set; }
        public GameServiceClient ServiceClient { get; private set; }

        public GameState CurrentGameState { get; private set; }
        public GameState NextGameState { get; private set; }

        private Task NetworkTask;
        private bool NetworkTaskRunning = false;

        private CancellationTokenSource NetworkTaskCancellationTokenSource = new CancellationTokenSource();

        private Scene StateScene = null;

        private EventReceiver<GameState> OnGameStateChangedEventListener = new EventReceiver<GameState>(GameEvents.ChangeStateEventKey);

        public GameManager(IServiceRegistry registry) : base(registry)
        {
            CurrentGameState = GameState.None;
            NextGameState = GameState.PickSession;
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
                case GameState.PickSession:
                    StateScene = game.Content.Load<Scene>("Scenes/PickSessionScene");
                    break;
                case GameState.Startup:
                    StateScene = game.Content.Load<Scene>("Scenes/StartupScene");
                    break;
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

            if (DataManager.Instance.Initialized)
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

        public void StartNetworkTask()
        {
            if (!NetworkTaskRunning)
            {
                NetworkClient.Connect();
                NetworkTaskRunning = true;

                NetworkTask = Task.Factory.StartNew(async () =>
                {
                    while (!NetworkClient.Connection.IsClosed)
                    {
                        if (NetworkTaskRunning == false)
                        {
                            break;
                        }

                        await foreach (var wrappedPacket in NetworkClient.Connection.GetPackets())
                        {
                            if (NetworkClient.Connection.TryUnwrap<PongPacket>(wrappedPacket, out var pongPacket))
                            {
                                NetworkEvents.PongPacketEventKey.Broadcast(pongPacket);
                            }
                            else if (NetworkClient.Connection.TryUnwrap<BattleInfoPacket>(wrappedPacket, out var battleInfoPacket))
                            {
                                NetworkEvents.BattleInfoPacketEventKey.Broadcast(battleInfoPacket);
                            }
                            else if (NetworkClient.Connection.TryUnwrap<BattleReadyPacket>(wrappedPacket, out var battleReadyPacket))
                            {
                                NetworkEvents.BattleReadyPacketEventKey.Broadcast(battleReadyPacket);
                            }
                            else if (NetworkClient.Connection.TryUnwrap<BattleTurnInfoPacket>(wrappedPacket, out var battleTurnInfoPacket))
                            {
                                NetworkEvents.BattleTurnInfoPacketEventKey.Broadcast(battleTurnInfoPacket);
                            }
                            else if (NetworkClient.Connection.TryUnwrap<BattleUnitTurnPacket>(wrappedPacket, out var battleUnitTurnPacket))
                            {
                                NetworkEvents.BattleUnitTurnPacketEventKey.Broadcast(battleUnitTurnPacket);
                            }
                            else if (NetworkClient.Connection.TryUnwrap<JoinSessionFailedPacket>(wrappedPacket, out var joinSessionFailedPacket))
                            {
                                NetworkEvents.JoinSessionFailedPacketEventKey.Broadcast(joinSessionFailedPacket);
                            }
                            else if (NetworkClient.Connection.TryUnwrap<JoinSessionSuccessPacket>(wrappedPacket, out var joinSessionSuccessPacket))
                            {
                                NetworkEvents.JoinSessionSuccessPacketEventKey.Broadcast(joinSessionSuccessPacket);
                            }
                            else if (NetworkClient.Connection.TryUnwrap<SessionDisconnectedPacket>(wrappedPacket, out var sessionDisconnectedPacket))
                            {
                                NetworkEvents.SessionDisconnectedPacketEventKey.Broadcast(sessionDisconnectedPacket);
                            }
                            else
                            {

                            }
                        }
                    }
                }, NetworkTaskCancellationTokenSource.Token);
            }
        }

        public void StopNetworkTask()
        {
            NetworkTaskRunning = false;
            NetworkTaskCancellationTokenSource.Cancel();
        }
    }
}
