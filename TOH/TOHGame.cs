using TOH.Systems;

namespace TOH
{
    public class TOHGame : Stride.Engine.Game
    {
        public GameManager GameManager { get; }

        public TOHGame() : base()
        {
            GameManager = new GameManager(Services);
            Services.AddService(GameManager);
        }

        protected override void Initialize()
        {
            base.Initialize();

            GameSystems.Add(GameManager);
        }
    }
}
