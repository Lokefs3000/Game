using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace GTool.Graphics
{
    public class Buffer<TType> : IDisposable
        where TType : unmanaged
    {
        private ID3D11Buffer? _buffer;
        private int _size;
        private BindFlags _flags;
        private int _stride;

        public ID3D11Buffer? InternalBuffer { get => _buffer; }
        public int Size { get => _size; }
        public int Stride { get => _stride; }

        public Buffer(int size, BindFlags bind)
        {
            _size = size;
            _flags = bind;
            _stride = Marshal.SizeOf<TType>();

            _buffer = GraphicsDevice.Instance.Device?.CreateBuffer(new BufferDescription
            {
                BindFlags = bind,
                ByteWidth = size * _stride,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
                Usage = ResourceUsage.Default
            });
        }

        public void Dispose()
        {
            _buffer?.Dispose();
        }

        public void Update(TType[] data)
        {
            if (_buffer == null)
                return;

            GraphicsDevice.Instance.Deferred?.UpdateSubresource(data, _buffer, 0, _stride);
        }

        public void Bind()
        {
            if (_buffer == null)
                return;

            switch (_flags)
            {
                case BindFlags.VertexBuffer:
                    GraphicsDevice.Instance.Deferred?.IASetVertexBuffer(0, _buffer, Marshal.SizeOf<TType>());
                    break;
                case BindFlags.IndexBuffer:
                    GraphicsDevice.Instance.Deferred?.IASetIndexBuffer(_buffer, Format.R32_UInt, 0);
                    break;
            }
        }
    }
}
