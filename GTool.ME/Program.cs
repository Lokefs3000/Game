namespace GTool.ME
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (Application app = new Application("GTool.ME.res.Content.bcf", new Windowing.WindowCreationSettings
            {
                Width = 1366,
                Height = 826,
                Flags = Windowing.WindowCreationFlags.Resizable,
                Title = "Map editor"
            })) { app.Run(); }
        }
    }
}