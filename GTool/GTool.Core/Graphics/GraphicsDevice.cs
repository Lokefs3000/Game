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
            _swapChain?.ResizeBuffers(2, (int)size.Width, (int)size.Height);
            _backBuffer = _swapChain?.GetBuffer<ID3D11Texture2D>(0);
            _backBufferView = Device?.CreateRenderTargetView(_backBuffer);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
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
            ID3D11CommandList? cmd = Deferred?.FinishCommandList(false);
            if (cmd != null)
            {
                Immediate?.ExecuteCommandList(cmd, true);
                cmd.Release();
            }
            _swapChain?.Present(1);
        }

        public static void Draw(int vertexCount, int startVertexLocation) => Instance.Deferred?.Draw(vertexCount, startVertexLocation);
        public static void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation) => Instance.Deferred?.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
    }

    public struct GraphicsDeviceSettings
    {
        public NativeWindow Window;
    }
}
