using Game.Assets;
using Game.Config;
using Game.Events;
using Game.Graphics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;

namespace Game.Core
{
    internal class GameClass : IDisposable
    {
        private ContentLoader _contentLoader;
        private CoreWindow _window;
        private GraphicsDeviceManager _deviceManager;

        public GameClass()
        {
            #region CreateLogger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Warning()
#endif
                .CreateLogger();
            #endregion

            Log.Information("Initializing game..");

            _contentLoader = new ContentLoader();
            _window = new CoreWindow(StartupConfig.Config.WindowWidth, StartupConfig.Config.WindowHeight, "Game");
            _deviceManager = new GraphicsDeviceManager(_window);
        }

        public void Dispose()
        {
            Log.Information("Disposing game..");

            _deviceManager.Dispose();
            _window.Dispose();
            _contentLoader.Dispose();

            Log.CloseAndFlush();
            GC.SuppressFinalize(this);
        }

        public void Run()
        {
            while (!_window.ShouldWindowClose)
            {
                EventPoller.PollEvents();

                _deviceManager.BindAndClearBackBuffer(new Color4(1.0f, 0.0f, 0.3f, 1.0f));

                _deviceManager.SubmitAndPresent();
            }
        }
    }
}
