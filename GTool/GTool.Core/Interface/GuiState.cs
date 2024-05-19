using GTool.Input;
using GTool.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Interface
{
    public class GuiState
    {
        private Dictionary<string, ButtonState> _buttonState;
        private Dictionary<string, TreeState> _treeState;

        internal GuiState()
        {

        }

        public bool ButtonBehaviorAB(string ID, Vector4 rect, ref bool hovered, ref bool held)
        {
            hovered = HitUtility.IsWithinRect(InputManager.Instance.Mouse, rect);
            if (!_buttonState.TryGetValue(ID, out ButtonState buttonState))
            {
                buttonState = new ButtonState();
                _buttonState.Add(ID, buttonState);
            }

            bool lmouse = InputManager.Instance.IsMouseDown(InputManager.MouseButton.Left);
            if (buttonState.WasHeld || (hovered && lmouse))
                held = lmouse;

            bool triggered = buttonState.WasHeld && InputManager.Instance.IsMouseReleased(InputManager.MouseButton.Left);
            buttonState.WasHeld = true;

            return triggered;
        }
        public bool ButtonBehaviorLC(string ID, Vector4 rect, ref bool hovered, ref bool held) => ButtonBehaviorAB(ID, new Vector4(rect.X, rect.Y, rect.X + rect.Z, rect.Y + rect.W), ref hovered, ref held);

        private struct ButtonState
        {
            public bool WasHeld;
        }
    }
}
