using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GEditor.Content
{
    internal class ContentLoader : GTool.Content.ContentLoader, IDisposable
    {
        private Dictionary<string, FileData> _filesystem = new Dictionary<string, FileData>();
        private Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        public int FilesystemSize { get { return _filesystem.Count; } }

        public ContentLoader() : base()
        {
            _instance = this;
        }

        public void Dispose()
        {
            foreach (var watcher in _watchers.Values)
                watcher.Dispose();
        }

        protected override void Append(Assembly assembly, string name)
        {
            string dirname = name.Substring(name.LastIndexOf("/") + 1);

            foreach (string file in Directory.GetFiles(name, "*.*", SearchOption.AllDirectories))
            {
                string filename = $"{dirname}/{file.Substring(name.Length + 1).Replace('\\', '/')}";
                _filesystem.Add(filename, new FileData
                {
                    Path = file
                });
            }

            FileSystemWatcher watcher = new FileSystemWatcher(name);
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;

            watcher.EnableRaisingEvents = true;
            _watchers.Add(name, watcher);
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            FileSystemWatcher watcher = (FileSystemWatcher)sender;
            string dirname = Path.GetDirectoryName(watcher.Path.Substring(watcher.Path.LastIndexOf("/"))) ?? "";
            //make sure that this actually still works!
            //shoud likely be replaced with a smarter solution..
            if (Directory.Exists(e.FullPath))
            {
                foreach (string file in Directory.GetFiles(e.FullPath, "*.*", SearchOption.AllDirectories))
                {
                    //gosh this wont work! who would have guessed?
                    string filename = $"{dirname}/{e.OldFullPath.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                    if (_filesystem.ContainsKey(filename))
                    {
                        FileData store = _filesystem[filename];
                        _filesystem.Remove(filename);

                        filename = $"{dirname}/{e.FullPath.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                        _filesystem.Add(filename, store);
                    }
                }
            }
            else
            {
                string filename = $"{dirname}/{e.OldFullPath.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                if (_filesystem.ContainsKey(filename))
                {
                    FileData store = _filesystem[filename];
                    _filesystem.Remove(filename);

                    filename = $"{dirname}/{e.FullPath.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                    _filesystem.Add(filename, store);
                }
            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher watcher = (FileSystemWatcher)sender;
            string dirname = Path.GetDirectoryName(watcher.Path.Substring(watcher.Path.LastIndexOf("/"))) ?? "";
            //make sure that this actually still works!
            //shoud likely be replaced with a smarter solution..
            if (Directory.Exists(e.FullPath))
            {
                foreach (string file in Directory.GetFiles(e.FullPath, "*.*", SearchOption.AllDirectories))
                {
                    string filename = $"{dirname}/{file.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                    if (_filesystem.ContainsKey(filename))
                        _filesystem.Remove(filename);
                }
            }
            else
            {
                string filename = $"{dirname}/{e.FullPath.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                if (_filesystem.ContainsKey(filename))
                    _filesystem.Remove(filename);
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher watcher = (FileSystemWatcher)sender;
            string dirname = Path.GetDirectoryName(watcher.Path.Substring(watcher.Path.LastIndexOf("/"))) ?? "";

            if (Directory.Exists(e.FullPath))
            {
                foreach (string file in Directory.GetFiles(e.FullPath, "*.*", SearchOption.AllDirectories))
                {
                    string filename = $"{dirname}/{file.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                    _filesystem.Add(filename, new FileData
                    {
                        Path = file
                    });
                }
            }
            else
            {
                string filename = $"{dirname}/{e.FullPath.Substring(watcher.Path.Length + 1).Replace('\\', '/')}";
                _filesystem.Add(filename, new FileData
                {
                    Path = e.FullPath
                });
            }
        }

        protected override byte[] GetFileBytes(string path)
        {
            if (_filesystem.TryGetValue(path, out FileData value))
            {
                return File.ReadAllBytes(value.Path);
            }

            Log.Error("Failed to find file: \"{@Path}\"", path);
            return Array.Empty<byte>();
        }

        private struct FileData
        {
            public string Path;
        }
    }
}
