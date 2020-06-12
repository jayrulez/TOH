namespace TOH.Windows
{
    class TOHApp
    {
        static void Main(string[] args)
        {
            using (var game = new TOHGame())
            {
                game.Run();
            }
        }
    }
}
