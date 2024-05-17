using GTool.Debugging;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Direct3D;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI.Debug;
using GTool.Windowing;
using Vortice.Mathematics;
using GTool.Graphics.Content;
using SharpGen.Runtime;
using Serilog;

namespace GTool.Graphics
{
    public class GraphicsDevice : IDisposable
    {
        internal static GraphicsDevice Instance;

        private IDXGIFactory3? _factory;
        public ID3D11Device? Device;

        public ID3D11DeviceContext? Immediate { get; private set; }
        public ID3D11DeviceContext? Deferred { get; private set; }

        private IDXGISwapChain1? _swapChain;
        private ID3D11Texture2D? _backBuffer;
        private ID3D11RenderTargetView? _backBufferView;
        private Viewport _viewport;

        private ID3D11RasterizerState? _rasterizerState;

        private bool _disposed;

#if DEBUG
        private const bool IsDebug = true;
#else
        private const bool IsDebug = false;
#endif

        public GraphicsDevice(GraphicsDeviceSettings settings)
        {
            Instance = this;

            Logger.Information("Debug layer enabled: {@Enabled}", IsDebug);

            try
            {
                _factory = CreateDXGIFactory2<IDXGIFactory3>(IsDebug);
                Device = D3D11CreateDevice(DriverType.Hardware, IsDebug ? DeviceCreationFlags.Debug : DeviceCreationFlags.None, FeatureLevel.Level_11_0, FeatureLevel.Level_11_1);

                Immediate = Device?.ImmediateContext;
                Deferred = Device?.CreateDeferredContext();

                _swapChain = _factory?.CreateSwapChainForHwnd(Device, settings.Window.Pointer, new SwapChainDescription1()
                {
                    Width = (int)settings.Window.WindowSize.Width,
                    Height = (int)settings.Window.WindowSize.Height,
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

                ResizeBuffers(settings.Window.WindowSize);

                _rasterizerState = Device?.CreateRasterizerState(RasterizerDescription.CullNone);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                Dispose();
#if DEBUG
                throw;
#else
                Environment.Exit(-1);
#endif
            }
        }

        internal void ResizeBuffers(Size size)
        {
            _backBuffer?.Release();
            _backBufferView?.Release();

            Result? res = _swapChain?.ResizeBuffers(2, (int)size.Width, (int)size.Height);
            if (res != null && res.Value.Failure && res.Value.Code == unchecked((int)0x887A0005)/*DXGI_ERROR_DEVICE_REMOVED*/)
            {
                HandleDeviceLost();
                throw new ApplicationException("Device lost!");
            }

            _backBuffer = _swapChain?.GetBuffer<ID3D11Texture2D>(0);
            _backBufferView = Device?.CreateRenderTargetView(_backBuffer);
            _viewport = new Viewport(size.Width, size.Height);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _rasterizerState?.Dispose();
                _swapChain?.Dispose();

                Deferred?.Dispose();
                Immediate?.Dispose();

                uint dcount = Device?.Release() ?? 0;
                uint fcount = _factory?.Release() ?? 0;

                PrintRemainingReferences(dcount, fcount);

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        private void PrintRemainingReferences(uint dcount, uint fcount)
        {
            if (dcount > 0)
            {
                Logger.Warning("{@Count} unresolved references to device!", dcount);

                if (IsDebug)
                {
                    ID3D11Debug? debug = Device?.QueryInterfaceOrNull<ID3D11Debug>();
                    if (debug != null)
                    {
                        debug.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail);
                        debug.Release();
                    }
                }
            }

            if (fcount > 0)
            {
                Logger.Warning("{@Count} unresolved references to factory!", fcount);

                if (IsDebug)
                {
                    IDXGIDebug1? debug = DXGIGetDebugInterface1<IDXGIDebug1>();
                    if (debug != null)
                    {
                        debug.ReportLiveObjects(DebugDxgi, ReportLiveObjectFlags.Detail);
                        debug.Release();
                    }
                }
            }
        }

        internal void Present()
        {
            try
            {
                ID3D11CommandList? cmd = Deferred?.FinishCommandList(false);
                if (cmd != null)
                {
                    Immediate?.ExecuteCommandList(cmd, true);
                    cmd.Release();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in present call! Message: {@Message}", ex.Message);
            }

            Result? res = _swapChain?.Present(1);
            if (res != null && res.Value.Failure && res.Value.Code == unchecked((int)0x887A0005)/*DXGI_ERROR_DEVICE_REMOVED*/)
            {
                HandleDeviceLost();
                throw new ApplicationException("Device lost!");
            }

            //theoreticly the next frame begins here..
            Deferred?.RSSetState(_rasterizerState);
            RestoreDefaultDrawViews();
            Deferred?.ClearRenderTargetView(_backBufferView, new Color4(1.0f, 0.5f, 0.0f, 1.0f));
        }

        private void HandleDeviceLost()
        {
            if (IsDebug)
            {
                Log.Error("Device was removed! Code: {@Code}", Device?.DeviceRemovedReason.ToString());
            }
            else
#pragma warning disable CS0162 // Unreachable code detected
                Log.Warning("Extended debug info is unavailable because debugging is not available!");
#pragma warning restore CS0162 // Unreachable code detected
        }

        public static void Draw(int vertexCount, int startVertexLocation) => Instance.Deferred?.Draw(vertexCount, startVertexLocation);
        public static void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation) => Instance.Deferred?.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);

        public static void SetVertexBuffer<TType>(Buffer<TType> buffer)
            where TType : unmanaged => Instance.Deferred?.IASetVertexBuffer(0, buffer.InternalBuffer, buffer.Stride);
        public static void SetIndexBuffer<TType>(Buffer<TType> buffer, Format format)
            where TType : unmanaged => Instance.Deferred?.IASetIndexBuffer(buffer.InternalBuffer, format, 0);

        public static void SetActiveShader(ID3D11VertexShader vshader, ID3D11PixelShader pshader, ID3D11InputLayout layout)
        {
            Instance.Deferred?.VSSetShader(vshader);
            Instance.Deferred?.PSSetShader(pshader);
            Instance.Deferred?.IASetInputLayout(layout);
        }

        public static void VSetConstantBuffer<TType>(Buffer<TType>? buffer, int slot = 0)
            where TType : unmanaged => Instance.Deferred?.VSSetConstantBuffer(slot, buffer?.InternalBuffer);
        public static void PSetConstantBuffer<TType>(Buffer<TType>? buffer, int slot = 0)
            where TType : unmanaged => Instance.Deferred?.PSSetConstantBuffer(slot, buffer?.InternalBuffer);

        public static void SetTopology(PrimitiveTopology topology) => Instance.Deferred?.IASetPrimitiveTopology(topology);

        public static void RestoreDefaultDrawViews()
        {
            Instance.Deferred?.RSSetViewport(Instance._viewport);
            Instance.Deferred?.OMSetRenderTargets(Instance._backBufferView);
        }

        internal static void FillShaderData(ref Shader.ShaderDataAsset data, byte[] vbytes, byte[] pbytes, InputElementDescription[] inputs)
        {
            data.VertexShader = Instance.Device?.CreateVertexShader(vbytes);
            data.PixelShader = Instance.Device?.CreatePixelShader(pbytes);
            data.InputLayout = Instance.Device?.CreateInputLayout(inputs, vbytes);
        }
    }

    public struct GraphicsDeviceSettings
    {
        public NativeWindow Window;
    }
}
