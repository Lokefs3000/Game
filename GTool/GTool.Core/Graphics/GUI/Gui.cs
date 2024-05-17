using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GTool.Content;
using GTool.Graphics.Content;
using GTool.Graphics.Data;
using Serilog;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace GTool.Graphics.GUI
{
    public static class Gui
    {
        private static GuiVertex[] _vertices = new GuiVertex[1];
        private static int _vertexArrayIdx = 0;

        private static ushort[] _indices = new ushort[1];
        private static int _indexArrayIdx = 0;

        private static Buffer<GuiVertex> _vertexBuffer;
        private static Buffer<ushort> _indexBuffer;

        private static Shader _shader;

        private static Buffer<CB> _constantBuffer;
        private static CB _constantBufferData;

        internal static void Initialize()
        {
            _vertexBuffer = new Buffer<GuiVertex>(_vertices.Length, BindFlags.VertexBuffer);
            _indexBuffer = new Buffer<ushort>(_indices.Length, BindFlags.VertexBuffer);
            _shader = ContentManager.GetOrCreateShader("GTool/Content/shaders/gui.hlsl");

            _constantBuffer = new Buffer<CB>(1, BindFlags.ConstantBuffer);
            _constantBufferData = new CB();

            if (!_shader.IsValid)
                Log.Fatal("Invalid shader in gui!");
        }

        internal static void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _shader.Dispose();
        }

        internal static void Resize(Size size)
        {
            _constantBufferData.Projection = Matrix4x4.CreateOrthographic(size.Width, size.Height, -1.0f, 1.0f);
            _constantBuffer.Update([_constantBufferData]);
        }

        internal static void Render()
        {
            if (_vertexArrayIdx == 0 || _indexArrayIdx == 0 || !_shader.IsValid)
                return;

            if (_vertexBuffer.Size < _vertices.Length)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = new Buffer<GuiVertex>(_vertices.Length, BindFlags.VertexBuffer);
            }
            if (_indexBuffer.Size < _indices.Length)
            {
                _indexBuffer.Dispose();
                _indexBuffer = new Buffer<ushort>(_indices.Length, BindFlags.VertexBuffer);
            }


            _vertexBuffer.Update(_vertices);
            _indexBuffer.Update(_indices);

            GraphicsDevice.RestoreDefaultDrawViews();
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.SetIndexBuffer(_indexBuffer, Format.R16_UInt);
            GraphicsDevice.VSetConstantBuffer(_constantBuffer);
            GraphicsDevice.SetTopology(PrimitiveTopology.TriangleList);

            _shader.Bind();
            GraphicsDevice.DrawIndexed(_indexArrayIdx, 0, 0);

            _vertexArrayIdx = 0;
            _indexArrayIdx = 0;
        }

        private static void AppendVertex(GuiVertex v)
        {
            if (_vertexArrayIdx >= _vertices.Length) //resize
            {
                GuiVertex[] newArray = new GuiVertex[_vertices.Length * 2];
                Array.Copy(_vertices, newArray, _vertices.Length);
                _vertices = newArray;
            }

            _vertices[_vertexArrayIdx++] = v;
        }

        private static void AppendIndex(ushort v)
        {
            if (_indexArrayIdx >= _indices.Length) //resize
            {
                ushort[] newArray = new ushort[_indices.Length * 2];
                Array.Copy(_indices, newArray, _indices.Length);
                _indices = newArray;
            }

            _indices[_indexArrayIdx++] = v;
        }

        public static void Rect(Vector4 rect, uint color)
        {
            AppendIndex((ushort)(0 + _vertexArrayIdx));
            AppendIndex((ushort)(2 + _vertexArrayIdx));
            AppendIndex((ushort)(1 + _vertexArrayIdx));
            AppendIndex((ushort)(2 + _vertexArrayIdx));
            AppendIndex((ushort)(1 + _vertexArrayIdx));
            AppendIndex((ushort)(3 + _vertexArrayIdx));

            AppendVertex(new GuiVertex { Position = new Vector2(rect.X, rect.Y), UV = Vector2.Zero, Color = color });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.Z, rect.Y), UV = Vector2.UnitY, Color = color });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.X, rect.W), UV = Vector2.UnitX, Color = color });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.Z, rect.W), UV = Vector2.One, Color = color });
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CB
        {
            public Matrix4x4 Projection;
        }
    }
}
