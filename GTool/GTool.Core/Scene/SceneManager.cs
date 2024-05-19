using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTool.Scene
{
    public class SceneManager : IDisposable
    {
        private List<Scene> _loadedScenes = new List<Scene>();
        public List<Scene> Scenes { get => _loadedScenes; }

        public SceneManager()
        {
            New("Untitled");
        }

        public void Dispose()
        {
            foreach (Scene scene in _loadedScenes)
                scene.Dispose();
            _loadedScenes.Clear();
        }

        public Scene New(string name)
        {
            Scene scene = new Scene(name);
            //if (name != string.Empty)
            _loadedScenes.Add(scene);
            return scene;
        }
    }
}
