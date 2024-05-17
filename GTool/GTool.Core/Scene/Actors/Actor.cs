using GTool.Mathematics;
using GTool.Scene.Components;

namespace GTool.Scene.Actors
{
    public class Actor : IDisposable
    {
        private List<Tuple<int, Type>> _components = new List<Tuple<int, Type>>();

        public Transform Transform { get; private set; }

        public Actor()
        {
            Transform = new Transform();
        }

        public virtual void Dispose()
        {
            foreach (Tuple<int, Type> component in _components)
                ECS.Destroy(component.Item2, component.Item1);
            _components.Clear();
        }

        public virtual void Update()
        {

        }

        public virtual void Render()
        {

        }

        public void AddComponent<TComponent>()
            where TComponent : Component, new()
        {
            int id = ECS.Create<TComponent>(this);
            _components.Add(new Tuple<int, Type>(id, typeof(TComponent)));
        }

        public void RemoveComponent<TComponent>(TComponent comp)
            where TComponent : Component
        {
            ECS.Destroy<TComponent>(comp.Id);
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Item1 == comp.Id)
                {
                    _components.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
