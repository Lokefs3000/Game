using GTool.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Input
{
    public class InputManager
    {
        public static InputManager Instance { get; private set; }

        private Vector2 _mouseLocation = Vector2.Zero;
        public Vector2 Mouse { get { return _mouseLocation; } }

        private bool[] _mouseBtnStates = new bool[(int)MouseButton.Count];
        private bool[] _mouseBtnReleased = new bool[(int)MouseButton.Count];

        private NativeWindow _attached;

        public InputManager(NativeWindow window)
        {
            Instance = this;

            _attached = window;

            IWindowProtocol? protocol = NativeWindow.GetProtocol();
            if (protocol != null)
            {
                protocol.MouseMove += Protocol_MouseMove;
                protocol.MouseButton += Protocol_MouseButton;
            }
        }

        public void Update()
        {
            _mouseBtnReleased[(int)MouseButton.Left] = false;
            _mouseBtnReleased[(int)MouseButton.Middle] = false;
            _mouseBtnReleased[(int)MouseButton.Right] = false;
        }

        private void Protocol_MouseMove(float x, float y)
        {
            _mouseLocation.X = x;
            _mouseLocation.Y = _attached.WindowSize.Height - y;
        }

        private void Protocol_MouseButton(MouseButton which, bool state)
        {
            if (_mouseBtnStates[(int)which] && !state)
                _mouseBtnReleased[(int)which] = true;
            _mouseBtnStates[(int)which] = state;
        }

        public bool IsMouseDown(MouseButton button) => _mouseBtnStates[(int)button];
        public bool IsMouseUp(MouseButton button) => !_mouseBtnStates[(int)button];
        public bool IsMouseReleased(MouseButton button) => !_mouseBtnReleased[(int)button];

        public enum MouseButton
        {
            Left = 0,
            Middle,
            Right,
            Count
        }
    }
}
