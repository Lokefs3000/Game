namespace Game
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //create, run and dispoe of the game
            using (GameApp game = new GameApp("Game.res.Content.bcf", new GTool.Windowing.WindowCreationSettings
            {
                Width = 1280,
                Height = 720,
                Flags = GTool.Windowing.WindowCreationFlags.Resizable,
                Title = "GameApp"
            }))
            {
                game.Run();
            }
        }
    }
}