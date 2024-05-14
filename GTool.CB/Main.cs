using System.Xml.Linq;

namespace GTool.CB
{
    public partial class Main : Form
    {
        private string _currentDataFile;
        private string _currentDataFolder;
        private string _currentContentFolder;

        public Main()
        {
            InitializeComponent();
            OpenDatabase();
            ResetData();
        }

        private void OpenDatabase()
        {
            DialogResult result = OpenDBFile.ShowDialog(this);
            if (result != DialogResult.OK)
            {
                Application.Exit();
                Environment.Exit(0);
                return;
            }

            _currentDataFile = Path.GetFullPath(OpenDBFile.FileName);
            _currentDataFolder = Path.GetDirectoryName(_currentDataFile);
            _currentContentFolder = Path.Combine(_currentDataFolder, "Content");

            if (!Directory.Exists(_currentContentFolder))
            {
                Application.Exit();
                Environment.Exit(0);
                return;
            }
        }

        private void ResetData()
        {
            _nodeHovered = null;

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
            }
        }

        private void ContextMenuRemove(object sender, EventArgs e) => _nodeHovered?.Remove();

        private void BuildContent(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(_currentDataFolder, "Content.bcf")))
                File.Delete(Path.Combine(_currentDataFolder, "Content.bcf"));
            using FileStream stream = File.OpenWrite(Path.Combine(_currentDataFolder, "Content.bcf"));
            if (stream.CanWrite)
                BCFWriter.WriteBCF(stream, FileTree.Nodes[0], FileTree.Nodes[1]);
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
    }
}
