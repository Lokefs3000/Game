using GTool.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GEditor.Editor
{
    internal class MenubarVisual
    {
        public void Draw(GEditor editor)
        {
            Gui.RectAB(new Vector4(0.0f, 0.0f, editor.WindowSize.Width, 25.0f), 0xff323232);

            Gui.Text("File", new Vector2(5, 0), 16.5f, 0xffffffff);
            Gui.Text("View", new Vector2(46, 0), 16.5f, 0xffffffff);
            Gui.Text("Tools", new Vector2(99, 0), 16.5f, 0xffffffff);
            Gui.Text("Windows", new Vector2(156, 0), 16.5f, 0xffffffff);

            Gui.RectAB(new Vector4(622.0f, 4.0f, 17.0f, 17.0f), 0xff444444);
            Gui.RectAB(new Vector4(641.0f, 4.0f, 17.0f, 17.0f), 0xff444444);

            Gui.Text("FPS: 75 (0.013ms)", new Vector2(1026, 2), 16.5f, 0xff888888, 2.0f);
        }
    }
}
