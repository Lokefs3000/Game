using Game.Core;

namespace Game
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //create, run and dispoe of the game
            using (GameClass game = new GameClass())
            {
                game.Run();
            }
        }
    }
}