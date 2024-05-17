using GTool.Scene.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Scene.Components
{
    public class Component : IDisposable
    {
        public readonly Actor Parent;
        public readonly int Id;

        internal Component(Actor parent, int id)
        {
            Parent = parent;
            Id = id;
        }

        public virtual void Dispose()
        {
            
        }
    }
}
