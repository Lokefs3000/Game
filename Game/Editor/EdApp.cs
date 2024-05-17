using Game.Editor;
using GTool;
using GTool.Content;
using GTool.Debugging;
using GTool.Windowing;
using Serilog;
using System.Numerics;

namespace Game
{
    internal class EdApp : GApplication
    {
        //#999999
        //#777777
        //#555555
        //#333333
        //#111111

        private Menubar _menubar;
        private Hierchy _hierchy;

        public EdApp(in string contentName, in WindowCreationSettings creationSettings) : base(contentName, creationSettings)
        {
            if (!Vector.IsHardwareAccelerated)
                Log.Warning("SIMD instructions not supported! This may cause some performance penalties..");
            else
                Log.Information("SIMD instruction are supported! This could improve performance..");

            SceneManager.New(string.Empty);

            _menubar = new Menubar();
            _hierchy = new Hierchy();
        }

        protected override void OnClose()
        {

        }

        protected override void OnUpdate()
        {

        }

        protected override void OnRender()
        {
            _menubar.Render(this);
            _hierchy.Render(this);
        }
    }
}
