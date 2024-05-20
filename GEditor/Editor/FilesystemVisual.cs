using GEditor.Content;
using GTool.Interface;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GEditor.Editor
{
    internal class FilesystemVisual
    {
        private DirectoryData _cachedFilesysRoot;
        private GEditor _editor;

        public FilesystemVisual(GEditor editor)
        {
            _cachedFilesysRoot = new DirectoryData();
            _editor = editor;

            editor.Loader.FilesystemChanged += RebuildFileSystem;
            RebuildFileSystem(editor.Loader);
        }

        private void RebuildFileSystem(ContentLoader loader)
        {
            ContentLoader.ContentDir? dir = loader.GetContentDirectory(_editor.ProjectName);
            if (dir != null)
            {
                Log.Debug("Rebuilding filesystem..");

                _cachedFilesysRoot = new DirectoryData()
                {
                    Name = _editor.ProjectName,
                    Path = _editor.ProjectDir,
                    Hash = _editor.ProjectDir.GetHashCode()
                };

                Dictionary<string, ContentLoader.PathData> files = loader.Files;
                foreach (KeyValuePair<string, ContentLoader.PathData> file in files)
                {
                    if (!file.Key.StartsWith(dir.Value.Path))
                        continue;

                    DirectoryData dirdata = GetDirectoryTo(file.Key, dir.Value.Path);
                    if (!file.Value.IsDirectory)
                    {
                        string gpath = Path.Combine(dirdata.Path, dir.Value.Name);

                        dirdata.Files.Add(new FileData
                        {
                            Name = dir.Value.Name,
                            Path = gpath,
                            Hash = gpath.GetHashCode()
                        });
                    }
                }
            }
        }

        private DirectoryData GetDirectoryTo(string name, string path)
        {
            string[] tokens = name.Substring(path.Length + 1).Split(Path.DirectorySeparatorChar);

            DirectoryData data = _cachedFilesysRoot;
            foreach (string token in tokens)
            {
                if (!data.Directories.TryGetValue(token, out DirectoryData newData))
                {
                    string gpath = Path.Combine(data.Path, token);

                    newData = new DirectoryData
                    {
                        Name = token,
                        Path = gpath,
                        Hash = gpath.GetHashCode()
                    };
                }
                else
                {
                    data = newData;
                    continue;
                }

                data.Directories.Add(token, newData);
                data = newData;
            }

            return data;
        }

        public void Draw(GEditor editor)
        {
            Gui.RectLC(new Vector4(0.0f, 415.0f, 244.0f, 305.0f), 0xff323232);

            Gui.RectLC(new Vector4(2.0f, 417.0f, 240.0f, 30.0f), 0xff272727);
            Gui.RectLC(new Vector4(214.0f, 419.0f, 26.0f, 26.0f), 0xff323232);
            Gui.RectLC(new Vector4(4.0f, 419.0f, 208.0f, 26.0f), 0xff323232);
            Gui.Text("Search..", new Vector2(6.0f, 418.0f), 18.0f, 0xff787878, 2.0f);

            Gui.RectLC(new Vector4(2.0f, 449.0f, 240.0f, 269.0f), 0xff272727);
            Gui.RectLC(new Vector4(4.0f, 451.0f, 236.0f, 265.0f), 0xff2e2e2e);

            _cursor = Vector2.Zero;
            DirectoryTree(_cachedFilesysRoot);
        }

        private Vector2 _cursor;

        private void DirectoryTree(DirectoryData data)
        {
            Gui.RectLC(new Vector4(6.0f + _cursor.X, 453.0f + _cursor.Y, 900.0f, 19.0f), 0xff292929);

            Gui.RectLC(new Vector4(8.0f + _cursor.X, 455.0f + _cursor.Y, 15.0f, 15.0f), 0xffffffff);
            Gui.RectLC(new Vector4(25.0f + _cursor.X, 455.0f + _cursor.Y, 15.0f, 15.0f), 0xffffffff);
            Gui.Text(data.Name, new Vector2(42.0f + _cursor.X, 455.0f + _cursor.Y), 14.5f, 0xffffffff);

            _cursor.Y += 21.0f;
            _cursor.X += 15.0f;

            for (int i = 0; i < data.Directories.Count; i++)
                DirectoryTree(data.Directories.ElementAt(i).Value);

            _cursor.X -= 15.0f;
        }

        private struct DirectoryData
        {
            public string Name;
            public string Path;

            public int Hash;

            public Dictionary<string, DirectoryData> Directories;
            public List<FileData> Files;

            public DirectoryData()
            {
                Name = string.Empty;
                Path = string.Empty;
                Directories = new Dictionary<string, DirectoryData>();
                Files = new List<FileData>();
            }
        }

        private struct FileData
        {
            public string Name;
            public string Path;

            public int Hash;
        }
    }
}
