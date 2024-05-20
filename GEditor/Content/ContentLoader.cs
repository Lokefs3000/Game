using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GEditor.Content
{
    internal class ContentLoader : GTool.Content.ContentLoader, IDisposable
    {
        private Dictionary<string, ContentDir> _filesystem = new Dictionary<string, ContentDir>();
        private Dictionary<string, PathData> _filesystemPaths = new Dictionary<string, PathData>();

        public int FilesystemSize { get { return _filesystemPaths.Count; } }
        public Dictionary<string, PathData> Files { get { return _filesystemPaths; } }

        public event OnFilesystemChange? FilesystemChanged;
        public delegate void OnFilesystemChange(ContentLoader loader);

        public ContentLoader() : base()
        {
            _instance = this;
        }

        public void Dispose()
        {
            foreach (var dir in _filesystem.Values)
                dir.Watcher.Dispose();
        }

        protected override void Append(Assembly assembly, string name)
        {
            name = name.Replace("\\", "/");
            string dirname = name.Substring(name.LastIndexOf("/") + 1) ?? string.Empty;

            ContentDir content = new ContentDir();
            content.Name = dirname;
            content.Path = Path.GetFullPath(name);

            content.Files = new Dictionary<string, FileData>();

            foreach (string file in Directory.GetFiles(name, "*.*", SearchOption.AllDirectories))
            {
                string filename = $"{dirname}/{file.Substring(name.Length + 1)}".Replace('\\', '/');
                content.Files.Add(filename, new FileData
                {
                    Path = file
                });
                _filesystemPaths.Add(Path.GetFullPath(file), new PathData { Value = dirname, IsDirectory = false });
            }

            foreach (string dir in Directory.GetDirectories(name, "*.*", SearchOption.AllDirectories))
            {
                _filesystemPaths.Add(Path.GetFullPath(dir), new PathData { Value = dirname, IsDirectory = true });
            }

            FileSystemWatcher watcher = new FileSystemWatcher(name);
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;

            watcher.EnableRaisingEvents = true;
            content.Watcher = watcher;

            _filesystem.Add(dirname, content);

            FilesystemChanged?.Invoke(this);
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string full = Path.GetFullPath(e.OldFullPath);
            string nfull = Path.GetFullPath(e.FullPath);

            foreach (KeyValuePair<string, ContentDir> content in _filesystem)
            {
                if (full.StartsWith(content.Value.Path))
                {
                    if (_filesystemPaths[full].IsDirectory)
                    {
                        for (int i = 0; i < _filesystemPaths.Count; i++)
                        {
                            KeyValuePair<string, PathData> kvp = _filesystemPaths.ElementAt(i);
                            if (kvp.Key.StartsWith(full) && kvp.Key != full)
                            {
                                PathData old = _filesystemPaths[kvp.Key];

                                string newkey = nfull + kvp.Key.Substring(full.Length);
                                string local = $"{content.Value.Name}/{kvp.Key.Substring(content.Value.Path.Length + 1)}".Replace('\\', '/');

                                if (!content.Value.Files.TryGetValue(local, out FileData value))
                                    Log.Warning("Failed to remove file from filesystem: {@Path}", full);
                                else
                                {
                                    content.Value.Files.Remove(local);
                                    local = $"{content.Value.Name}/{newkey.Substring(content.Value.Path.Length + 1)}".Replace('\\', '/');

                                    value.Path = newkey;
                                    content.Value.Files.TryAdd(local, value);

                                    _filesystemPaths.Remove(kvp.Key);
                                    _filesystemPaths.Add(newkey, old);
                                }
                            }
                        }

                        PathData fold = _filesystemPaths[full];
                        _filesystemPaths.Remove(full);
                        _filesystemPaths.Add(nfull, fold);
                    }
                    else
                    {
                        string glocal = $"{content.Value.Name}/{full.Substring(content.Value.Path.Length + 1)}";
                        if (!content.Value.Files.TryGetValue(glocal, out FileData gvalue))
                            Log.Warning("Failed to remove file from filesystem: {@Path}", full);
                        else
                        {
                            content.Value.Files.Remove(glocal);
                            glocal = $"{content.Value.Name}/{nfull.Substring(content.Value.Path.Length + 1)}";

                            gvalue.Path = nfull;
                            content.Value.Files.TryAdd(glocal, gvalue);

                            _filesystemPaths.Remove(full);
                            _filesystemPaths.Add(nfull, new PathData { Value = content.Key, IsDirectory = false });
                        }
                    }

                    break;
                }
            }

            FilesystemChanged?.Invoke(this);
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string full = Path.GetFullPath(e.FullPath);

            foreach (KeyValuePair<string, ContentDir> content in _filesystem)
            {
                if (full.StartsWith(content.Value.Path))
                {
                    if (_filesystemPaths[full].IsDirectory)
                    {
                        for (int i = 0; i < _filesystemPaths.Count; i++)
                        {
                            KeyValuePair<string, PathData> kvp = _filesystemPaths.ElementAt(i);
                            if (kvp.Key.StartsWith(full))
                            {
                                _filesystemPaths.Remove(kvp.Key);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        string local = $"{content.Value.Name}/{full.Substring(content.Value.Path.Length + 1)}";
                        if (!content.Value.Files.Remove(local))
                            Log.Warning("Failed to remove file from filesystem: {@Path}", full);
                    }

                    break;
                }
            }

            _filesystemPaths.Remove(full);
            FilesystemChanged?.Invoke(this);
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string full = Path.GetFullPath(e.FullPath);

            foreach (KeyValuePair<string, ContentDir> content in _filesystem)
            {
                if (full.StartsWith(content.Value.Path))
                {
                    if (Directory.Exists(e.FullPath))
                    {
                        foreach (string file in Directory.GetFiles(full, "*.*", SearchOption.AllDirectories))
                        {
                            string filename = $"{content.Value.Name}/{full.Substring(content.Value.Path.Length + 1)}";
                            content.Value.Files.TryAdd(filename, new FileData
                            {
                                Path = file
                            });
                            _filesystemPaths.TryAdd(Path.GetFullPath(file), new PathData { Value = content.Key, IsDirectory = false });
                        }

                        foreach (string file in Directory.GetDirectories(full, "*.*", SearchOption.AllDirectories))
                        {
                            _filesystemPaths.TryAdd(Path.GetFullPath(file), new PathData { Value = content.Key, IsDirectory = true });
                        }

                        _filesystemPaths.TryAdd(full, new PathData { Value = content.Key, IsDirectory = true });
                    }
                    else
                    {
                        if (!content.Value.Files.TryAdd($"{content.Value.Name}/{full.Substring(content.Value.Path.Length + 1)}", new FileData
                        {
                            Path = full
                        }))
                            Log.Warning("Failed to add file to filesystem: {@Path}", full);

                        _filesystemPaths.TryAdd(full, new PathData { Value = content.Key, IsDirectory = false });
                    }

                    break;
                }
            }

            FilesystemChanged?.Invoke(this);
        }

        public ContentDir? GetContentDirectory(string path)
        {
            if (_filesystem.TryGetValue(path, out var contentDirectory))
                return contentDirectory;
            return null;
        }

        protected override byte[] GetFileBytes(string path)
        {
            string dir = path.Substring(0, path.IndexOf("/")) ?? string.Empty;
            if (_filesystem.TryGetValue(dir, out ContentDir content))
            {
                if (content.Files.TryGetValue(path, out FileData file))
                {
                    return File.ReadAllBytes(file.Path);
                }

                Log.Error("Failed to find file: \"{@Path}\"", path);
                return Array.Empty<byte>();
            }

            Log.Error("Failed to find directory: \"{@Path}\"", path);
            return Array.Empty<byte>();
        }

        public struct ContentDir
        {
            public string Name;
            public string Path;

            public Dictionary<string, FileData> Files;
            public FileSystemWatcher Watcher;
        }

        public struct FileData
        {
            public string Path;
        }

        public struct PathData
        {
            public string Value;
            public bool IsDirectory;
        }
    }
}
