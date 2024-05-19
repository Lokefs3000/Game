using GTool.Windowing;

namespace GEditor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-edopen" && Directory.Exists(args[1]))
            {
                using (GEditor hub = new GEditor(args[1], new WindowCreationSettings
                {
                    Width = 1280,
                    Height = 720,
                    Title = "Hub",
                    Flags = WindowCreationFlags.Resizable
                })) hub.Run();
            }
            else
            {
                using (GHub hub = new GHub(new WindowCreationSettings
                {
                    Width = 900,
                    Height = 600,
                    Title = "Hub"
                })) hub.Run();
            }
        }
    }
}