
using RectpackSharp;
using SharpFont;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Vortice.D3DCompiler;

namespace GTool.CB
{
    internal static class FilePostProcess
    {
        public static List<string> AcceptedExtensions = [ ".png", ".ogg", ".fbx", ".hlsl", ".ttf" ];
        public static ProcessQuality Quality = ProcessQuality.None;

        public static bool Process(string file, string outfile)
        {
            string ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".png":
                    throw new NotImplementedException();
                case ".ogg":
                    throw new NotImplementedException();
                case ".fbx":
                    throw new NotImplementedException();
                case ".hlsl":
                    return ProcessHLSL(file, outfile);
                case ".ttf":
                    return ProcessTTF(file, outfile);
            }

            return false;
        }

        private static bool ProcessHLSL(string file, string outfile)
        {
            if (!HasBeenInvalidated(file, outfile))
                return true;
            Main.DebugWriteLine($"Reproccesing invalidated file: {file}..");

            ShaderFlags flags = ShaderFlags.None;
            switch (Quality)
            {
                case ProcessQuality.Medium:
                    flags |= ShaderFlags.OptimizationLevel1;
                    break;
                case ProcessQuality.High:
                    flags |= ShaderFlags.OptimizationLevel2;
                    break;
                case ProcessQuality.Max:
                    flags |= ShaderFlags.OptimizationLevel3;
                    break;
                default:
                    break;
            }
#if DEBUG
            if (Quality != ProcessQuality.Max)
                flags |= ShaderFlags.Debug;
#endif

            string source = File.ReadAllText(file).Replace("\r\n", "\n"); //perhaps use ReadAllLines instead?

            List<ShaderInputElement> inputs = new List<ShaderInputElement>();
            {
                string[] lines = source.Split('\n');
                StringBuilder newSource = new StringBuilder();

                bool reading = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.StartsWith("##"))
                    {
                        string trimmed = line.Substring(2);
                        string[] keys = trimmed.Split(' ');

                        if (keys.Length != 3)
                        {
                            newSource.AppendLine(line);
                            continue;
                        }

                        ShaderInputElement input = new ShaderInputElement();

                        switch (keys[0])
                        {
                            case "float2":
                                input.Format = 16;
                                break;
                            case "float3":
                                input.Format = 6;
                                break;
                            case "float4":
                                input.Format = 2;
                                break;
                            case "byte4":
                                input.Format = 28;
                                break;
                            default:
                                Main.DebugWriteLine($"Unkown input element format at line: {line}");
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

            string name = Path.GetFileName(file);

            try
            {
                ReadOnlyMemory<byte> vbytes = Compiler.Compile(source, "vertex", name, "vs_5_0", flags);
                ReadOnlyMemory<byte> pbytes = Compiler.Compile(source, "pixel", name, "ps_5_0", flags);

                File.WriteAllText(outfile, string.Empty); //clear file
                FileStream stream = File.OpenWrite(outfile);

                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write((byte)Quality);
                    bw.Write(File.GetLastWriteTime(file).Ticks);
                    bw.Write(vbytes.Length);
                    bw.Write(pbytes.Length);
                    bw.Write(vbytes.ToArray());
                    bw.Write(pbytes.ToArray());

                    bw.Write((byte)inputs.Count);
                    foreach (ShaderInputElement element in inputs)
                    {
                        bw.Write(element.SemanticName);
                        bw.Write(element.SemanticIndex);
                        bw.Write(element.Format);
                    }
                }
            }
            catch (Exception ex)
            {
                Main.DebugWriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private static bool ProcessTTF(string file, string outfile)
        {
            if (!HasBeenInvalidated(file, outfile))
                return true;
            Main.DebugWriteLine($"Reproccesing invalidated file: {file}..");

            Library lib = new Library();
            Face face = new Face(lib, file);

            int range = face.GlyphCount;
            List<FontGlyphData> data = new List<FontGlyphData>();
            List<PackingRectangle> pack = new List<PackingRectangle>();

            PackingRectangle bounds = new PackingRectangle(0, 0, 0, 0);

            try
            {
                face.SetPixelSizes(0, 48);

                int j = 0;
                for (int i = 0; i < face.GlyphCount; i++)
                {
                    face.LoadChar((uint)i, LoadFlags.Render, LoadTarget.Normal);
                    if (face.Glyph.Bitmap.Buffer == nint.Zero)
                    {
                        Main.DebugWriteLine($"Failed to get glyph: {i}");
                        continue;
                    }

                    data.Add(new FontGlyphData
                    {
                        Id = (ushort)i,
                        Position = Vector2.Zero,
                        Size = new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                        Bearing = new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                        Advance = (byte)(face.Glyph.Advance.X.Value >> 6),
                        Pixels = face.Glyph.Bitmap.BufferData
                    });

                    pack.Add(new PackingRectangle(0, 0, (uint)data[j].Size.X, (uint)data[j].Size.Y));
                    j++;
                }

                PackingRectangle[] copyPack = pack.ToArray();
                RectanglePacker.Pack(copyPack, out bounds, PackingHints.FindBest, 1, 4U - (uint)Quality);
                pack = new List<PackingRectangle>(copyPack);
            }
            catch (Exception ex)
            {
                Main.DebugWriteLine(ex.Message);
                Main.DebugWriteLine(ex.StackTrace ?? "Failed to retrieve stacktrace!");
#if DEBUG
                //throw;
#endif
                return false;
            }

            face.Dispose();
            lib.Dispose();

            byte[] pixels = new byte[bounds.Width * bounds.Height];
            for (int i = 0; i < data.Count; i++)
            {
                FontGlyphData glyph = data[i];
                PackingRectangle rect = pack[i];

                glyph.Position = new Vector2(rect.X, rect.Y);
                for (int j = 0; j < glyph.Size.Y; j++)
                {
                    Array.Copy(glyph.Pixels, (int)(j * glyph.Size.X), pixels, (int)(rect.X + rect.Y * bounds.Width), (int)glyph.Size.X);
                }

                data[i] = glyph;
            }

            File.WriteAllText(outfile, string.Empty); //clear file
            FileStream stream = File.OpenWrite(outfile);

            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write((byte)Quality);
                bw.Write(File.GetLastWriteTime(file).Ticks);
                bw.Write((ushort)bounds.Width);
                bw.Write((ushort)bounds.Height);
                bw.Write(pixels);

                foreach (FontGlyphData glyph in data)
                {
                    bw.Write(glyph.Id);
                    bw.Write((ushort)glyph.Position.X);
                    bw.Write((ushort)glyph.Position.Y);
                    bw.Write((byte)glyph.Size.X);
                    bw.Write((byte)glyph.Size.Y);
                    bw.Write((byte)glyph.Bearing.X);
                    bw.Write((byte)glyph.Bearing.Y);
                    bw.Write(glyph.Advance);
                }
            }

            return true;
        }

        private static bool HasBeenInvalidated(string file, string outfile)
        {
            if (!File.Exists(outfile))
                return true;

            long ticks = 0;
            ProcessQuality quality = ProcessQuality.None;

            FileStream stream = File.OpenRead(outfile);
            using (BinaryReader br = new BinaryReader(stream))
            {
                ticks = br.ReadInt64();
                quality = (ProcessQuality)br.ReadByte();
            }

            return ticks != File.GetLastWriteTime(file).Ticks || quality != Quality;
        }

        public enum ProcessQuality
        {
            None = 0,
            Medium = 1,
            High = 2,
            Max = 3
        }

        private struct ShaderInputElement
        {
            public string SemanticName;
            public byte SemanticIndex;
            public byte Format;
        }

        private struct FontGlyphData
        {
            public ushort Id;
            public Vector2 Position;
            public Vector2 Size;
            public Vector2 Bearing;
            public byte Advance;
            public byte[] Pixels;
        }
    }
}
