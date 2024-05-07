using ImageMagick;
using K4os.Compression.LZ4;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace ContentBuilder
{
    public partial class Main : Form
    {
        private ImageList _primaryList;
        private string _openProject;

        public Main()
        {
            InitializeComponent();
            MagickNET.Initialize();

            _primaryList = new ImageList();
            _openProject = "";
        }

        public void Main_Load(object sender, EventArgs e)
        {
            _primaryList.Images.AddRange([
                GetStockIcon(StockIconId.Folder),
                GetStockIcon(StockIconId.FolderOpen),
                GetStockIcon(StockIconId.MixedFiles),
                ]);

            treeView1.ImageList = _primaryList;

            PrintLog("Loaded resources!");
        }

        private Image GetStockIcon(StockIconId id)
        {
            Icon icon = SystemIcons.GetStockIcon(StockIconId.Folder);
            Image image = icon.ToBitmap();
            icon.Dispose();
            return image;
        }

        private void SaveDatabaseAs()
        {
            throw new NotImplementedException();
        }

        private void SaveDatabase()
        {
            throw new NotImplementedException();
        }

        private void OpenDatabase()
        {
            DialogResult res = OpenDatabaseDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                try
                {
                    string root = Directory.GetParent(OpenDatabaseDialog.FileName).FullName;

                    List<string> avail = new List<string>();
                    foreach (string item in Directory.GetFiles(Path.Combine(root, "Content/"), "*.*", SearchOption.AllDirectories))
                        avail.Add(item.Replace("\\", "/").Substring(root.Length + 1));

                    Dictionary<string, TreeNode> dirTree = new Dictionary<string, TreeNode>();
                    TreeNode externalTree = new TreeNode("External");
                    externalTree.Checked = true;

                    JsonNode? file = JsonNode.Parse(File.ReadAllText(OpenDatabaseDialog.FileName));
                    Debug.Assert(file != null);

                    JsonNode? files = file["Files"];
                    Debug.Assert(files != null);
                    foreach (JsonNode? node in files.AsArray())
                    {
                        Debug.Assert(node != null);

                        FileInformation info = new FileInformation();
                        info.Path = node["Path"]!.ToString();
                        info.Name = Path.GetFileNameWithoutExtension(info.Path);

                        info.IsLocal = avail.Contains(info.Path);
                        info.DoesExist = Path.Exists(info.Path);

                        string ext = Path.GetExtension(info.Path);
                        if (ext == ".png")
                            info.Type = FileType.Image;
                        else if (ext == ".hlsl")
                            info.Type = FileType.Shader;

                        switch (info.Type)
                        {
                            case FileType.Image:
                                {
                                    ImageInformation image = new ImageInformation();

                                    MagickImageInfo imageInfo = new MagickImageInfo(Path.Combine(root, info.Path));
                                    image.Width = imageInfo.Width;
                                    image.Height = imageInfo.Height;
                                    image.MaxMips = (int)(Math.Log2(Math.Max(image.Width, image.Height)) + 1);

                                    image.Filter = (Filtering)node["Filtering"].GetValue<int>();
                                    image.Wrap = (Wrapping)node["Wrapping"].GetValue<int>();
                                    image.MipMaps = Math.Clamp(node["MipLevels"].GetValue<int>(), 0, image.MaxMips);

                                    info.Meta = image;
                                }
                                break;
                            case FileType.Shader:
                                break;
                            default:
                                break;
                        }

                        TreeNode currNode = new TreeNode(info.Name);
                        currNode.Checked = true;
                        currNode.Name = info.Name;
                        currNode.Tag = info;
                        currNode.ContextMenuStrip = contextMenuStrip2;

                        currNode.SelectedImageIndex = currNode.ImageIndex = 2;

                        if (info.IsLocal)
                        {
                            string[] deep = info.Path.Split('/');

                            string? prev = null;
                            for (int i = 0; i < deep.Length - 1; i++)
                                if (!dirTree.ContainsKey(deep[i]))
                                {
                                    TreeNode next = new TreeNode(deep[i]);
                                    next.Name = deep[i];
                                    next.Checked = true;
                                    next.ContextMenuStrip = contextMenuStrip1;

                                    next.ImageIndex = 0;
                                    next.SelectedImageIndex = 1;

                                    if (prev != null)
                                    {
                                        TreeNode prevNode = dirTree[prev];
                                        prevNode.Nodes.Add(next);
                                        dirTree[prev] = prevNode;
                                    }

                                    dirTree.Add(deep[i], next);
                                    prev = deep[i];
                                }

                            TreeNode rootNode = dirTree[deep[deep.Length - 2]];
                            rootNode.Nodes.Add(currNode);
                            dirTree[deep[deep.Length - 2]] = rootNode;
                        }
                        else
                        {
                            externalTree.Nodes.Add(currNode);
                        }
                    }

                    ResetAll();

                    if (dirTree.TryGetValue("Content", out TreeNode? content) && content != null)
                        treeView1.Nodes.Add(content);
                    treeView1.Nodes.Add(externalTree);

                    _openProject = root;
                    Text = $"{OpenDatabaseDialog.FileName} - Content builder";
                    PrintLog("Loaded database!");
                }
                catch (Exception ex)
                {
                    PrintLog(ex.Message);
#if DEBUG
                    throw;
#endif
                }
            }
        }

        private void ResetAll()
        {
            treeView1.Nodes.Clear();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            _primaryList.Dispose();
        }

        private void PrintLog(string msg)
        {
            Debug.WriteLine(msg);

            List<string> lines = new List<string>(log.Lines)
            {
                msg
            };
            log.Lines = lines.ToArray();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e) => ResetAll();
        private void openToolStripMenuItem_Click(object sender, EventArgs e) => OpenDatabase();
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) => SaveDatabase();
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) => SaveDatabaseAs();

        private void buildToolStripMenuItem_Click(object sender, EventArgs e) => BuildDatabase();

        private void clearToolStripMenuItem_Click(object sender, EventArgs e) => log.Lines = Array.Empty<string>();

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            FileInformation? info = (FileInformation?)(e.Node?.Tag);
            if (info != null)
            {
                switch (info?.Type)
                {
                    case FileType.Image:
                        {
                            foreach (Control ctrl in flowLayoutPanel1.Controls)
                                ctrl.Dispose();
                            flowLayoutPanel1.Controls.Clear();

                            ImageInformation imageInfo = (ImageInformation)(info?.Meta);

                            AddProperty("Filter:", ValueType.Dropdown, imageInfo.Filter);
                            AddProperty("Wrap:", ValueType.Dropdown, imageInfo.Wrap);
                            AddProperty("Mip levels:", ValueType.Slider, val => imageInfo.MipMaps = (int)val, 0, imageInfo.MaxMips, imageInfo.MipMaps);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void BuildDatabase()
        {
            try
            {
                List<FileInformation> files = new List<FileInformation>();

                foreach (TreeNode node in treeView1.Nodes)
                {
                    RecursiveTreeNodes(files, node);
                }

                if (!Directory.Exists("temp"))
                {
                    Directory.CreateDirectory("temp");
                }

                {
                    List<FileBuildMeta> meta = new List<FileBuildMeta>();

                    foreach (FileInformation file in files)
                    {
                        FileBuildMeta tempMeta = new FileBuildMeta();

                        switch (file.Type)
                        {
                            case FileType.Image:
                                {
                                    using MagickImage image = new MagickImage(file.IsLocal ? Path.Combine(_openProject, file.Path) : file.Path, MagickFormat.Rgba);
                                    using IPixelCollection<byte> pixels = image.GetPixels();

                                    byte[] inputs = pixels.ToArray();
                                    byte[] output = new byte[LZ4Codec.MaximumOutputSize(inputs.Length)];
                                    int length = LZ4Codec.Encode(inputs, 0, inputs.Length, output, 0, output.Length);

                                    string modname = Path.Combine("temp/", file.Path.Replace("/", ".")) + ".raw";
                                    using FileStream stream = File.OpenWrite(modname);

                                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
                                    {
                                        ImageInformation info = (ImageInformation)file.Meta;

                                        writer.Write([82, 73, 70, 0]);
                                        writer.Write((ushort)1);
                                        writer.Write((byte)info.Filter);
                                        writer.Write((byte)info.Wrap);
                                        writer.Write((byte)info.MipMaps);
                                        writer.Write((ushort)info.Width);
                                        writer.Write((ushort)info.Height);
                                    }
                                    
                                    stream.Write(output, 0, length);

                                    tempMeta.NewPath = modname;
                                    tempMeta.Size = length;
                                }
                                break;
                            case FileType.Shader:
                                break;
                            default:
                                {
                                    using FileStream stream = File.OpenRead(file.IsLocal ? Path.Combine(_openProject, file.Path) : file.Path);
                                    stream.Seek(0, SeekOrigin.End);

                                    tempMeta.NewPath = file.Path;
                                    tempMeta.Size = stream.Position;
                                }
                                break;
                        }

                        meta.Add(tempMeta);
                    }

                    for (int i = 0; i < files.Count; i++)
                    {
                        FileInformation temp = files[i];
                        temp.TempMeta = meta[i];
                        files[i] = temp;
                    }
                }

                File.Delete(Path.Combine(_openProject, "Content.bcf"));
                using (FileStream stream = File.OpenWrite(Path.Combine(_openProject, "Content.bcf")))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    {
                        writer.Write([(byte)66, (byte)67, (byte)70, (byte)0]);
                        writer.Write((ushort)1);
                        writer.Write(files.Count);

                        foreach (FileInformation file in files)
                        {
                            writer.Write(file.Name);   //name
                            writer.Write(0UL);        //offset
                            writer.Write(((FileBuildMeta)file.TempMeta).Size); //size
                        }

                        long offset = stream.Position;
                        stream.Position = 10;

                        foreach (FileInformation file in files)
                        {
                            writer.Write(file.Name);   //name
                            writer.Write(offset);     //offset
                            writer.Write(((FileBuildMeta)file.TempMeta).Size); //size

                            offset += ((FileBuildMeta)file.TempMeta).Size;
                        }
                    }

                    foreach (FileInformation file in files)
                    {
                        byte[] bytes = File.ReadAllBytes(((FileBuildMeta)file.TempMeta).NewPath);
                        stream.Write(bytes);
                    }
                }

                foreach (string file in Directory.GetFiles("temp", "*.*", SearchOption.AllDirectories))
                    File.Delete(file);
                Directory.Delete("temp", true);

                PrintLog($"Build content to: {Path.Combine(_openProject, "Content.bcf")}");
            }
            catch (Exception ex)
            {
                PrintLog(ex.Message);
#if DEBUG
                throw;
#endif
            }
        }

        private void RecursiveTreeNodes(List<FileInformation> files, TreeNode root)
        {
            foreach(TreeNode node in root.Nodes)
            {
                if (node.Tag != null)
                    files.Add((FileInformation)node.Tag);
                RecursiveTreeNodes(files, node);
            }
        }

        private enum ValueType
        {
            Number,
            Slider,
            String,
            Bool,
            Dropdown
        }

        private void AddProperty(string name, ValueType type, Action<object> onChange, params object[] values)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            panel.Height = 24;
            panel.Width = flowLayoutPanel1.Width;

            Label label = new Label();
            label.Text = name;
            label.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top;
            panel.Controls.Add(label);

            switch (type)
            {
                case ValueType.Number:
                    {

                    }
                    break;
                case ValueType.Slider:
                    {
                        TrackBar trackBar = new TrackBar();
                        trackBar.Dock = DockStyle.Right;
                        trackBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top;
                        trackBar.Location = new Point(panel.Width - trackBar.Width - 8, trackBar.Location.Y);
                        trackBar.Value = (int)values[2];
                        trackBar.Maximum = (int)values[1];
                        trackBar.Minimum = (int)values[0];
                        trackBar.TickFrequency = 1;
                        trackBar.LargeChange = 1;
                        trackBar.SmallChange = 1;
                        trackBar.ValueChanged += (sender, args) => onChange.Invoke(trackBar.Value);
                        panel.Controls.Add(trackBar);
                    }
                    break;
                case ValueType.String:
                    {

                    }
                    break;
                case ValueType.Bool:
                    {

                    }
                    break;
                case ValueType.Dropdown:
                default:
                    break;
            }

            flowLayoutPanel1.Controls.Add(panel);
        }

        private void AddProperty<TEnum>(string name, ValueType type, TEnum selected)
            where TEnum : struct, Enum
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            panel.Height = 24;
            panel.Width = flowLayoutPanel1.Width;

            Label label = new Label();
            label.Text = name;
            label.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top;
            panel.Controls.Add(label);

            ComboBox comboBox = new ComboBox();
            foreach (TEnum @enum in Enum.GetValues<TEnum>())
                comboBox.Items.Add(@enum);
            comboBox.Dock = DockStyle.Right;
            comboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top;
            comboBox.Location = new Point(panel.Width - comboBox.Width - 8, comboBox.Location.Y);
            comboBox.SelectedText = selected.ToString();
            panel.Controls.Add(comboBox);

            flowLayoutPanel1.Controls.Add(panel);
        }

        private enum FileType
        {
            Image,
            Shader,
        }

        private struct FileInformation
        {
            public string Path;
            public string Name;

            public bool IsLocal;
            public bool DoesExist;

            public FileType Type;
            public object? Meta;

            public object? TempMeta;
        }

        private struct FileBuildMeta
        {
            public long Size;
            public string NewPath;
        }

        private enum Filtering
        {
            MIN_MAG_MIP_POINT = 0,
            MIN_MAG_POINT_MIP_LINEAR = 0x1,
            MIN_POINT_MAG_LINEAR_MIP_POINT = 0x4,
            MIN_POINT_MAG_MIP_LINEAR = 0x5,
            MIN_LINEAR_MAG_MIP_POINT = 0x10,
            MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x11,
            MIN_MAG_LINEAR_MIP_POINT = 0x14,
            MIN_MAG_MIP_LINEAR = 0x15,
            ANISOTROPIC = 0x55,
        }

        private enum Wrapping
        {
            ADDRESS_WRAP = 1,
            ADDRESS_MIRROR,
            ADDRESS_CLAMP,
            ADDRESS_BORDER,
            ADDRESS_MIRROR_ONCE
        }

        private struct ImageInformation
        {
            public Filtering Filter;
            public Wrapping Wrap;
            public int MipMaps;

            public int Width;
            public int Height;
            public int MaxMips;
        }
    }
}
