using Stride.Engine;

namespace TOH.Windows
{
    class TOHApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
