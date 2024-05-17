using GTool.Scene.Actors;
using GTool.Scene.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Scene
{
    internal class ECS
    {
        private static Dictionary<int, ICArray> _array = new Dictionary<int, ICArray>();

        public static int Create<TComponent>(Actor parent)
            where TComponent : Component, new()
        {
            int hId = typeof(TComponent).GUID.GetHashCode();
            if (!_array.TryGetValue(hId, out ICArray? arr) || arr == null)
            {
                arr = new CArray<TComponent>();
                _array.Add(hId, arr);
            }

            ConstructorInfo ctor = typeof(TComponent).GetConstructor([typeof(Actor), typeof(int)]);
            TComponent component = (TComponent)ctor.Invoke([parent, ((CArray<TComponent>)arr).RandomId()]);

            ((CArray<TComponent>)arr).Add(component);
            return component.Id;
        }

        public static void Destroy<TComponent>(int id)
            where TComponent : Component => Destroy(typeof(TComponent), id);

        public static void Destroy(Type type, int id)
        {
            int hId = type.GUID.GetHashCode();
            if (!_array.TryGetValue(hId, out ICArray? arr) || arr == null)
            {
                return;
            }

            Component component = arr.GetComponent(id);
            component.Dispose();

            arr.RemoveComponent(component.Id);
        }

        private interface ICArray
        {
            public int RandomId();
            public void Add(Component component);
            public Component GetComponent(int id);
            public void RemoveComponent(int id);
        }

        private class CArray<TComponent> : ICArray
            where TComponent : Component
        {
            public List<Tuple<int, TComponent>> Contained = new List<Tuple<int, TComponent>>();

            public int RandomId()
            {
                int rng = Random.Shared.Next(int.MinValue, int.MaxValue);
                while (true)
                {
                    bool found = false;
                    foreach (Tuple<int, TComponent> pair in Contained)
                        if (pair.Item1 == rng)
                        {
                            found = true;
                            break;
                        }

                    if (found)
                        rng = Random.Shared.Next(int.MinValue, int.MaxValue);
                    else
                        break;
                }

                return rng;
            }

            public void Add(Component component)
            {
                Contained.Add(new Tuple<int, TComponent>(component.Id, (TComponent)component));
            }

            public Component GetComponent(int id)
            {
                foreach (Tuple<int, TComponent> pair in Contained)
                {
                    if (pair.Item1 == id)
                        return pair.Item2;
                }

                throw new KeyNotFoundException("Failed to find component with id!");
            }

            public void RemoveComponent(int id)
            {
                for (int i = 0; i < Contained.Count; i++)
                {
                    if (Contained[i].Item1 == id)
                    {
                        Contained.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
