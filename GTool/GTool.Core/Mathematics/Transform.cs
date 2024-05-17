using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Mathematics
{
    public class Transform
    {
        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;

        private Matrix4x4 _matrix;

        private bool _modified;

        public Transform()
        {

        }

        public void Update()
        {
            if (!_modified) return;

            _matrix = Matrix4x4.CreateTranslation(_position);
            _matrix = Matrix4x4.Transform(_matrix, _rotation);
            _matrix *= Matrix4x4.CreateScale(_scale);

            _modified = false;
        }

        public Vector3 Position { get { return _position; } set { _position = value; _modified = true; } }
        public Quaternion Rotation { get { return _rotation; } set { _rotation = value; _modified = true; } }
        public Vector3 Scale { get { return _scale; } set { _scale = value; _modified = true; } }
        public Matrix4x4 Matrix { get { return _matrix; } }
    }
}
