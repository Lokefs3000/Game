using System.Numerics;
using System.Runtime.InteropServices;
using GTool.Content;
using GTool.Graphics;
using GTool.Graphics.Content;
using GTool.Graphics.Data;
using Serilog;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace GTool.Interface
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
        private static Font? _font;

        private static Buffer<CB> _constantBuffer;
        private static CB _constantBufferData;

        private static Vector2 _fullWindowSize;
        private static Vector2 _halfWindowSize;

        private static BlendState _blendState;

        public static readonly GuiState State = new GuiState();

        public static void Initialize()
        {
            _vertexBuffer = new Buffer<GuiVertex>(_vertices.Length, BindFlags.VertexBuffer);
            _indexBuffer = new Buffer<ushort>(_indices.Length, BindFlags.IndexBuffer);
            _shader = ContentManager.GetShader("Engine/shaders/gui.hlsl");

            _constantBuffer = new Buffer<CB>(1, BindFlags.ConstantBuffer);
            _constantBufferData = new CB();

            _blendState = new BlendState(Blend.SourceAlpha, Blend.One, Blend.InverseSourceAlpha, Blend.Zero, BlendOperation.Add, BlendOperation.Add);

            if (!_shader.IsValid)
                Log.Fatal("Invalid shader in gui!");
        }

        public static void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _shader.Dispose();
            _font?.Dispose();
        }

        public static void Resize(Size size)
        {
            Vector2 divSize = size.ToVector2() / new Vector2(1280.0f, 720.0f);
            Vector2 relSize = size.ToVector2() / divSize;

            _constantBufferData.Projection = Matrix4x4.CreateOrthographic(relSize.X, relSize.Y, -1.0f, 1.0f);
            _constantBuffer.Update([_constantBufferData]);

            _fullWindowSize = relSize;
            _halfWindowSize = _fullWindowSize / 2.0f;
        }

        public static void Render()
        {
            if (_vertexArrayIdx == 0 || _indexArrayIdx == 0 || _shader == null || !_shader.IsValid)
                return;

            if (_vertexBuffer.Size < _vertices.Length)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = new Buffer<GuiVertex>(_vertices.Length, BindFlags.VertexBuffer);
            }
            if (_indexBuffer.Size < _indices.Length)
            {
                _indexBuffer.Dispose();
                _indexBuffer = new Buffer<ushort>(_indices.Length, BindFlags.IndexBuffer);
            }


            _vertexBuffer.Update(_vertices);
            _indexBuffer.Update(_indices);

            GraphicsDevice.RestoreDefaultDrawViews();
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.SetIndexBuffer(_indexBuffer, Format.R16_UInt);
            GraphicsDevice.VSetConstantBuffer(_constantBuffer);
            GraphicsDevice.SetTopology(PrimitiveTopology.TriangleList);

            _font?.Bind();
            _shader.Bind();
            _blendState.Bind();

            GraphicsDevice.DrawIndexed(_indexArrayIdx, 0, 0);

            _vertexArrayIdx = 0;
            _indexArrayIdx = 0;
        }

        public static void SetDefaultFont(Font font)
        {
            _font = new Font(font.Data);
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

        public static void RectAB(Vector4 rect, uint color)
        {
            AppendIndex((ushort)(1 + _vertexArrayIdx));
            AppendIndex((ushort)(2 + _vertexArrayIdx));
            AppendIndex((ushort)(0 + _vertexArrayIdx));
            AppendIndex((ushort)(2 + _vertexArrayIdx));
            AppendIndex((ushort)(1 + _vertexArrayIdx));
            AppendIndex((ushort)(3 + _vertexArrayIdx));

            AppendVertex(new GuiVertex { Position = new Vector2(rect.X - _halfWindowSize.X, _fullWindowSize.Y - rect.Y - _halfWindowSize.Y), UV = Vector2.Zero, Color = color, Attribute = 0.0f });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.Z - _halfWindowSize.X, _fullWindowSize.Y - rect.Y - _halfWindowSize.Y), UV = Vector2.UnitY, Color = color, Attribute = 0.0f });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.X - _halfWindowSize.X, _fullWindowSize.Y - rect.W - _halfWindowSize.Y), UV = Vector2.UnitX, Color = color, Attribute = 0.0f });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.Z - _halfWindowSize.X, _fullWindowSize.Y - rect.W - _halfWindowSize.Y), UV = Vector2.One, Color = color, Attribute = 0.0f });
        }

        public static void RectLC(Vector4 rect, uint color)
        {
            AppendIndex((ushort)(1 + _vertexArrayIdx));
            AppendIndex((ushort)(2 + _vertexArrayIdx));
            AppendIndex((ushort)(0 + _vertexArrayIdx));
            AppendIndex((ushort)(2 + _vertexArrayIdx));
            AppendIndex((ushort)(1 + _vertexArrayIdx));
            AppendIndex((ushort)(3 + _vertexArrayIdx));

            float fx = rect.X + rect.Z;
            float fy = rect.Y + rect.W;

            AppendVertex(new GuiVertex { Position = new Vector2(rect.X - _halfWindowSize.X, _fullWindowSize.Y - rect.Y - _halfWindowSize.Y), UV = Vector2.Zero, Color = color, Attribute = 0.0f });
            AppendVertex(new GuiVertex { Position = new Vector2(fx - _halfWindowSize.X, _fullWindowSize.Y - rect.Y - _halfWindowSize.Y), UV = Vector2.UnitY, Color = color, Attribute = 0.0f });
            AppendVertex(new GuiVertex { Position = new Vector2(rect.X - _halfWindowSize.X, _fullWindowSize.Y - fy - _halfWindowSize.Y), UV = Vector2.UnitX, Color = color, Attribute = 0.0f });
            AppendVertex(new GuiVertex { Position = new Vector2(fx - _halfWindowSize.X, _fullWindowSize.Y - fy - _halfWindowSize.Y), UV = Vector2.One, Color = color, Attribute = 0.0f });
        }

        public static void Text(string text, Vector2 position, float size, uint color, float skew = 0.0f, Font? font = null)
        {
            if (font == null)
                font = _font;
            if (font == null || font.Data == null)
                return;

            float scale = size * 0.015625f;
            float sizeScaled = size * scale;

            position.X -= _halfWindowSize.X;
            position.Y = _fullWindowSize.Y - position.Y - _halfWindowSize.Y - sizeScaled * 4.0f;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    position.X += sizeScaled;
                    continue;
                }

                Font.Glyph gl = font.Data.Glyphs[text[i]];

                float x = position.X + gl.Bearing.X * scale;
                float y = position.Y - (gl.Size.Y - gl.Bearing.Y) * scale;

                float w = gl.Size.X * scale;
                float h = gl.Size.Y * scale;

                AppendIndex((ushort)(0 + _vertexArrayIdx));
                AppendIndex((ushort)(2 + _vertexArrayIdx));
                AppendIndex((ushort)(1 + _vertexArrayIdx));
                AppendIndex((ushort)(3 + _vertexArrayIdx));
                AppendIndex((ushort)(1 + _vertexArrayIdx));
                AppendIndex((ushort)(2 + _vertexArrayIdx));

                AppendVertex(new GuiVertex { Position = new Vector2(x, y), UV = new Vector2(gl.UV.X, gl.UV.W), Color = color, Attribute = 2.0f });
                AppendVertex(new GuiVertex { Position = new Vector2(x + w, y), UV = new Vector2(gl.UV.Z, gl.UV.W), Color = color, Attribute = 2.0f });
                AppendVertex(new GuiVertex { Position = new Vector2(x + skew, y + h), UV = new Vector2(gl.UV.X, gl.UV.Y), Color = color, Attribute = 2.0f });
                AppendVertex(new GuiVertex { Position = new Vector2(x + w + skew, y + h), UV = new Vector2(gl.UV.Z, gl.UV.Y), Color = color, Attribute = 2.0f });

                position.X += gl.Advance * scale;
            }
        }

        public static void TextC(string text, Vector4 position, float size, uint color, float skew = 0.0f, Font? font = null)
        {
            if (font == null)
                font = _font;
            if (font == null || font.Data == null)
                return;

            float scale = size * 0.015625f;
            float sizeScaled = size * scale;

            Vector2 tsize = TextSize(text, size, font);
            position.X += (position.Z - tsize.X) * 0.5f;

            position.X -= _halfWindowSize.X;
            position.Y = _fullWindowSize.Y - position.Y - _halfWindowSize.Y - sizeScaled * 4.0f;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    position.X += sizeScaled;
                    continue;
                }

                Font.Glyph gl = font.Data.Glyphs[text[i]];

                float x = position.X + gl.Bearing.X * scale;
                float y = position.Y - (gl.Size.Y - gl.Bearing.Y) * scale;

                float w = gl.Size.X * scale;
                float h = gl.Size.Y * scale;

                AppendIndex((ushort)(0 + _vertexArrayIdx));
                AppendIndex((ushort)(2 + _vertexArrayIdx));
                AppendIndex((ushort)(1 + _vertexArrayIdx));
                AppendIndex((ushort)(3 + _vertexArrayIdx));
                AppendIndex((ushort)(1 + _vertexArrayIdx));
                AppendIndex((ushort)(2 + _vertexArrayIdx));

                AppendVertex(new GuiVertex { Position = new Vector2(x, y), UV = new Vector2(gl.UV.X, gl.UV.W), Color = color, Attribute = 2.0f });
                AppendVertex(new GuiVertex { Position = new Vector2(x + w, y), UV = new Vector2(gl.UV.Z, gl.UV.W), Color = color, Attribute = 2.0f });
                AppendVertex(new GuiVertex { Position = new Vector2(x + skew, y + h), UV = new Vector2(gl.UV.X, gl.UV.Y), Color = color, Attribute = 2.0f });
                AppendVertex(new GuiVertex { Position = new Vector2(x + w + skew, y + h), UV = new Vector2(gl.UV.Z, gl.UV.Y), Color = color, Attribute = 2.0f });

                position.X += gl.Advance * scale;
            }
        }

        public static Vector2 TextSize(string text, float size, Font? font = null)
        {
            if (font == null)
                font = _font;
            if (font == null || font.Data == null)
                return Vector2.Zero;

            float scale = size * 0.015625f;
            float sizeScaled = size * scale;

            float px = 0.0f;
            float py = 0.0f;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    px += sizeScaled;
                    continue;
                }

                Font.Glyph gl = font.Data.Glyphs[text[i]];

                float w = gl.Size.X * scale;
                float h = gl.Size.Y * scale;

                px += gl.Advance * scale;
                py = Math.Max(py, h);
            }

            return new Vector2(px, py);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CB
        {
            public Matrix4x4 Projection;
        }
    }
}
