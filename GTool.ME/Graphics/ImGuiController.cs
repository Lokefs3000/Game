using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GTool.ME.Graphics
{
    internal class ImGuiController : IDisposable
    {
        public ImGuiController()
        {
            ImGui.CreateContext();
        }

        public void Dispose()
        {
            ImGui.DestroyContext();
        }

        public void Update()
        {
            ImGui.NewFrame();
        }

        public void Render()
        {
            ImGui.Render();
        }
    }
}
