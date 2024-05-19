using GTool.Content;
using GTool.Graphics;
using GTool.Graphics.Content;
using RectpackSharp;
using Serilog;
using SharpFont;
using System.Numerics;
using System.Text;
using Vortice.D3DCompiler;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace GEditor.Content
{
    internal class ContentManager : GTool.Content.ContentManager
    {
        public ContentManager() : base()
        {
            _instance = this;
        }

        protected override Shader GetOrCreateShader(string path)
        {
            if (_assets.TryGetValue(path, out IVirtualAsset? shader))
            {
                if (shader == null || shader?.Type != AssetType.Shader)
                    throw new InvalidOperationException("Invalid virtual asset handle!");
                else
                    return new Shader((Shader.ShaderDataAsset?)shader);
            }

            string source = ContentLoader.GetString(path);

            Shader.ShaderDataAsset data = new Shader.ShaderDataAsset();
            data.Name = path;

            if (source.Length > 0)
            {
                Log.Debug($"Creating asset: \"{path}\"");

#if DEBUG
                ShaderFlags flags = ShaderFlags.Debug | ShaderFlags.OptimizationLevel1;
#else
                ShaderFlags flags = ShaderFlags.OptimizationLevel2;
#endif

                List<InputElementDescription> inputs = new List<InputElementDescription>();
                {
                    string[] lines = source.Split('\n');
                    StringBuilder newSource = new StringBuilder();

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (line.StartsWith("##"))
                        {
                            string trimmed = line.Substring(2);
                            string[] keys = trimmed.Split(' ');

                            if (keys.Length != 3)
                            {
                                newSource.AppendLine(line);
                                continue;
                            }

                            InputElementDescription input = new InputElementDescription("", 0, Format.Unknown, 0);

                            switch (keys[0])
                            {
                                case "float":
                                    input.Format = Format.R32_Float;
                                    break;
                                case "float2":
                                    input.Format = Format.R32G32_Float;
                                    break;
                                case "float3":
                                    input.Format = Format.R32G32B32_Float;
                                    break;
                                case "float4":
                                    input.Format = Format.R32G32B32A32_Float;
                                    break;
                                case "byte4":
                                    input.Format = Format.R8G8B8A8_UNorm;
                                    break;
                                default:
                                    Log.Warning($"Unkown input element format at line: {line}");
                                    continue;
                            }

                            if (byte.TryParse(keys[2].Last().ToString(), out byte success))
                            {
                                input.SemanticName = keys[2].Substring(0, keys[2].Length - 1);
                                input.SemanticIndex = success;
                            }
                            else
                            {
                                input.SemanticName = keys[2];
                                input.SemanticIndex = 0;
                            }

                            inputs.Add(input);
                        }
                        else
                            newSource.AppendLine(line);
                    }

                    source = newSource.ToString();
                }

                ReadOnlyMemory<byte> vbc = Compiler.Compile(source, "vertex", Path.GetFileNameWithoutExtension(path), "vs_5_0", flags);
                ReadOnlyMemory<byte> pbc = Compiler.Compile(source, "pixel", Path.GetFileNameWithoutExtension(path), "ps_5_0", flags);

                GraphicsDevice.FillShaderData(ref data, vbc.ToArray(), pbc.ToArray(), inputs.ToArray());
            }

            _assets.Add(path, data);
            return new Shader(data);
        }

        protected override Font GetOrCreateFont(string path)
        {
            if (_assets.TryGetValue(path, out IVirtualAsset? shader))
            {
                if (shader == null || shader?.Type != AssetType.Shader)
                    throw new InvalidOperationException("Invalid virtual asset handle!");
                else
                    return new Font((Font.FontDataAsset?)shader);
            }

            byte[] source = ContentLoader.GetBytes(path);

            Font.FontDataAsset data = new Font.FontDataAsset();
            data.Name = path;

            if (source.Length > 0)
            {
                Log.Debug($"Creating asset: \"{path}\"");

                Library lib = new Library();
                Face face = new Face(lib, source, 0);

                List<Font.Glyph> glyphs = new List<Font.Glyph>();
                List<byte[]> bitmaps = new List<byte[]>();
                List<PackingRectangle> packingRectangles = new List<PackingRectangle>();

                face.SetPixelSizes(0, 64);
                for (uint i = 0; i < byte.MaxValue; i++)
                {
                    try
                    {
                        face.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);
                        face.Glyph.RenderGlyph((RenderMode)5);

                        int w = face.Glyph.Bitmap.Width;
                        int h = face.Glyph.Bitmap.Rows;
  
                        bitmaps.Add(face.Glyph.Bitmap.BufferData);
                        glyphs.Add(new Font.Glyph
                        {
                            UV = Vector4.Zero,
                            Size = new Vector2(w, h),
                            Bearing = new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                            Advance = (byte)face.Glyph.Advance.X.ToInt32()
                        });
                        packingRectangles.Add(new PackingRectangle
                        {
                            Id = (int)i,
                            Width = (uint)w + 2U,
                            Height = (uint)h + 2U
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }

                PackingRectangle bounds;
                {
                    PackingRectangle[] r = packingRectangles.ToArray();
                    RectanglePacker.Pack(r, out bounds);

                    packingRectangles.Clear();
                    packingRectangles.AddRange(r);
                    packingRectangles.Sort((x, y) => x.Id.CompareTo(y.Id));
                }

                byte[] buffer = new byte[(bounds.Width + 1) * (bounds.Height + 1)];
                for (int i = 0; i < glyphs.Count; i++)
                {
                    PackingRectangle rectangle = packingRectangles[i];
                    Font.Glyph glyph = glyphs[i];
                    byte[] pixels = bitmaps[i];

                    for (int y = 0; y < glyph.Size.Y; y++)
                        Array.Copy(pixels, (int)(y * glyph.Size.X), buffer, (int)(rectangle.X + (rectangle.Y + y) * bounds.Width), (int)glyph.Size.X);

                    glyph.UV = new Vector4(
                        rectangle.X / (float)bounds.Width,
                        rectangle.Y / (float)bounds.Height,
                        (rectangle.X + rectangle.Width) / (float)bounds.Width,
                        (rectangle.Y + rectangle.Height) / (float)bounds.Height);

                    data.Glyphs.TryAdd(System.Text.Encoding.UTF8.GetString([(byte)rectangle.Id])[0], glyph);
                }

                GraphicsDevice.FillFontData(ref data, buffer, (int)bounds.Width, (int)bounds.Height);
            }

            _assets.Add(path, data);
            return new Font(data);
        }
    }
}
