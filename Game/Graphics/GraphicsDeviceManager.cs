using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Direct3D;
using SDL;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;
using static SDL.SDL;
using System.Numerics;
using Game.Config;
using Vortice.Mathematics;

namespace Game.Graphics
{
    internal class GraphicsDeviceManager : IDisposable
    {
        private IDXGIFactory3 _factory;
        private ID3D11Device _device;

        private ID3D11DeviceContext _immediate;
        private ID3D11DeviceContext _deferred;

        private IDXGISwapChain1 _swapChain;
        private ID3D11Texture2D _backBuffer;
        private ID3D11RenderTargetView? _backBufferView;

        public GraphicsDeviceManager(CoreWindow _renderWindow)
        {
            try
            {
                nint hWnd = _renderWindow.NativeWindowPointer;
                Debug.Assert(hWnd != nint.Zero);

                DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.None;
                bool dxgiFactoryDebug = false;
#if DEBUG
                deviceCreationFlags |= DeviceCreationFlags.Debug;
                dxgiFactoryDebug = true;
#endif

                _factory = CreateDXGIFactory2<IDXGIFactory3>(dxgiFactoryDebug);
                _device = D3D11CreateDevice(DriverType.Hardware, deviceCreationFlags, FeatureLevel.Level_11_0, FeatureLevel.Level_11_1);

                _immediate = _device.ImmediateContext;
                _deferred = _device.CreateDeferredContext();


                Vector2 pixelSize = _renderWindow.Size;
                _swapChain = _factory.CreateSwapChainForHwnd(_device, hWnd, new SwapChainDescription1
                {
                    Width = (int)pixelSize.X,
                    Height = (int)pixelSize.Y,
                    Format = Format.R8G8B8A8_UNorm,
                    Stereo = false,
                    SampleDescription = SampleDescription.Default,
                    BufferUsage = Usage.RenderTargetOutput,
                    BufferCount = 2,
                    Scaling = Scaling.None,
                    SwapEffect = SwapEffect.FlipDiscard,
                    AlphaMode = AlphaMode.Unspecified,
                    Flags = SwapChainFlags.None
                });

                _backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
                ResizeBuffers(_renderWindow.Size);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            _swapChain.Dispose();
            _deferred.Dispose();
            _immediate.Dispose();
            _device.Dispose();
            _factory.Dispose();

            GC.SuppressFinalize(this);
        }

        public void ResizeBuffers(Vector2 size)
        {
            _backBufferView?.Dispose();
            _swapChain.ResizeBuffers(2, (int)size.X, (int)size.Y);
            _backBufferView = _device.CreateRenderTargetView(_backBuffer);
            Debug.Assert(_backBufferView != null);
        }

        public void BindAndClearBackBuffer(Color4 color)
        {
            _deferred.OMSetRenderTargets(_backBufferView);
            _deferred.ClearRenderTargetView(_backBufferView, in color);
        }

        public void SubmitAndPresent()
        {
            try
            {
                ID3D11CommandList? cmdList = _deferred.FinishCommandList(false);
                if (cmdList != null)
                {
                    _immediate.ExecuteCommandList(cmdList, true);
                    cmdList.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            _swapChain.Present(StartupConfig.Config.RenderVSync);
        }
    }
}
