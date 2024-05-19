using GTool.Content;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace GTool.Graphics.Content
{
    public class Font : IDisposable
    {
        private readonly FontDataAsset? _data;
        private readonly bool _valid;

        public FontDataAsset? Data { get { return _data; } }
        public bool IsValid { get { return _valid; } }

        public Font(FontDataAsset? data)
        {
            _data = data;
            _valid = data == null || data.ResourceView != null && data.SamplerState != null && data.Texture != null;

            _data?.Reference();
        }

        public void Dispose()
        {
            _data?.Dispose();
        }

        public void Bind()
        {
            if (_valid)
            {
                GraphicsDevice.PSetShaderResource(_data?.ResourceView);
                GraphicsDevice.PSetSampler(_data?.SamplerState);
            }
        }

        public class FontDataAsset : ContentManager.IVirtualAsset
        {
            public AssetType Type => AssetType.Font;
            public string Name { get => _name; set => _name = value; }

            private string _name = string.Empty;

            public ID3D11Texture2D? Texture { get; set; }
            public ID3D11SamplerState? SamplerState { get; set; }
            public ID3D11ShaderResourceView? ResourceView { get; set; }
            public Dictionary<char, Glyph> Glyphs { get; set; } = new Dictionary<char, Glyph>();

            public void Reference()
            {
                Texture?.AddRef();
                SamplerState?.AddRef();
                ResourceView?.AddRef();
            }

            public void Dispose()
            {
                uint ref1 = Texture?.Release() ?? 1;
                uint ref2 = SamplerState?.Release() ?? 1;
                uint ref3 = ResourceView?.Release() ?? 1;

                if (ref1 != ref2 || ref1 != ref3 || ref2 != ref3)
                    Log.Error("Inconsistant reference on font resources!");
                if (ref1 <= 1) //because this means 0 references! Because everything starts at 1 reference and this is the min.
                    ContentManager.Remove(Name);
            }
        }

        public struct Glyph
        {
            public Vector4 UV;
            public Vector2 Size;
            public Vector2 Bearing;
            public byte Advance;
        }
    }
}
