using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Config
{
    internal class StartupConfig
    {
        public static readonly StartupConfig Config = new StartupConfig();

        public readonly int WindowWidth;
        public readonly int WindowHeight;
        public readonly int RenderVSync;

        public StartupConfig()
        {
            if (!File.Exists("res/startup.config"))
            {
                Log.Fatal("Failed to find {@Config}!", "res/startup.config");
                Environment.Exit(1);
            }

            Dictionary<string, string> kvDict = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines("res/startup.config");
            foreach (string line in lines)
            {
                string[] kv = line.Split('=');
                kvDict.Add(kv[0].Trim(), kv[1].Trim());                
            }

            WindowWidth = int.Parse(kvDict["window_width"]);
            WindowHeight = int.Parse(kvDict["window_height"]);
            RenderVSync = int.Parse(kvDict["render_vsync"]);
        }
    }
}
