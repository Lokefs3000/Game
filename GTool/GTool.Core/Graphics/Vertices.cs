using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Graphics
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector4 UV; //XY = Offset, ZW = Limit
        public Vector3 Normal;
    }

    public struct GuiVertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public uint Color;
    }
}
