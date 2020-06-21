using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.UI.Controls;
using Stride.UI;
using Stride.UI.Events;
using TOH.Systems;

namespace TOH
{
    public class PickSessionUIScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio

        private UIComponent PickSessionUI;

        private Button RobertButton;
        private Button EvonButton;


        public override void Start()
        {
            PickSessionUI = Entity.Get<UIComponent>();

            if (PickSessionUI != null)
            {
                RobertButton = PickSessionUI.Page.RootElement.FindVisualChildOfType<Button>("Robert");

                if (RobertButton != null)
                {
                    RobertButton.Click += (object sender, RoutedEventArgs args) =>
                    {
                        RobertButton.IsEnabled = false;

                        if (EvonButton != null)
                            EvonButton.IsEnabled = false;

                        SessionSelected("efbc3c1a-90f8-4896-9f69-d6ae3dc5b6db");
                    };
                }

                EvonButton = PickSessionUI.Page.RootElement.FindVisualChildOfType<Button>("Evon");


                if (EvonButton != null)
                {
                    EvonButton.Click += (object sender, RoutedEventArgs args) =>
                    {
                        EvonButton.IsEnabled = false;

                        if (RobertButton != null)
                            RobertButton.IsEnabled = false;

                        SessionSelected("e2b10a43-726b-41fe-95e5-909dca5bf154");
                    };
                }

            }
        }

        private void SessionSelected(string sessionId)
        {
            StaticConfig.SessionId = sessionId;

            GameEvents.ChangeStateEventKey.Broadcast(GameState.Startup);
        }

        public override void Update()
        {
            // Do stuff every new frame
        }
    }
}
