using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTool.CB
{
    internal partial class BuildConfig : Form
    {
        public DialogResult Result = DialogResult.None;
        public FilePostProcess.ProcessQuality Quality = FilePostProcess.ProcessQuality.Medium;

        public BuildConfig()
        {
            InitializeComponent();
            button1.Select();
        }

        private void BuildPressed(object sender, EventArgs e)
        {
            Result = DialogResult.OK;
            Close();
        }

        private void CancelPressed(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }

        private void QualityChanged(object sender, EventArgs e) => Quality = (FilePostProcess.ProcessQuality)comboBox1.SelectedIndex;
    }
}
