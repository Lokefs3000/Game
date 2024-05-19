using GTool.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GEditor.Editor
{
    internal class FilesystemVisual
    {
        public void Draw(GEditor editor)
        {
            Gui.RectLC(new Vector4(0.0f, 415.0f, 244.0f, 305.0f), 0xff323232);

            Gui.RectLC(new Vector4(2.0f, 417.0f, 240.0f, 30.0f), 0xff272727);
            Gui.RectLC(new Vector4(214.0f, 419.0f, 26.0f, 26.0f), 0xff323232);
            Gui.RectLC(new Vector4(4.0f, 419.0f, 208.0f, 26.0f), 0xff323232);
            Gui.Text("Search..", new Vector2(6.0f, 418.0f), 18.0f, 0xff787878, 2.0f);

            Gui.RectLC(new Vector4(2.0f, 449.0f, 240.0f, 269.0f), 0xff272727);
            Gui.RectLC(new Vector4(4.0f, 451.0f, 236.0f, 265.0f), 0xff2e2e2e);
        }
    }
}
