using GTool.Interface;
using System.Numerics;

namespace GEditor.Editor
{
    internal class BottomTabsVisual
    {
        public int TabId = 0;

        public void Draw(GEditor editor)
        {
            Gui.RectLC(new Vector4(242.0f, 553.0f, 784.0f, 167.0f), 0xff323232);

            Gui.RectLC(new Vector4(244.0f, 555.0f, 780.0f, 24.0f), 0xff272727);
            Gui.RectLC(new Vector4(246.0f, 557.0f, 100.0f, 20.0f), 0xff4b4b4b);
            Gui.RectLC(new Vector4(348.0f, 557.0f, 100.0f, 20.0f), 0xff2e2e2e);

            Gui.TextC("Output", new Vector4(246.0f, 557.0f, 100.0f, 20.0f), 15.0f, 0xffffffff);
            Gui.TextC("Memory", new Vector4(348.0f, 557.0f, 100.0f, 20.0f), 15.0f, 0xffffffff);
        }
    }
}
