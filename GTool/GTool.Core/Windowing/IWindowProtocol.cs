using GTool.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Windowing
{
    internal interface IWindowProtocol
    {
        public nint CreateWindow(in WindowCreationSettings settings);
        public void DestroyWindow(nint window);
        public void ProcessWindows();

        public void BindClosed(WindowEvent @event);
        public void UnbindClosed(WindowEvent @event);

        public event WindowResizeEvent? WindowResize;

        public event MouseMoveEvent? MouseMove;
        public event MouseButtonEvent? MouseButton;

        public delegate void WindowEvent(nint window);
        public delegate void WindowResizeEvent(float width, float height);
        public delegate void MouseMoveEvent(float x, float y);
        public delegate void MouseButtonEvent(InputManager.MouseButton button, bool state);
    }

    public struct WindowCreationSettings
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public string Title;
        public WindowCreationFlags Flags;
        public nint Parent;
    }

    public enum WindowCreationFlags
    {
        None = 0,
        Resizable = (1<<1),
        Fullscreen = (1<<2),
    }
}
