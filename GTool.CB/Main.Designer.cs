namespace GTool.CB
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            TreeNode treeNode1 = new TreeNode("Content", 14, 1);
            TreeNode treeNode2 = new TreeNode("External", 15, 1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            TreeContentMStripSmall = new ContextMenuStrip(components);
            addFileToolStripMenuItem = new ToolStripMenuItem();
            fileToolStripMenuItem1 = new ToolStripMenuItem();
            folderToolStripMenuItem = new ToolStripMenuItem();
            newFolderToolStripMenuItem = new ToolStripMenuItem();
            splitContainer1 = new SplitContainer();
            FileTree = new TreeView();
            TreeImageList = new ImageList(components);
            MainWindowMStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            buildToolStripMenuItem = new ToolStripMenuItem();
            TreeContentMStripLarge = new ContextMenuStrip(components);
            addToolStripMenuItem = new ToolStripMenuItem();
            fileToolStripMenuItem2 = new ToolStripMenuItem();
            folderToolStripMenuItem1 = new ToolStripMenuItem();
            newFolderToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            renameToolStripMenuItem = new ToolStripMenuItem();
            removeToolStripMenuItem = new ToolStripMenuItem();
            TreeContentMStripMedium = new ContextMenuStrip(components);
            renameToolStripMenuItem1 = new ToolStripMenuItem();
            removeToolStripMenuItem1 = new ToolStripMenuItem();
            AddFileDialog = new OpenFileDialog();
            OpenDBFile = new OpenFileDialog();
            TreeContentMStripSmall.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            MainWindowMStrip.SuspendLayout();
            TreeContentMStripLarge.SuspendLayout();
            TreeContentMStripMedium.SuspendLayout();
            SuspendLayout();
            // 
            // TreeContentMStripSmall
            // 
            TreeContentMStripSmall.Items.AddRange(new ToolStripItem[] { addFileToolStripMenuItem, newFolderToolStripMenuItem });
            TreeContentMStripSmall.Name = "TreeContentMStripSmall";
            TreeContentMStripSmall.Size = new Size(133, 48);
            // 
            // addFileToolStripMenuItem
            // 
            addFileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fileToolStripMenuItem1, folderToolStripMenuItem });
            addFileToolStripMenuItem.Name = "addFileToolStripMenuItem";
            addFileToolStripMenuItem.Size = new Size(132, 22);
            addFileToolStripMenuItem.Text = "Add";
            // 
            // fileToolStripMenuItem1
            // 
            fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            fileToolStripMenuItem1.Size = new Size(107, 22);
            fileToolStripMenuItem1.Text = "File";
            fileToolStripMenuItem1.Click += ContextMenuAddFileGlobal;
            // 
            // folderToolStripMenuItem
            // 
            folderToolStripMenuItem.Name = "folderToolStripMenuItem";
            folderToolStripMenuItem.Size = new Size(107, 22);
            folderToolStripMenuItem.Text = "Folder";
            // 
            // newFolderToolStripMenuItem
            // 
            newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
            newFolderToolStripMenuItem.Size = new Size(132, 22);
            newFolderToolStripMenuItem.Text = "New folder";
            newFolderToolStripMenuItem.Click += ContextMenuAddNewFolder;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(FileTree);
            splitContainer1.Panel1.Controls.Add(MainWindowMStrip);
            splitContainer1.Size = new Size(1260, 732);
            splitContainer1.SplitterDistance = 420;
            splitContainer1.TabIndex = 1;
            // 
            // FileTree
            // 
            FileTree.Dock = DockStyle.Fill;
            FileTree.ImageIndex = 0;
            FileTree.ImageList = TreeImageList;
            FileTree.Indent = 27;
            FileTree.LabelEdit = true;
            FileTree.Location = new Point(0, 24);
            FileTree.Name = "FileTree";
            treeNode1.ContextMenuStrip = TreeContentMStripSmall;
            treeNode1.ImageIndex = 14;
            treeNode1.Name = "Content";
            treeNode1.SelectedImageIndex = 1;
            treeNode1.Text = "Content";
            treeNode2.ContextMenuStrip = TreeContentMStripSmall;
            treeNode2.ImageIndex = 15;
            treeNode2.Name = "External";
            treeNode2.SelectedImageIndex = 1;
            treeNode2.Text = "External";
            FileTree.Nodes.AddRange(new TreeNode[] { treeNode1, treeNode2 });
            FileTree.SelectedImageIndex = 0;
            FileTree.Size = new Size(420, 708);
            FileTree.TabIndex = 1;
            FileTree.AfterLabelEdit += ContextMenuRenameTree;
            FileTree.NodeMouseHover += FileTreeNodeHover;
            // 
            // TreeImageList
            // 
            TreeImageList.ColorDepth = ColorDepth.Depth32Bit;
            TreeImageList.ImageStream = (ImageListStreamer)resources.GetObject("TreeImageList.ImageStream");
            TreeImageList.TransparentColor = Color.Transparent;
            TreeImageList.Images.SetKeyName(0, "2.ico");
            TreeImageList.Images.SetKeyName(1, "10.ico");
            TreeImageList.Images.SetKeyName(2, "26.ico");
            TreeImageList.Images.SetKeyName(3, "96.ico");
            TreeImageList.Images.SetKeyName(4, "104.ico");
            TreeImageList.Images.SetKeyName(5, "112.ico");
            TreeImageList.Images.SetKeyName(6, "606.ico");
            TreeImageList.Images.SetKeyName(7, "630.ico");
            TreeImageList.Images.SetKeyName(8, "1115.ico");
            TreeImageList.Images.SetKeyName(9, "1173.ico");
            TreeImageList.Images.SetKeyName(10, "1181.ico");
            TreeImageList.Images.SetKeyName(11, "1189.ico");
            TreeImageList.Images.SetKeyName(12, "1392.ico");
            TreeImageList.Images.SetKeyName(13, "1424.ico");
            TreeImageList.Images.SetKeyName(14, "1607.ico");
            TreeImageList.Images.SetKeyName(15, "1615.ico");
            // 
            // MainWindowMStrip
            // 
            MainWindowMStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, buildToolStripMenuItem });
            MainWindowMStrip.Location = new Point(0, 0);
            MainWindowMStrip.Name = "MainWindowMStrip";
            MainWindowMStrip.Size = new Size(420, 24);
            MainWindowMStrip.TabIndex = 0;
            MainWindowMStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // buildToolStripMenuItem
            // 
            buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            buildToolStripMenuItem.Size = new Size(46, 20);
            buildToolStripMenuItem.Text = "Build";
            buildToolStripMenuItem.Click += BuildContent;
            // 
            // TreeContentMStripLarge
            // 
            TreeContentMStripLarge.Items.AddRange(new ToolStripItem[] { addToolStripMenuItem, newFolderToolStripMenuItem1, toolStripMenuItem1, renameToolStripMenuItem, removeToolStripMenuItem });
            TreeContentMStripLarge.Name = "TreeContentMStripLarge";
            TreeContentMStripLarge.Size = new Size(133, 98);
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fileToolStripMenuItem2, folderToolStripMenuItem1 });
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new Size(132, 22);
            addToolStripMenuItem.Text = "Add";
            // 
            // fileToolStripMenuItem2
            // 
            fileToolStripMenuItem2.Name = "fileToolStripMenuItem2";
            fileToolStripMenuItem2.Size = new Size(107, 22);
            fileToolStripMenuItem2.Text = "File";
            fileToolStripMenuItem2.Click += ContextMenuAddFileGlobal;
            // 
            // folderToolStripMenuItem1
            // 
            folderToolStripMenuItem1.Name = "folderToolStripMenuItem1";
            folderToolStripMenuItem1.Size = new Size(107, 22);
            folderToolStripMenuItem1.Text = "Folder";
            // 
            // newFolderToolStripMenuItem1
            // 
            newFolderToolStripMenuItem1.Name = "newFolderToolStripMenuItem1";
            newFolderToolStripMenuItem1.Size = new Size(132, 22);
            newFolderToolStripMenuItem1.Text = "New folder";
            newFolderToolStripMenuItem1.Click += ContextMenuAddNewFolder;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(129, 6);
            // 
            // renameToolStripMenuItem
            // 
            renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            renameToolStripMenuItem.Size = new Size(132, 22);
            renameToolStripMenuItem.Text = "Rename";
            renameToolStripMenuItem.Click += ContextMenuRename;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new Size(132, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += ContextMenuRemove;
            // 
            // TreeContentMStripMedium
            // 
            TreeContentMStripMedium.Items.AddRange(new ToolStripItem[] { renameToolStripMenuItem1, removeToolStripMenuItem1 });
            TreeContentMStripMedium.Name = "TreeContentMStripMedium";
            TreeContentMStripMedium.Size = new Size(118, 48);
            // 
            // renameToolStripMenuItem1
            // 
            renameToolStripMenuItem1.Name = "renameToolStripMenuItem1";
            renameToolStripMenuItem1.Size = new Size(117, 22);
            renameToolStripMenuItem1.Text = "Rename";
            renameToolStripMenuItem1.Click += ContextMenuRename;
            // 
            // removeToolStripMenuItem1
            // 
            removeToolStripMenuItem1.Name = "removeToolStripMenuItem1";
            removeToolStripMenuItem1.Size = new Size(117, 22);
            removeToolStripMenuItem1.Text = "Remove";
            removeToolStripMenuItem1.Click += ContextMenuRemove;
            // 
            // AddFileDialog
            // 
            AddFileDialog.Filter = "All files|*.*";
            AddFileDialog.Multiselect = true;
            // 
            // OpenDBFile
            // 
            OpenDBFile.DefaultExt = "cbdb";
            OpenDBFile.Filter = "Database files|*.cbdb";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 732);
            Controls.Add(splitContainer1);
            MainMenuStrip = MainWindowMStrip;
            Name = "Main";
            Text = "Content builder";
            TreeContentMStripSmall.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            MainWindowMStrip.ResumeLayout(false);
            MainWindowMStrip.PerformLayout();
            TreeContentMStripLarge.ResumeLayout(false);
            TreeContentMStripMedium.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer splitContainer1;
        private MenuStrip MainWindowMStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private TreeView FileTree;
        private ImageList TreeImageList;
        private ContextMenuStrip TreeContentMStripSmall;
        private ToolStripMenuItem addFileToolStripMenuItem;
        private ContextMenuStrip TreeContentMStripLarge;
        private ToolStripMenuItem fileToolStripMenuItem1;
        private ToolStripMenuItem folderToolStripMenuItem;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem2;
        private ToolStripMenuItem folderToolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripMenuItem removeToolStripMenuItem;
        private ContextMenuStrip TreeContentMStripMedium;
        private ToolStripMenuItem renameToolStripMenuItem1;
        private ToolStripMenuItem removeToolStripMenuItem1;
        private OpenFileDialog AddFileDialog;
        private OpenFileDialog OpenDBFile;
        private ToolStripMenuItem newFolderToolStripMenuItem;
        private ToolStripMenuItem newFolderToolStripMenuItem1;
        private ToolStripMenuItem buildToolStripMenuItem;
    }
}
