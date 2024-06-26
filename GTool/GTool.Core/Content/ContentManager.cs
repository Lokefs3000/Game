﻿using GTool.Graphics;
using GTool.Graphics.Content;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace GTool.Content
{
    public class ContentManager : IDisposable
    {
        protected static ContentManager _instance;

        protected static readonly Dictionary<string, IVirtualAsset?> _assets = new Dictionary<string, IVirtualAsset?>();

        public ContentManager()
        {
            _instance = this;
        }

        public void Dispose()
        {
            foreach (var asset in _assets.Values)
                asset.Dispose();
            _assets.Clear();
        }

        protected virtual Shader GetOrCreateShader(string path)
        {
            if (_assets.TryGetValue(path, out IVirtualAsset? shader))
            {
                if (shader == null || shader?.Type != AssetType.Shader)
                    throw new InvalidOperationException("Invalid virtual asset handle!");
                else
                    return new Shader((Shader.ShaderDataAsset?)shader);
            }

            byte[] source = ContentLoader.GetBytes(path);

            Shader.ShaderDataAsset data = new Shader.ShaderDataAsset();
            data.Name = path;

            if (source.Length > 0)
            {
                Log.Debug($"Creating asset: \"{path}\"");

                byte[] vbc = Array.Empty<byte>();
                byte[] pbc = Array.Empty<byte>();

                InputElementDescription[] inputs = Array.Empty<InputElementDescription>();

                MemoryStream stream = new MemoryStream(source);
                using (BinaryReader br = new BinaryReader(stream))
                {
                    vbc = new byte[br.ReadInt32()];
                    pbc = new byte[br.ReadInt32()];

                    br.Read(vbc, 0, vbc.Length);
                    br.Read(pbc, 0, pbc.Length);

                    inputs = new InputElementDescription[br.ReadByte()];
                    for (int i = 0; i < inputs.Length; i++)
                        inputs[i] = new InputElementDescription(br.ReadString(), br.ReadByte(), (Format)br.ReadByte(), 0);
                }

                GraphicsDevice.FillShaderData(ref data, vbc, pbc, inputs);
            }

            _assets.Add(path, data);
            return new Shader(data);
        }

        protected virtual Font GetOrCreateFont(string path)
        {
            if (_assets.TryGetValue(path, out IVirtualAsset? shader))
            {
                if (shader == null || shader?.Type != AssetType.Shader)
                    throw new InvalidOperationException("Invalid virtual asset handle!");
                else
                    return new Font((Font.FontDataAsset?)shader);
            }

            throw new NotImplementedException();
        }

        protected virtual void RemoveAsset(string path)
        {
            Log.Debug($"Removing asset: \"{path}\"");
            if (_assets.ContainsKey(path))
                _assets.Remove(path);
        }

        public static Shader GetShader(string path) => _instance.GetOrCreateShader(path);
        public static Font GetFont(string path) => _instance.GetOrCreateFont(path);
        public static void Remove(string path) => _instance.RemoveAsset(path);

        public interface IVirtualAsset : IDisposable
        {
            public AssetType Type { get; }
            public string Name { get; set; }

            public void Reference();
        }
    }

    public enum AssetType
    {
        None = 0,
        Shader,
        Texture2D,
        TextureCube,
        Mesh,
        Font
    }
}
