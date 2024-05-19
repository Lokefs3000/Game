using GTool.Interface;
using GTool.Scene;
using GTool.Scene.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace GEditor.Editor
{
    internal class HierchyVisual
    {
        private Vector2 _treeCursor = Vector2.Zero;

        public void Draw(GEditor editor)
        {
            Vector2 cursor = Vector2.Zero;

            Gui.RectLC(new Vector4(0.0f, 22.0f, 244.0f, 395.0f), 0xff323232);

            Gui.RectLC(new Vector4(2.0f, 25.0f, 240.0f, 30.0f), 0xff272727);
            Gui.RectLC(new Vector4(214.0f, 27.0f, 26.0f, 26.0f), 0xff323232);
            Gui.RectLC(new Vector4(4.0f, 27.0f, 208.0f, 26.0f), 0xff323232);
            Gui.Text("Search..", new Vector2(7.0f, 26.0f), 18.0f, 0xff787878, 2.0f);

            Gui.RectLC(new Vector4(2.0f, 57.0f, 240.0f, 358.0f), 0xff272727);
            Gui.RectLC(new Vector4(4.0f, 59.0f, 236.0f, 354.0f), 0xff2e2e2e);

            _treeCursor = Vector2.Zero;
            foreach (Scene scene in editor.SceneManager.Scenes)
                StartTreeFromScene(scene);
        }

        private void StartTreeFromScene(Scene scene)
        {
            bool hovered = false, held = false;
            bool pressed = Gui.State.ButtonBehaviorLC($"{scene.Name}_ROOTVIEW", new Vector4(6.0f, 61.0f + _treeCursor.Y, 232.0f, 24.0f), ref hovered, ref held);

            Gui.RectLC(new Vector4(6.0f, 61.0f + _treeCursor.Y, 232.0f, 24.0f), 0xff202020);
            Gui.RectLC(new Vector4(8.0f, 63.0f + _treeCursor.Y, 20.0f, 20.0f), 0xffffffff);
            Gui.RectLC(new Vector4(30.0f, 63.0f + _treeCursor.Y, 20.0f, 20.0f), 0xffffffff);
            Gui.Text(scene.Name, new Vector2(54.0f, 63.0f + _treeCursor.Y), 16.0f, 0xffffffff);

            _treeCursor.Y += 26.0f; //2px padding
            _treeCursor.X = 4.0f;

            if (pressed)

            foreach (Actor actor in scene.Actors)
            {
                TreeFromActor(actor);
            }
        }

        private void TreeFromActor(Actor root)
        {


            foreach (Actor actor in root.Children)
            {
                TreeFromActor(actor);
            }
        }
    }
}
