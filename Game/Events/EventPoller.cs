using SDL;
using static SDL.SDL;

namespace Game.Events
{
    internal class EventPoller
    {
        private static readonly SDL_Event _event = new SDL_Event();

        public static unsafe void PollEvents()
        {
            fixed (SDL_Event* ptr = &_event)
            {
                while (SDL_PollEvent(ptr))
                {
                    switch (_event.type)
                    {
                        case SDL_EventType.WindowCloseRequested:
                            OnWindowClose?.Invoke();
                            break;
                    }
                }
            }
        }

        public delegate void WindowClose();
        public static event WindowClose? OnWindowClose;
    }
}
