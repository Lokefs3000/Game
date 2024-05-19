using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace GTool.Graphics
{
    public class BlendState : IDisposable
    {
        private ID3D11BlendState? _blendState;

        public BlendState(Blend source, Blend sourceAlpha, Blend dest, Blend destAlpha, BlendOperation op, BlendOperation alphaOp)
        {
            _blendState = GraphicsDevice.CreateBlendState(source, sourceAlpha, dest, destAlpha, op, alphaOp);
        }

        public void Dispose()
        {
            _blendState?.Dispose();
        }

        public void Bind()
        {
            GraphicsDevice.SetBlendState(_blendState);
        }
    }
}
