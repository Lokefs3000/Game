namespace ContentBuilder
{
    partial class AddFolderDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowLayoutPanel1 = new FlowLayoutPanel();
            AF_Add = new Button();
            AF_Cancel = new Button();
            treeView1 = new TreeView();
            radioButton1 = new RadioButton();
            flowLayoutPanel2 = new FlowLayoutPanel();
            panel1 = new Panel();
            flowLayoutPanel2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.Location = new Point(0, 380);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(422, 0);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // AF_Add
            // 
            AF_Add.Location = new Point(3, 3);
            AF_Add.Name = "AF_Add";
            AF_Add.Size = new Size(96, 23);
            AF_Add.TabIndex = 0;
            AF_Add.Text = "Add";
            AF_Add.UseVisualStyleBackColor = true;
            // 
            // AF_Cancel
            // 
            AF_Cancel.Location = new Point(105, 3);
            AF_Cancel.Name = "AF_Cancel";
            AF_Cancel.Size = new Size(96, 23);
            AF_Cancel.TabIndex = 1;
            AF_Cancel.Text = "Cancel";
            AF_Cancel.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            treeView1.CheckBoxes = true;
            treeView1.Dock = DockStyle.Fill;
            treeView1.Location = new Point(0, 0);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(422, 380);
            treeView1.TabIndex = 2;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(12, 8);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(53, 19);
            radioButton1.TabIndex = 2;
            radioButton1.TabStop = true;
            radioButton1.Text = "Copy";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Controls.Add(AF_Add);
            flowLayoutPanel2.Controls.Add(AF_Cancel);
            flowLayoutPanel2.Location = new Point(215, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(204, 29);
            flowLayoutPanel2.TabIndex = 3;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.Controls.Add(flowLayoutPanel2);
            panel1.Controls.Add(radioButton1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 345);
            panel1.Name = "panel1";
            panel1.Size = new Size(422, 35);
            panel1.TabIndex = 3;
            // 
            // AddFolderDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(422, 380);
            Controls.Add(panel1);
            Controls.Add(treeView1);
            Controls.Add(flowLayoutPanel1);
            MinimumSize = new Size(438, 419);
            Name = "AddFolderDialog";
            Text = "Add folder";
            flowLayoutPanel2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private FlowLayoutPanel flowLayoutPanel1;
        private Button AF_Add;
        private Button AF_Cancel;
        private TreeView treeView1;
        private RadioButton radioButton1;
        private FlowLayoutPanel flowLayoutPanel2;
        private Panel panel1;
    }
}