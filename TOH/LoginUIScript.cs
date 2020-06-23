using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Events;
using System.Threading.Tasks;
using TOH.Systems;

namespace TOH
{
    public class LoginUIScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        private UIComponent LoginUI;
        private EditText UsernameInput;
        private Button LoginButton;
        private TextBlock LoginStatusText;

        private enum LoginState
        {
            None,
            Processing,
            Successful,
            Failed
        }

        private LoginState PlayerLoginState = LoginState.None;
        private GameManager GameManager;

        public override void Start()
        {

            var game = (TOHGame)Game;

            GameManager = game.GameManager;


            // Initialization of the script.
            LoginUI = Entity.Get<UIComponent>();

            if (LoginUI != null)
            {
                UsernameInput = LoginUI.Page.RootElement.FindVisualChildOfType<EditText>();
                LoginButton = LoginUI.Page.RootElement.FindVisualChildOfType<Button>();

                LoginStatusText = LoginUI.Page.RootElement.FindVisualChildOfType<TextBlock>("LoginStatusText");
                LoginStatusText.Visibility = Visibility.Collapsed;

                if (LoginButton != null)
                {
                    LoginButton.Click += (object sender, RoutedEventArgs args) =>
                    {
                        DoLogin();
                    };
                }
            }
        }

        private void DoLogin()
        {
            if (!string.IsNullOrEmpty(UsernameInput?.Text))
            {
                LoginButton.IsEnabled = false;

                PlayerLoginState = LoginState.Processing;

                Task.Factory.StartNew(() =>
                {
                    var loginResponse = GameManager.ServiceClient.PlayerService.Login(new Common.Services.IdentifierData<string> { Identifier = UsernameInput.Text.Trim() });

                    if (loginResponse.IsSuccessful)
                    {
                        GameDatabase.Instance.SetSession(new GameDatabase.Session
                        {
                            SessionId = loginResponse.Data.Id,
                            PlayerId = loginResponse.Data.PlayerId
                        });

                        PlayerLoginState = LoginState.Successful;
                    }
                    else
                    {
                        PlayerLoginState = LoginState.Failed;
                    }
                }).ContinueWith((Task task) =>
                {
                    LoginButton.IsEnabled = true;
                });
            }
        }

        public override void Update()
        {
            if (PlayerLoginState == LoginState.None)
            {

            }
            else if (PlayerLoginState == LoginState.Processing)
            {
                LoginStatusText.Visibility = Visibility.Visible;
                LoginStatusText.Text = "Processing login.";
            }
            else if (PlayerLoginState == LoginState.Successful)
            {
                LoginStatusText.Visibility = Visibility.Visible;
                LoginStatusText.Text = "Login Successful";
                PlayerLoginState = LoginState.None;
                GameEvents.ChangeStateEventKey.Broadcast(GameState.Startup);
            }
            else if (PlayerLoginState == LoginState.Failed)
            {

                LoginStatusText.Visibility = Visibility.Visible;
                LoginStatusText.Text = "Login failed.";
            }
        }
    }
}
