using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Graphics.GUI
{
    public static class Gui
    {
        private static GuiVertex[] _vertices = new GuiVertex[1];
        private static int _vertexArrayIdx = 0;

        private static ushort[] _indices = new ushort[1];
        private static int _indexArrayIdx = 0;

        internal static void Initialize()
        {

        }

        internal static void Dispose()
        {

        }

        internal static void Render()
        {
            _vertexArrayIdx = 0;
            _indexArrayIdx = 0;
        }

        private static void AppendVertex(GuiVertex v)
        {
            if (_vertexArrayIdx >= _vertices.Length) //resize
            {
                GuiVertex[] newArray = new GuiVertex[(int)(_vertices.Length * 1.5f)];
                Array.Copy(_vertices, newArray, _vertices.Length);
                _vertices = newArray;
            }

            _vertices[_vertexArrayIdx++] = v;
        }

        private static void AppendIndex(ushort v)
        {
            if (_indexArrayIdx >= _indices.Length) //resize
            {
                ushort[] newArray = new ushort[(int)(_indices.Length * 1.5f)];
                Array.Copy(_indices, newArray, _indices.Length);
                _indices = newArray;
            }

            _indices[_indexArrayIdx++] = v;
        }

        public static void Rect(Vector4 rect, uint color)
        {
            AppendVertex(new GuiVertex { Position = new Vector2(rect.X, rect.Y), UV = Vector2.Zero, Color = color });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.Z, rect.Y), UV = Vector2.UnitY, Color = color });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.X, rect.W), UV = Vector2.UnitX, Color = color });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.Z, rect.W), UV = Vector2.One, Color = color });

            AppendIndex(0);
            AppendIndex(2);
            AppendIndex(1);
            AppendIndex(2);
            AppendIndex(1);
            AppendIndex(3);
        }
    }
}
