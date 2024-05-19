using GEditor.Content;
using GEditor.Hub;
using GTool.Graphics;
using GTool.Graphics.Content;
using GTool.Input;
using GTool.Interface;
using GTool.Utility;
using GTool.Windowing;
using Serilog;
using System.Numerics;

namespace GEditor
{
    internal class GHub : NativeWindow
    {
        public GraphicsDevice Device { get; private set; }
        public ContentLoader Loader { get; private set; }
        public ContentManager Content { get; private set; }
        public InputManager Input { get; private set; }

        private Font _primaryFont;

        private NewProjectDialog _newProjectDialog;

        public static GHub Instance { get; private set; }

        public GHub(in WindowCreationSettings creationSettings) : base(creationSettings)
        {
            Instance = this;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Device = new GraphicsDevice(new GraphicsDeviceSettings
            {
                Window = this
            });

            Loader = new ContentLoader();
            ContentLoader.AppendFileSystem("res/Engine");
            ContentLoader.AppendFileSystem("res/Editor");

            Content = new ContentManager();

            Gui.Initialize();
            Gui.Resize(WindowSize);

            Input = new InputManager(this);

            _primaryFont = ContentManager.GetFont("Editor/fonts/poppins.ttf");
            Gui.SetDefaultFont(_primaryFont);

            _newProjectDialog = new NewProjectDialog();
        }

        public override void Dispose()
        {
            _primaryFont.Dispose();

            Gui.Dispose();
            Content.Dispose();
            Loader.Dispose();
            Device.Dispose();
            base.Dispose();
        }

        private bool _isHoldingNew;
        private bool _isHoldingOpen;

        private void OnUpdate()
        {
            Vector2 half = WindowSize.ToVector2() / 2.0f;

            Vector4 newRect = new Vector4(half.X - 150.0f, half.Y - 40.0f, half.X - 10.0f, half.Y - 10.0f);
            bool isHoveringNew = HitUtility.IsWithinRect(Input.Mouse - half, newRect) && !_newProjectDialog.Show;

            Vector4 openRect = new Vector4(half.X - 310.0f, half.Y - 40.0f, half.X - 160.0f, half.Y - 10.0f);
            bool isHoveringOpen = HitUtility.IsWithinRect(Input.Mouse - half, openRect) && !_newProjectDialog.Show;

            if (_isHoldingNew || (isHoveringNew && Input.IsMouseDown(InputManager.MouseButton.Left)))
                _isHoldingNew = Input.IsMouseDown(InputManager.MouseButton.Left) && !_isHoldingOpen && !_newProjectDialog.Show;
            else if (_isHoldingOpen || (isHoveringOpen && Input.IsMouseDown(InputManager.MouseButton.Left)))
                _isHoldingOpen = Input.IsMouseDown(InputManager.MouseButton.Left) && !_isHoldingNew && !_newProjectDialog.Show;

            if (_isHoldingNew && Input.IsMouseDown(InputManager.MouseButton.Left))
                _newProjectDialog.Show = true;

            Gui.RectAB(new Vector4(-half.X, -half.Y, half.X, half.Y), 0xff222222);
            Gui.RectAB(new Vector4(-half.X + 10.0f, -half.Y + 10.0f, half.X - 10.0f, half.Y - 50.0f), 0xff111111);
            Gui.RectAB(newRect, DecideColorIncrease(0xff8b500b, 0x00202020, isHoveringNew, _isHoldingNew));
            Gui.RectAB(openRect, DecideColorIncrease(0xff333333, 0x0202020, isHoveringOpen, _isHoldingOpen));

            Gui.Text("Projects", new Vector2(-half.X + 10.0f, half.Y - 37.5f), 35, 0xffffffff);
            Gui.Text("Open", new Vector2(half.X - 270.0f, half.Y - 33.5f), 25, 0xffffffff);
            Gui.Text("New", new Vector2(half.X - 107.0f, half.Y - 33.5f), 25, 0xffffffff);

            _newProjectDialog.Update(half);
        }

        public static uint DecideColorIncrease(uint baseColor, uint incColor, bool hovered, bool held)
        {
            return baseColor + (hovered || held ? incColor : uint.MinValue) + (held ? incColor : uint.MaxValue);
        }

        private void OnRender()
        {
            
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
