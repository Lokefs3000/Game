using GTool.Graphics.GUI;
using GTool.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics.PackedVector;

namespace Game.Editor
{
    internal class Hierchy
    {
        public void Render(EdApp app)
        {
            float hw = app.WindowSize.Width * 0.5f;
            float hh = app.WindowSize.Height * 0.5f;

            float w = -hw + hw * 0.35f;
            Gui.Rect(new Vector4(-hw, hh - 24.0f, w, 0.0f), 0xff292929);

            Gui.Rect(new Vector4(-hw, hh - 24.0f, -hw + 2.0f, 0.0f), 0xff666666);
            Gui.Rect(new Vector4(w - 2.0f, hh - 24.0f, w, 0.0f), 0xff666666);
            Gui.Rect(new Vector4(-hw, hh - 24.0f, w, hh - 26.0f), 0xff666666);
            Gui.Rect(new Vector4(-hw, -2.0f, w, 0.0f), 0xff666666);

            foreach (Scene scene in app.SceneManager.Scenes)
            {
                Gui.Rect(new Vector4(-hw + 2.0f, hh - 26.0f, w - 2.0f, hh - 54.0f), 0xff191919);
            }
        }
    }
}
