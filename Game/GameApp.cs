using GTool;
using GTool.Content;
using GTool.Debugging;
using GTool.Windowing;
using System.Numerics;

namespace Game
{
    internal class GameApp : GApplication
    {
        public GameApp(in string contentName, in WindowCreationSettings creationSettings) : base(contentName, creationSettings)
        {
            if (!Vector.IsHardwareAccelerated)
                Logger.Warning("SIMD instructions not supported! This may cause some performance penalties..");
            else
                Logger.Information("SIMD instruction are supported! This could improve performance..");
        }

        protected override void OnClose()
        {
            
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnRender()
        {
            
        }
    }
}
