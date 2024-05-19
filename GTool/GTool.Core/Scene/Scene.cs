using GTool.Scene.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Scene
{
    public class Scene : IDisposable
    {
        public List<Actor> Actors = new List<Actor>();

        public readonly string Name;

        internal Scene(string name)
        {
            Name = name;
        }

        public void Dispose()
        {
            foreach (var actor in Actors)
                actor.Dispose();
            Actors.Clear();
        }
    }
}
