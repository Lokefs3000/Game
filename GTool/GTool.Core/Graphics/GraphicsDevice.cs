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

        private ID3D11InfoQueue? _infoQueue;

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

                if (IsDebug)
                {
                    _infoQueue = Device.QueryInterfaceOrNull<ID3D11InfoQueue>();
                    if (_infoQueue == null)
                        Log.Warning("Failed to get D3D11 info queue!");
                }

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
                    SwapEffect = SwapEffect.FlipSequential,
                    AlphaMode = AlphaMode.Unspecified,
                    Flags = SwapChainFlags.None
                });

                ResizeBuffers(settings.Window.WindowSize);

                _rasterizerState = Device?.CreateRasterizerState(RasterizerDescription.CullBack);
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
            try
            {
                uint r1 = _backBuffer?.Release() ?? 0;
                uint r2 = _backBufferView?.Release() ?? 0;

                if (r1 != 0 || r2 != 0)
                {
                    Log.Error("A swapchain resource has not properly been released for resize!");
                    return;
                }

                _backBuffer = null;
                _backBufferView = null;

                Result? res = _swapChain?.ResizeBuffers(2, (int)size.Width, (int)size.Height);
                if (res != null && res.Value.Failure)
                {
                    if (res.Value.Code == unchecked((int)0x887A0005)/*DXGI_ERROR_DEVICE_REMOVED*/)
                    {
                        HandleDeviceLost();
                    }

                    res?.CheckError();
                }

                _backBuffer = _swapChain?.GetBuffer<ID3D11Texture2D>(0);
                _backBufferView = Device?.CreateRenderTargetView(_backBuffer);
                _viewport = new Viewport(size.Width, size.Height);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                FlushMessageQueue();
                throw;
            }
        }

        internal static void Resize(Size size) => Instance?.ResizeBuffers(size);

        public void Dispose()
        {
            if (!_disposed)
            {
                _rasterizerState?.Release();
                _swapChain?.Release();

                Deferred?.Release();
                Immediate?.Release();

                _infoQueue?.Release();
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

        public void FlushMessageQueue()
        {
            if (IsDebug && _infoQueue != null)
            {
                ulong stored = _infoQueue.NumStoredMessages;
                for (ulong i = 0; i < stored; i++)
                {
                    Message msg = _infoQueue.GetMessage(i);

                    switch (msg.Severity)
                    {
                        case MessageSeverity.Corruption:
                            Log.Fatal("D3D11 Error: [{@Severity}/{@Catergory}]: {@Message}", msg.Severity, msg.Category, msg.Description);
                            break;
                        case MessageSeverity.Error:
                            Log.Error("D3D11 Error: [{@Severity}/{@Catergory}]: {@Message}", msg.Severity, msg.Category, msg.Description);
                            break;
                        case MessageSeverity.Warning:
                            Log.Warning("D3D11 Error: [{@Severity}/{@Catergory}]: {@Message}", msg.Severity, msg.Category, msg.Description);
                            break;
                        case MessageSeverity.Info:
                            Log.Information("D3D11 Error: [{@Severity}/{@Catergory}]: {@Message}", msg.Severity, msg.Category, msg.Description);
                            break;
                        case MessageSeverity.Message:
                            Log.Debug("D3D11 Error: [{@Severity}/{@Catergory}]: {@Message}", msg.Severity, msg.Category, msg.Description);
                            break;
                        default:
                            break;
                    }
                }

                _infoQueue.ClearStoredMessages();
            }
        }

        public void Present()
        {
            try
            {
                ID3D11CommandList? cmd = Deferred?.FinishCommandList(false);
                if (cmd != null)
                {
                    Immediate?.ExecuteCommandList(cmd, false);
                    cmd.Release();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in present call! Message: {@Message}", ex.Message);
            }

            Result? res = _swapChain?.Present(1, PresentFlags.None, new PresentParameters { DirtyRectangles = [] });
            if (res != null && res.Value.Failure && res.Value.Code == unchecked((int)0x887A0005)/*DXGI_ERROR_DEVICE_REMOVED*/)
            {
                HandleDeviceLost();
                throw new ApplicationException("Device lost!");
            }

            //theoreticly the next frame begins here..
            Deferred?.RSSetState(_rasterizerState);
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

        public static void VSetShaderResource(ID3D11ShaderResourceView? resourceView, int slot = 0)
            => Instance.Deferred?.VSSetShaderResource(slot, resourceView);
        public static void PSetShaderResource(ID3D11ShaderResourceView? resourceView, int slot = 0)
            => Instance.Deferred?.PSSetShaderResource(slot, resourceView);

        public static void VSetSampler(ID3D11SamplerState? samplerState, int slot = 0)
            => Instance.Deferred?.VSSetSampler(slot, samplerState);
        public static void PSetSampler(ID3D11SamplerState? samplerState, int slot = 0)
            => Instance.Deferred?.PSSetSampler(slot, samplerState);

        public static void SetTopology(PrimitiveTopology topology) => Instance.Deferred?.IASetPrimitiveTopology(topology);
        public static void SetBlendState(ID3D11BlendState? blendState) => Instance.Deferred?.OMSetBlendState(blendState);

        public static void RestoreDefaultDrawViews()
        {
            Instance.Deferred?.RSSetViewport(Instance._viewport);
            Instance.Deferred?.OMSetRenderTargets(Instance._backBufferView);
        }

        public static void FillShaderData(ref Shader.ShaderDataAsset data, byte[] vbytes, byte[] pbytes, InputElementDescription[] inputs)
        {
            data.VertexShader = Instance.Device?.CreateVertexShader(vbytes);
            data.PixelShader = Instance.Device?.CreatePixelShader(pbytes);
            data.InputLayout = Instance.Device?.CreateInputLayout(inputs, vbytes);
        }

        public static void FillFontData(ref Font.FontDataAsset data, byte[] buffer, int width, int height)
        {
            data.Texture = Instance.Device?.CreateTexture2D(new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8_UNorm,
                SampleDescription = SampleDescription.Default,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            });

            data.ResourceView = Instance.Device?.CreateShaderResourceView(data.Texture, null);

            data.SamplerState = Instance.Device?.CreateSamplerState(new SamplerDescription
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaxLOD = 1,
                MinLOD = 0,
                MipLODBias = 0.0f,
                BorderColor = new Color4(0.0f),
                ComparisonFunc = ComparisonFunction.Never,
                MaxAnisotropy = 0,
            });

            if (data.Texture != null)
                Instance.Deferred?.UpdateSubresource(buffer, data.Texture, 0, width);
        }

        internal static ID3D11BlendState? CreateBlendState(Blend source, Blend sourceAlpha, Blend dest, Blend destAlpha, BlendOperation op, BlendOperation alphaOp)
        {
            return Instance.Device?.CreateBlendState(new BlendDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
                RenderTarget = new BlendDescription.RenderTarget__FixedBuffer
                {
                    e0 = new RenderTargetBlendDescription
                    {
                        BlendEnable = true,
                        BlendOperation = op,
                        BlendOperationAlpha = alphaOp,
                        DestinationBlend = dest,
                        DestinationBlendAlpha = destAlpha,
                        SourceBlend = source,
                        SourceBlendAlpha = sourceAlpha,
                        RenderTargetWriteMask = ColorWriteEnable.All
                    }
                }
            });
        }
    }

    public struct GraphicsDeviceSettings
    {
        public NativeWindow Window;
    }
}
