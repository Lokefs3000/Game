using Game.Events;
using Game.Graphics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core
{
    internal class GameClass : IDisposable
    {
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

            _window = new CoreWindow(1280, 720, "Game");
            _deviceManager = new GraphicsDeviceManager(_window);
        }

        public void Dispose()
        {
            Log.Information("Disposing game..");

            _deviceManager.Dispose();
            _window.Dispose();

            Log.CloseAndFlush();
            GC.SuppressFinalize(this);
        }

        public void Run()
        {
            while (!_window.ShouldWindowClose)
            {
                EventPoller.PollEvents();
                _deviceManager.SubmitAndPresent();
            }
        }
    }
}
