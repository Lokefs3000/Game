using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Graphics.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector4 UV; //XY = Offset, ZW = Limit
        public Vector3 Normal;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GuiVertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public uint Color;
        public float Attribute;
    }
}
