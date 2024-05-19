using GTool.Graphics;
using GTool.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;

namespace GTool.Windowing
{
    public class NativeWindow : IDisposable
    {
        private static IWindowProtocol? _protocol;
        private bool _disposed;

        private nint _nativeWindow;
        public nint Pointer { get => _nativeWindow; }

        private bool _isWindowClosed = false;
        public bool IsWindowClosed { get => _isWindowClosed; }

        public Size WindowSize;

        public NativeWindow(in WindowCreationSettings creationSettings)
        {
            if (_protocol == null)
            {
                if (OperatingSystem.IsWindows())
                    _protocol = new Win32Protocol();
                else
                    throw new NotSupportedException("Operating system window not supported!");
            }

            WindowSize = new Size(creationSettings.Width, creationSettings.Height);

            _nativeWindow = _protocol.CreateWindow(creationSettings);

            _protocol.BindClosed(WindowClosedEv);
            _protocol.WindowResize += OnResize;
        }

        private void OnResize(float width, float height)
        {
            WindowSize = new Size(width, height);

            GraphicsDevice.Resize(WindowSize);
            Gui.Resize(WindowSize);
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                if (_protocol != null)
                {
                    _protocol.UnbindClosed(WindowClosedEv);
                    _protocol.WindowResize -= OnResize;

                    _protocol.DestroyWindow(_nativeWindow);
                }

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public static void PollEvents() => _protocol?.ProcessWindows();

        private void WindowClosedEv(nint window)
        {
            if (window == _nativeWindow)
                _isWindowClosed = true;
        }

        internal static IWindowProtocol? GetProtocol() => _protocol;
    }
}
