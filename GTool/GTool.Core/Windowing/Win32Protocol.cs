using GTool.Debugging;
using GTool.Input;
using System.Runtime.InteropServices;
using static GTool.Windowing.IWindowProtocol;

namespace GTool.Windowing
{
    internal class Win32Protocol : IWindowProtocol
    {
        private static Win32Protocol _instance;

        private bool _isClassRegistered = false;
        private int _windowsCreated = 0;
        private nint _moduleHandle = nint.Zero;

        private const string CLASSNAME = "GTool_CLSN";

        private const int CW_USEDEFAULT = unchecked((int)2147483648);
        private const ulong WS_OVERLAPPED = 0x00000000L;
        private const ulong WS_CAPTION = 0x00C00000L;
        private const ulong WS_SYSMENU = 0x00080000L;
        private const ulong WS_THICKFRAME = 0x00040000L;
        private const ulong WS_MINIMIZEBOX = 0x00020000L;
        private const ulong WS_MAXIMIZEBOX = 0x00010000L;

        private WNDPROC? _windowProc;

        public static event IWindowProtocol.WindowEvent? WindowClosed;

        public event IWindowProtocol.WindowResizeEvent? WindowResize;

        public event IWindowProtocol.MouseMoveEvent? MouseMove;
        public event IWindowProtocol.MouseButtonEvent? MouseButton;

        public Win32Protocol()
        {
            _instance = this;
        }

        public nint CreateWindow(in WindowCreationSettings settings)
        {
            if (_windowProc == null)
            {
                _windowProc = WindowProc;
            }
            if (_moduleHandle == nint.Zero)
            {
                _moduleHandle = Marshal.GetHINSTANCE(typeof(Win32Protocol).Module);
            }
            if (!_isClassRegistered)
            {
                WNDCLASSW dwClass = new WNDCLASSW();
                dwClass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_windowProc);
                dwClass.lpszClassName = CLASSNAME;
                dwClass.hInstance = _moduleHandle;
                Logger.Assert(Interop.RegisterClassW(ref dwClass) != 1410);
                _isClassRegistered = true;
            }
            _windowsCreated++;

            ulong style = GetStyleFromFlags(settings.Flags);

            RECT rect = new RECT { left = 0, top = 0, right = settings.Width, bottom = settings.Height };
            Logger.Assert(Interop.AdjustWindowRectEx(ref rect, style, true, 0)); //might not actually be working 100%

            nint window = Interop.CreateWindowExW(
                0,
                CLASSNAME,
                settings.Title,
                style,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                (int)(rect.right - rect.left) + 8/*bad win32 fix*/,
                (int)(rect.bottom - rect.top) + 47,
                nint.Zero,
                nint.Zero,
                _moduleHandle,
                nint.Zero);

            Logger.Assert(window != nint.Zero);
            Logger.Assert(!Interop.ShowWindow(window, 1));

            return window;
        }

        public void DestroyWindow(nint window)
        {
            if (window == IntPtr.Zero) throw new ArgumentNullException(nameof(window));

            _windowsCreated--;
            if (_windowsCreated == 0)
            {
                Logger.Assert(Interop.UnregisterClassW(CLASSNAME, nint.Zero));
                _isClassRegistered = false;
            }
        }
        
        private const uint PM_REMOVE = 1;

        public void ProcessWindows()
        {
            MSG msg = new MSG();
            while (Interop.PeekMessage(ref msg, nint.Zero, 0, 0, PM_REMOVE))
            {
                Interop.TranslateMessage(ref msg);
                Interop.DispatchMessageW(ref msg);
            }
        }

        public void BindClosed(IWindowProtocol.WindowEvent @event) => WindowClosed += @event;
        public void UnbindClosed(IWindowProtocol.WindowEvent @event) => WindowClosed -= @event;

        private ulong GetStyleFromFlags(WindowCreationFlags flags)
        {
            ulong style = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;

            if ((flags & WindowCreationFlags.Resizable) != 0)
                style = style | WS_MAXIMIZEBOX | WS_THICKFRAME;

            return style;
        }

        private const int WM_DESTROY = 2;
        private const int WM_CLOSE = 16;
        private const int WM_MOUSEMOVE = 512;
        private const int WM_LBUTTONDOWN = 513;
        private const int WM_LBUTTONUP = 514;
        private const int WM_MBUTTONDOWN = 519;
        private const int WM_MBUTTONUP = 520;
        private const int WM_RBUTTONDOWN = 516;
        private const int WM_RBUTTONUP = 517;
        private const int WM_SIZE = 5;

        private const int MK_LBUTTON = 1;
        private const int MK_MBUTTON = 16;
        private const int MK_RBUTTON = 2;

        private static nint WindowProc(nint hWnd, uint uMsg, nint wParam, nint lParam)
        {
            switch (uMsg)
            {
                case WM_CLOSE:
                    {
                        WindowClosed?.Invoke(hWnd);
                        Interop.DestroyWindow(hWnd);
                        return 0;
                    }
                case WM_DESTROY:
                    {
                        Interop.PostQuitMessage(0);
                        return 0;
                    }
                case WM_MOUSEMOVE:
                    {
                        _instance.MouseMove?.Invoke((float)GET_X_LPARAM(lParam), (float)GET_Y_LPARAM(lParam));
                        return 0;
                    }
                case WM_LBUTTONDOWN:
                    {
                        _instance.MouseButton?.Invoke(InputManager.MouseButton.Left, true);
                        return 0;
                    }
                case WM_LBUTTONUP:
                    {
                        _instance.MouseButton?.Invoke(InputManager.MouseButton.Left, false);
                        return 0;
                    }
                case WM_MBUTTONDOWN:
                    {
                        _instance.MouseButton?.Invoke(InputManager.MouseButton.Middle, true);
                        return 0;
                    }
                case WM_MBUTTONUP:
                    {
                        _instance.MouseButton?.Invoke(InputManager.MouseButton.Middle, false);
                        return 0;
                    }
                case WM_RBUTTONDOWN:
                    {
                        _instance.MouseButton?.Invoke(InputManager.MouseButton.Right, true);
                        return 0;
                    }
                case WM_RBUTTONUP:
                    {
                        _instance.MouseButton?.Invoke(InputManager.MouseButton.Right, false);
                        return 0;
                    }
                case WM_SIZE:
                    {
                        _instance.WindowResize?.Invoke(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                        return 0;
                    }
                default:
                    return Interop.DefWindowProcW(hWnd, uMsg, wParam, lParam);
            }
        }

        private static int GET_X_LPARAM(nint lParam) => (int)lParam & 0xFFFF;
        private static int GET_Y_LPARAM(nint lParam) => (int)(lParam >> 16) & 0xFFFF;

        internal class Interop
        {
            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern ushort RegisterClassW([In] ref WNDCLASSW lpWndClass);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern bool UnregisterClassW([MarshalAs(UnmanagedType.LPWStr)] string lpClassName, nint hInstance);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern nint CreateWindowExW(ulong dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, ulong dwStyle, int X, int Y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern bool DestroyWindow(nint hWnd);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern bool ShowWindow(nint hWnd, int nCmdShow);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern nint DefWindowProcW(nint hWnd, uint uMsg, nint wParam, nint lParam);

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern nint GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] in string? lpModuleName);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int PostQuitMessage(int nExitCode);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern bool PeekMessage([In] ref MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern bool TranslateMessage([In] ref MSG lpMsg);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern nint DispatchMessageW([In] ref MSG lpMsg);

            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern bool AdjustWindowRectEx([In][Out] ref RECT lpRect, ulong dwStyle, bool bMenu, ulong dwExStyle);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WNDCLASSW
        {
            public uint style;
            public nint lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public nint hInstance;
            public nint hIcon;
            public nint hCursor;
            public nint hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]  public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MSG
        {
            nint hwnd;
            uint message;
            nint wParam;
            nint lParam;
            ulong time;
            POINT pt;
            ulong lPrivate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public long X;
            public long Y;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct RECT
        {
            public long left;
            public long top;
            public long right;
            public long bottom;
        }

        internal delegate nint WNDPROC(nint hWnd, uint uMsg, nint wParam, nint lParam);
    }
}
