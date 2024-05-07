using Game.Assets;
using Game.Events;
using SDL;
using System.Numerics;
using static SDL.SDL;

namespace Game.Graphics
{
    internal class CoreWindow : IDisposable
    {
        private SDL_Window _window;

        public bool ShouldWindowClose { get => _closeRequested; }
        private bool _closeRequested;

        public Vector2 Size { get => _size; }
        private Vector2 _size;

        public CoreWindow(int width, int height, string title)
        {
            SDL_Init(SDL_InitFlags.Video);
            _window = SDL_CreateWindow(title, width, height, SDL_WindowFlags.Resizable);
            _size = new Vector2(width, height);

            _closeRequested = false;

            EventPoller.OnWindowClose += () => { _closeRequested = true; };

            LoadIcon();
        }

        public void Dispose()
        {
            SDL_DestroyWindow(_window);
            SDL_Quit();
        }

        private nint GetNativePointer()
        {
            if (OperatingSystem.IsWindows())
                return SDL_GetProperty(SDL_GetWindowProperties(_window), SDL_PROP_WINDOW_WIN32_HWND_POINTER);
            else
                throw new NotSupportedException("Only windows is supported!");
        }

        public nint NativeWindowPointer => GetNativePointer();

        private unsafe void LoadIcon()
        {
            byte[] source = ContentLoader.ReadBytes("win_icon");

            SDL_Surface* surface = SDL_CreateSurfaceFrom(, 150, 150, 4, SDL_PixelFormatEnum.Rgba32);
            SDL_SetWindowIcon(_window, surface);
            SDL_DestroySurface(surface);
        }
    }
}
