﻿using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using TOH.Systems;

namespace TOH
{
    public class LoginUIScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        private UIComponent LoginUI;

        public override void Start()
        {
            var game = (TOHGame)Game;

            // Initialization of the script.
            LoginUI = Entity.Get<UIComponent>();

            if (LoginUI != null)
            {
                var loginButton = LoginUI.Page.RootElement.FindVisualChildOfType<Button>();

                if (loginButton != null)
                {
                    loginButton.Click += (object sender, RoutedEventArgs args) =>
                    {
                        //game.GameManager.StopNetworkTask();
                        GameEvents.ChangeStateEventKey.Broadcast(GameState.Home);
                    };
                }
            }
        }

        public override void Update()
        {
            // Do stuff every new frame
        }
    }
}
