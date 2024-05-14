using GTool.Content;
using GTool.Debugging;
using GTool.Graphics;
using GTool.Graphics.GUI;
using GTool.Windowing;
using System.Reflection;

namespace GTool
{
    public class GApplication : NativeWindow
    {
        public GraphicsDevice Device { get; private set; }

        public GApplication(in string contentName, in WindowCreationSettings creationSettings) : base(creationSettings)
        {
            Device = new GraphicsDevice(new GraphicsDeviceSettings
            {
                Window = this
            });

            ContentLoader.Initialize("GTool.res.Content.bcf", Assembly.GetExecutingAssembly());
            ContentLoader.Initialize(contentName);

            Gui.Initialize();
        }

        public override void Dispose()
        {
            OnClose();
            Gui.Dispose();
            Device.Dispose();
            base.Dispose();
            ContentLoader.Dispose();
            //Logger.Dispose(); //crashes AOT (ilc) for some reason??
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
