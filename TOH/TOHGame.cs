using TOH.Systems;

namespace TOH
{
    public class TOHGame : Stride.Engine.Game
    {
        public GameManagerSystem GameManager { get; }

        public TOHGame() : base()
        {
            GameManager = new GameManagerSystem(Services);
            Services.AddService(GameManager);
        }

        protected override void Initialize()
        {
            base.Initialize();

            GameSystems.Add(GameManager);
        }
    }
}
