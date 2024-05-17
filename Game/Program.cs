namespace Game
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-editor")
                using (EdApp ed = new EdApp("Game.res.Content.bcf", new GTool.Windowing.WindowCreationSettings
                {
                    Width = 1920,
                    Height = 1080,
                    Flags = GTool.Windowing.WindowCreationFlags.Resizable,
                    Title = "Editor"
                }))
                {
                    ed.Run();
                }
            else
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