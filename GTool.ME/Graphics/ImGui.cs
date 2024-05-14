using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GTool.ME.Graphics
{
    internal static class ImGui
    {
        public static void CreateContext() => Binder.CreateContext();
        public static void DestroyContext() => Binder.DestroyContext();
        public static void NewFrame() => Binder.NewFrame();
        public static void Render() => Binder.Render();

        private static unsafe class Binder
        {
            [DllImport("GTool.ME.Gui.dll")]
            public static extern nint CreateContext();

            [DllImport("GTool.ME.Gui.dll")]
            public static extern nint DestroyContext();

            [DllImport("GTool.ME.Gui.dll")]
            public static extern nint NewFrame();

            [DllImport("GTool.ME.Gui.dll")]
            public static extern nint Render();
        }
    }
}
