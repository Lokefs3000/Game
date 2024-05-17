﻿using GTool;
using GTool.Content;
using GTool.Debugging;
using GTool.Windowing;
using Serilog;
using System.Numerics;

namespace Game
{
    internal class GameApp : GApplication
    {
        public GameApp(in string contentName, in WindowCreationSettings creationSettings) : base(contentName, creationSettings)
        {
            if (!Vector.IsHardwareAccelerated)
                Log.Warning("SIMD instructions not supported! This may cause some performance penalties..");
            else
                Log.Information("SIMD instruction are supported! This could improve performance..");

            SceneManager.New(string.Empty);
        }

        protected override void OnClose()
        {
            
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnRender()
        {
            
        }
    }
}
