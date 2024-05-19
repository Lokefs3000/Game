using GTool.Content;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace GTool.Graphics.Content
{
    public class Shader : IDisposable
    {
        private readonly ShaderDataAsset? _data;
        private readonly bool _valid;

        public bool IsValid { get { return _valid; } }

        public Shader(ShaderDataAsset? data)
        {
            _data = data;
            _valid = data == null || data.InputLayout != null && data.VertexShader != null && data.PixelShader != null;

            _data?.Reference();
        }

        public void Dispose()
        {
            _data?.Dispose();
        }

        public void Bind()
        {
            if (_valid)
                GraphicsDevice.SetActiveShader(_data?.VertexShader, _data?.PixelShader, _data?.InputLayout);
        }

        public class ShaderDataAsset : ContentManager.IVirtualAsset
        {
            public AssetType Type => AssetType.Shader;
            public string Name { get => _name; set => _name = value; }

            private string _name = string.Empty;

            public ID3D11VertexShader? VertexShader { get; set; }
            public ID3D11PixelShader? PixelShader { get; set; }
            public ID3D11InputLayout? InputLayout { get; set; }

            public void Reference()
            {
                VertexShader?.AddRef();
                PixelShader?.AddRef();
                InputLayout?.AddRef();
            }

            public void Dispose()
            {
                uint ref1 = VertexShader?.Release() ?? 1;
                uint ref2 = PixelShader?.Release() ?? 1;
                uint ref3 = InputLayout?.Release() ?? 1;

                if (ref1 != ref2 || ref1 != ref3 || ref2 != ref3)
                    Log.Error("Inconsistant reference on shader resources!");
                if (ref1 <= 1)
                    ContentManager.Remove(Name);
            }
        }
    }
}
