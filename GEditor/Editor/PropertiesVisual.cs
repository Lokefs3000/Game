using GTool.Interface;
using System.Numerics;

namespace GEditor.Editor
{
    internal class PropertiesVisual
    {
        public void Draw(GEditor editor)
        {
            Gui.RectLC(new Vector4(1024.0f, 25.0f, 256.0f, 695.0f), 0xff323232);

            Gui.RectLC(new Vector4(1026.0f, 25.0f, 252.0f, 50.0f), 0xff272727);
            Gui.RectLC(new Vector4(1184.0f, 55.0f, 92.0f, 18.0f), 0xff323232);
            Gui.RectLC(new Vector4(1055.0f, 55.0f, 92.0f, 18.0f), 0xff323232);

            Gui.Text("Untagged", new Vector2(1057.0f, 59.0f), 12.0f, 0xffffffff);
            Gui.Text("Default", new Vector2(1186.0f, 59.0f), 12.0f, 0xffffffff);

            Gui.Text("Tag:", new Vector2(1028.0f, 59.0f), 12.0f, 0xffffffff);
            Gui.Text("Layer:", new Vector2(1148.0f, 59.0f), 12.0f, 0xffffffff);

            Gui.RectLC(new Vector4(1250.0f, 27.0f, 26.0f, 26.0f), 0xff1b1b1b);
            Gui.RectLC(new Vector4(1251.0f, 28.0f, 24.0f, 24.0f), 0xff323232);
            Gui.RectLC(new Vector4(1028.0f, 28.0f, 170.0f, 26.0f), 0xff323232);

            Gui.Text("Static:", new Vector2(1200.0f, 30.5f), 15.5f, 0xffffffff);
            Gui.Text("Name..", new Vector2(1033.0f, 30.5f), 15.5f, 0xff787878, 2.0f);
        }
    }
}
