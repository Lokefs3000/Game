using GTool.ME.Graphics;
using GTool.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTool.ME
{
    internal class Application : GApplication
    {
        private ImGuiController _controller;

        public Application(in string contentName, in WindowCreationSettings creationSettings) : base(contentName, creationSettings)
        {
            _controller = new ImGuiController();
        }

        protected override void OnClose()
        {
            _controller.Dispose();
        }

        protected override void OnUpdate()
        {
            _controller.Update();
        }

        protected override void OnRender()
        {
            _controller.Render();
        }
    }
}
