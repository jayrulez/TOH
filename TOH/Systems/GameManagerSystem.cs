﻿using Stride.Core;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Games;
using System;
using System.IO;
using System.Threading.Tasks;
using TOH.Common.Data;

namespace TOH.Systems
{
    public enum GameState
    {
        None,
        InitializeData,
        Login,
        Home,
        Battle
    }

    public class GameEvents
    {
        public static EventKey<GameState> ChangeStateEventKey = new EventKey<GameState>();
    }

    public class GameManagerSystem : GameSystemBase
    {
        public GameState CurrentGameState { get; private set; }
        public GameState NextGameState { get; private set; }

        private Scene StateScene = null;

        private EventReceiver<GameState> OnGameStateChangedEventListener = new EventReceiver<GameState>(GameEvents.ChangeStateEventKey);

        public GameManagerSystem(IServiceRegistry registry) : base(registry)
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
                GameEvents.ChangeStateEventKey.Broadcast(GameState.Login);
            }
            else
            {
            }
        }
    }
}
