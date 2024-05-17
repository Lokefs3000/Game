using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace GTool.CB
{
    public partial class Main : Form
    {
        private static Main _this;

        private string _currentDataFile;
        private string _currentDataFolder;
        private string _currentContentFolder;

        private bool _hasEditOccured;

        public Main()
        {
            _this = this;
            InitializeComponent();
            ResetData();
        }

        private void OpenDatabase()
        {
            DialogResult result = OpenDBFile.ShowDialog(this);
            if (result != DialogResult.OK)
                return;

            string dfile = Path.GetFullPath(OpenDBFile.FileName);
            string dfolder = Path.GetDirectoryName(dfile);
            string cfolder = Path.Combine(dfolder, "Content");

            if (!Directory.Exists(cfolder))
                return;

            _currentDataFile = dfile;
            _currentDataFolder = dfolder;
            _currentContentFolder = cfolder;

            LoadDatabase();
        }

        private void ResetData()
        {
            _nodeHovered = null;
            NotifyEditOccured(false);

            FileTree.Nodes.Clear();
            FileTree.Nodes.Add(CreateTreeNode("Content", 14, 1, TreeContentMStripSmall, CreateFileData(_currentContentFolder, true)));
            FileTree.Nodes.Add(CreateTreeNode("External", 15, 1, TreeContentMStripSmall, CreateFileData(_currentContentFolder, true)));
        }

        private TreeNode CreateTreeNode(string name, int imageId, int selImageId, ContextMenuStrip? contextMenu, FileData data)
        {
            TreeNode node = new TreeNode(name);
            node.ImageIndex = imageId;
            node.SelectedImageIndex = selImageId;
            node.Tag = data;
            node.Name = data.Name;
            node.ContextMenuStrip = contextMenu;

            return node;
        }

        private FileData CreateFileData(string path, bool fake = false)
        {
            return new FileData
            {
                Name = Path.GetFileName(path),
                FullPath = path,
                IsDirectory = Directory.Exists(path) || fake,
                IsFake = fake,
            };
        }

        private TreeNode? _nodeHovered = null;
        private void FileTreeNodeHover(object sender, TreeNodeMouseHoverEventArgs e) => _nodeHovered = e.Node;

        private void AddFileToActiveNode(string file, bool isLocal)
        {
            if (_nodeHovered == null) return;

            if (_nodeHovered.FullPath.StartsWith("External") || !Path.GetFullPath(file).StartsWith(file))
                isLocal = false;
            else
                isLocal = true;

            if (!isLocal)
            {
                FileData data = (FileData)_nodeHovered.Tag;
                string parent = data.FullPath;
                string copyPath = Path.Combine(parent, Path.GetFileName(file));
                if (File.Exists(copyPath))
                    File.Delete(copyPath);
                File.Copy(file, copyPath);

                TreeNode node = CreateTreeNode(Path.GetFileName(copyPath), DetermineImageId(copyPath), DetermineImageId(copyPath, true), DetermineStripType(copyPath), CreateFileData(copyPath));
                _nodeHovered.Nodes.Add(node);
            }
            else
            {
                TreeNode node = CreateTreeNode(Path.GetFileName(file), DetermineImageId(file), DetermineImageId(file, true), DetermineStripType(file), CreateFileData(file));
                _nodeHovered.Nodes.Add(node);
            }

            NotifyEditOccured(true);
        }

        private void AddFile(bool local)
        {
            if (_nodeHovered != null)
            {
                DialogResult result = AddFileDialog.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    foreach (string file in AddFileDialog.FileNames)
                        AddFileToActiveNode(file, local);
                }

                NotifyEditOccured(true);
            }
        }

        private void ContextMenuAddFileLocal(object sender, EventArgs e) => AddFile(true);
        private void ContextMenuAddFileGlobal(object sender, EventArgs e) => AddFile(false);

        private void ContextMenuAddNewFolder(object sender, EventArgs e)
        {
            if (_nodeHovered != null)
            {
                TreeNode node = new TreeNode("");
                _nodeHovered.Nodes.Add(node);
                node.BeginEdit();
                node.EnsureVisible();
                while (node.IsEditing)
                    Application.DoEvents();

                string name = node.Text;
                string full = Path.Combine(_currentDataFolder, _nodeHovered.FullPath.Replace("External", "Content"), name);
                node.Remove();

                if (_nodeHovered.Tag != null && !((FileData)_nodeHovered.Tag).IsDirectory)
                    _nodeHovered = _nodeHovered.Parent;

                if (Directory.Exists(full))
                {
                    for (int i = 0; i < _nodeHovered.Nodes.Count; i++)
                    {
                        if (_nodeHovered.Nodes[i].Name == name)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(full);
                }

                node = CreateTreeNode(name, DetermineImageId(full), DetermineImageId(full, true), DetermineStripType(full), CreateFileData(full));
                _nodeHovered.Nodes.Add(node);
                FileTree.Update();

                NotifyEditOccured(true);
            }
        }

        private void ContextMenuRename(object sender, EventArgs e)
        {
            if (_nodeHovered != null)
            {
                string oldText = _nodeHovered.Text;

                _nodeHovered.EnsureVisible();
                _nodeHovered.BeginEdit();
                while (_nodeHovered.IsEditing)
                    Application.DoEvents();

                string newText = _nodeHovered.Text;

                FileData data = (FileData)_nodeHovered.Tag;
                string dir = data.IsDirectory ? data.FullPath : Path.GetDirectoryName(data.FullPath);

                if (data.IsDirectory)
                    Directory.Move(dir, Path.Combine(Directory.GetParent(dir).FullName, newText));
                else
                    File.Move(Path.Combine(dir, oldText), Path.Combine(dir, newText));

                data.FullPath = data.IsDirectory ? Path.Combine(Directory.GetParent(dir).FullName, newText) : Path.Combine(dir, newText);
                data.Name = newText;

                _nodeHovered.Tag = data;
                _nodeHovered.Name = newText;

                NotifyEditOccured(true);
            }
        }

        private void ContextMenuRenameTree(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node != null && e.Label != null && e.Node.Tag != null)
            {
                string oldText = e.Node.Name;
                string newText = e.Label;

                FileData data = (FileData)e.Node.Tag;
                string dir = data.IsDirectory ? data.FullPath : Path.GetDirectoryName(data.FullPath);

                if (data.IsDirectory)
                    Directory.Move(dir, Path.Combine(Directory.GetParent(dir).FullName, newText));
                else
                    File.Move(Path.Combine(dir, oldText), Path.Combine(dir, newText));

                data.FullPath = data.IsDirectory ? Path.Combine(Directory.GetParent(dir).FullName, newText) : Path.Combine(dir, newText);
                data.Name = newText;

                e.Node.Tag = data;
                e.Node.Name = newText;

                NotifyEditOccured(true);
            }
        }

        private void ContextMenuRemove(object sender, EventArgs e) => _nodeHovered?.Remove();

        private bool _continueWithBuild = true;

        private void BuildContent(object sender, EventArgs e)
        {
            if (_currentDataFolder == null)
            {
                SaveDatabase();
                if (_currentDataFolder == null)
                    return;
            }

            BuildConfig bc = new BuildConfig();
            bc.ShowDialog();
            bc.Dispose();

            if (bc.Result != DialogResult.OK)
                return;
            FilePostProcess.Quality = bc.Quality;

            PreprocessFiles();
            if (!_continueWithBuild)
            {
                Main.DebugWriteLine("Stopping build..");
                return;
            }

            File.WriteAllText(Path.Combine(_currentDataFolder, "Content.bcf"), string.Empty);
            using FileStream stream = File.OpenWrite(Path.Combine(_currentDataFolder, "Content.bcf"));
            if (stream.CanWrite)
                BCFWriter.WriteBCF(stream, FileTree.Nodes[0], FileTree.Nodes[1]);
        }

        private void PreprocessFiles()
        {
            _continueWithBuild = true;

            string dir = Path.Combine(_currentDataFolder, "Library");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            foreach (TreeNode node in FileTree.Nodes)
                PPFSRecursive(node, dir);
        }

        private void PPFSRecursive(TreeNode root, string dir)
        {
            if (!_continueWithBuild)
                return;

            if (root.Tag != null)
            {
                FileData data = (FileData)root.Tag;

                if (FilePostProcess.AcceptedExtensions.Contains(Path.GetExtension(data.FullPath)))
                {
                    string outfile = Path.Combine(dir, GetLocalPath(data.FullPath).Replace("/", "."));
                    _continueWithBuild = FilePostProcess.Process(data.FullPath, outfile) && _continueWithBuild;
                    data.PPFPath = outfile;
                }
                else
                    data.PPFPath = null;

                root.Tag = data;
            }

            foreach (TreeNode node in root.Nodes)
                PPFSRecursive(node, dir);
        }

        private int DetermineImageId(string file, bool selectedId = false)
        {
            if (Directory.Exists(file))
                return selectedId ? 1 : 2;
            else
            {
                string ext = Path.GetExtension(file);
                if (ext == ".png")
                    return 10;
                else if (ext == ".ttf")
                    return 12;
                else if (ext == ".ogg")
                    return 9;
                else if (ext == ".webm")
                    return 11;
                else if (ext == ".config")
                    return 6;
                else
                    return 0;
            }
        }

        private ContextMenuStrip? DetermineStripType(string file)
        {
            if (Directory.Exists(file))
                return TreeContentMStripLarge;
            else
                return TreeContentMStripMedium;
        }

        private void CloseApplication()
        {
            if (_hasEditOccured)
            {
                DialogResult res = MessageBox.Show($"You have unsaved changes in file: {_currentDataFile}!", "Unsaved changes!", MessageBoxButtons.YesNoCancel);
                switch (res)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        SaveDatabase(false);
                        break;
                    default:
                        break;
                }
            }

            Close();
        }

        private void SaveDatabase(bool asNew = false)
        {
            if (!File.Exists(_currentDataFile) || asNew)
            {
                DialogResult res = SaveDBFile.ShowDialog();

                switch (res)
                {
                    case DialogResult.OK:
                        _currentDataFile = SaveDBFile.FileName;
                        _currentDataFolder = Path.GetDirectoryName(_currentDataFile);
                        _currentContentFolder = Path.Combine(_currentDataFolder, "Content");
                        break;
                    default:
                        return;
                }
            }

            JsonObject root = new JsonObject();
            
            JsonArray files = new JsonArray();
            foreach (TreeNode treeNode in FileTree.Nodes)
                RecursiveSaveArray(files, treeNode);
            root.Add("Files", files);

            File.WriteAllText(_currentDataFile, root.ToJsonString());

            LoadDatabase();
            NotifyEditOccured(false);
        }

        private void RecursiveSaveArray(JsonArray files, TreeNode root)
        {
            if (root.Tag != null)
            {
                FileData data = (FileData)root.Tag;
                if (File.Exists(data.FullPath))
                    files.Add(GetLocalPath(data.FullPath));
            }

            foreach (TreeNode treeNode in root.Nodes)
                RecursiveSaveArray(files, treeNode);
        }

        private void MenubarFileNewAction(object sender, EventArgs e) => ResetData();
        private void MenubarFileOpenAction(object sender, EventArgs e) => OpenDatabase();
        private void MenubarFileSaveAction(object sender, EventArgs e) => SaveDatabase();
        private void MenubarFileSaveAsAction(object sender, EventArgs e) => SaveDatabase(true);

        private void MenubarFileExitAction(object sender, EventArgs e) => CloseApplication();

        private void NotifyEditOccured(bool changed)
        {
            _hasEditOccured = changed;

            string? name = _currentDataFolder == null ? "Unkown" : Path.GetDirectoryName(_currentDataFolder);
            if (_hasEditOccured)
                Text = $"{name}* - Content builder";
            else
                Text = $"{name} - Content builder";
        }

        private string GetLocalPath(string path)
        {
            try
            {
                if (_currentContentFolder == null)
                    return path;
                return path.Substring(_currentContentFolder.Length + 1).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                Main.DebugWriteLine(ex.Message);
                return path;
            }
        }

        private void LoadDatabase()
        {
            if (_currentDataFile == null)
                return;

            ResetData();

            FileTree.SelectedNode = FileTree.Nodes[0];
            _nodeHovered = FileTree.SelectedNode;

            JsonNode root = JsonNode.Parse(File.ReadAllText(_currentDataFile));
            foreach (JsonNode node in root["Files"].AsArray())
            {
                string file = node.GetValue<string>();
                
                if (file.Contains("/"))
                {
                    string[] strings = file.Split('/');
                    string full = _currentContentFolder;

                    foreach (string s in strings)
                    {
                        full = Path.Combine(full, s);
                        FileData data = CreateFileData(full);

                        if (data.IsDirectory)
                        {
                            TreeNode folderNode = CreateTreeNode(data.Name, DetermineImageId(full), DetermineImageId(full, true), DetermineStripType(full), data);
                            FileTree.SelectedNode.Nodes.Add(folderNode);
                            FileTree.SelectedNode = folderNode;
                            _nodeHovered = folderNode;
                        }
                        else
                            AddFileToActiveNode(full, true);
                    }
                }
                else
                    AddFileToActiveNode(Path.Combine(_currentContentFolder, file), true);
            }

            NotifyEditOccured(false);
        }

        private static StringBuilder _log = new StringBuilder();
        public static void DebugWriteLine(string message)
        {
            _log.AppendLine(message);
            _this.textBox1.Text = _log.ToString();
        }
    }
}
