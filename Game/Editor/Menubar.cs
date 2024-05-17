using GTool.Graphics.GUI;
using System.Numerics;

namespace Game.Editor
{
    internal class Menubar
    {
        public void Render(EdApp app)
        {
            float hw = app.WindowSize.Width * 0.5f;
            float hh = app.WindowSize.Height * 0.5f;

            Gui.Rect(new Vector4(-hw, -hh, hw, hh), 0xff111111);
            Gui.Rect(new Vector4(-hw, hh, hw, hh - 24.0f), 0xff292929);
            Gui.Rect(new Vector4(-hw, hh - 24.0f, hw, hh - 26.0f), 0xff666666);
        }
    }
}
