using GTool.Content;
using GTool.Debugging;
using GTool.Graphics;
using GTool.Interface;
using GTool.Scene;
using GTool.Windowing;
using Serilog;
using System.Reflection;

namespace GTool
{
    public class GApplication : NativeWindow
    {
        public GraphicsDevice Device { get; private set; }
        public SceneManager SceneManager { get; private set; }

        public GApplication(in string contentName, in WindowCreationSettings creationSettings) : base(creationSettings)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Device = new GraphicsDevice(new GraphicsDeviceSettings
            {
                Window = this
            });

            ContentLoader.AppendFileSystem("GTool.res.Content.bcf", Assembly.GetExecutingAssembly());
            //ContentLoader.Initialize(contentName);

            Gui.Initialize();
            Gui.Resize(WindowSize);

            SceneManager = new SceneManager();
        }

        public override void Dispose()
        {
            OnClose();
            Gui.Dispose();
            Device.Dispose();
            base.Dispose();
            SceneManager.Dispose();
            //ContentManager.Dispose();
            ContentLoader.Dispose();
            //Logger.Dispose(); //crashes AOT (ilc.exe) for some reason??
        }

        public void Run()
        {
            while (!IsWindowClosed)
            {
                PollEvents();
                
                OnUpdate();

                OnRender();
                Gui.Render();
                Device.Present();
            }
        }

        protected virtual void OnClose() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnRender() { }
    }
}
