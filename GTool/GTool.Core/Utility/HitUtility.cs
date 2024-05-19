using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Utility
{
    public static class HitUtility
    {
        public static bool IsWithinRect(Vector2 point, Vector4 rect)
            => point.X > rect.X && point.Y > rect.Y && point.X < rect.Z && point.Y < rect.W;
    }
}
