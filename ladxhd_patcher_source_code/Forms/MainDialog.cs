using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LADXHD_Patcher
{
    public partial class Form_MainForm : Form
    {
        public AdvRichTextBox TextBox_Info;
        public TransparentLabel TextBox_NoClick;

        public Form_MainForm()
        {
            InitializeComponent();
        }

        private void Form_MainForm_Load(object sender, EventArgs e)
        {
            Forms.CreatePatcherText();
        }

        public void ToggleDialog(bool toggle)
        {
            button_Patch.Enabled = toggle;
            button_ChangeLog.Enabled = toggle;
            button_Exit.Enabled = toggle;
        }

        public void UpdateProgressBar(int value)
        {
            progressBar.Value = value;
        }

        private void button_Patch_Click(object sender, EventArgs e)
        {
            Functions.StartPatching();
        }

        private void button_ChangeLog_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/blob/main/CHANGELOG.md");
        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

}
