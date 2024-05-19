using GEditor.Content;
using GEditor.Editor;
using GEditor.Hub;
using GTool.Graphics;
using GTool.Graphics.Content;
using GTool.Input;
using GTool.Interface;
using GTool.Scene;
using GTool.Utility;
using GTool.Windowing;
using Serilog;
using System.Numerics;

namespace GEditor
{
    internal class GEditor : NativeWindow
    {
        public GraphicsDevice Device { get; private set; }
        public ContentLoader Loader { get; private set; }
        public ContentManager Content { get; private set; }
        public InputManager Input { get; private set; }
        public SceneManager SceneManager { get; private set; }

        private Font _primaryFont;

        private string _projectDir;

        private MenubarVisual _menubar;
        private HierchyVisual _hierchy;
        private FilesystemVisual _filesystem;
        private BottomTabsVisual _bottomTabs;
        private PropertiesVisual _properties;

        public GEditor(in string project, in WindowCreationSettings creationSettings) : base(creationSettings)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            _projectDir = Path.GetFullPath(project);

            Device = new GraphicsDevice(new GraphicsDeviceSettings
            {
                Window = this
            });

            Loader = new ContentLoader();
            ContentLoader.AppendFileSystem("res/Engine");
            ContentLoader.AppendFileSystem("res/Editor");
            ContentLoader.AppendFileSystem(_projectDir);

            Content = new ContentManager();

            Gui.Initialize();
            Gui.Resize(WindowSize);

            Input = new InputManager(this);
            SceneManager = new SceneManager();

            _primaryFont = ContentManager.GetFont("Editor/fonts/poppins.ttf");
            Gui.SetDefaultFont(_primaryFont);

            _menubar = new MenubarVisual();
            _hierchy = new HierchyVisual();
            _filesystem = new FilesystemVisual();
            _bottomTabs = new BottomTabsVisual();
            _properties = new PropertiesVisual();
        }

        public override void Dispose()
        {
            _primaryFont.Dispose();

            SceneManager.Dispose();
            Gui.Dispose();
            Content.Dispose();
            Loader.Dispose();
            Device.Dispose();
            base.Dispose();
        }

        private void OnUpdate()
        {
            
        }

        private void OnRender()
        {
            Gui.RectAB(new Vector4(0.0f, 0.0f, WindowSize.Width, WindowSize.Height), 0xff1e1e1e);

            _properties.Draw(this);
            _bottomTabs.Draw(this);
            _filesystem.Draw(this);
            _hierchy.Draw(this);
            _menubar.Draw(this);
        }

        public void Run()
        {
            while (!IsWindowClosed)
            {
                Input.Update();
                PollEvents();

                OnUpdate();

                OnRender();
                Gui.Render();
                Device.Present();

                Device.FlushMessageQueue();
            }
        }
    }
}
