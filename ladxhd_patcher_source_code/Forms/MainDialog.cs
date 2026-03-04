using System;
using System.Diagnostics;
using System.Windows.Forms;
using static LADXHD_Patcher.Config;

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
            comboBox_Platform.SelectedIndex = 0;
            combBox_API.SelectedIndex = 0;
        }

        public void ToggleDialog(bool toggle)
        {
            button_Patch.Enabled = toggle;
            button_ChangeLog.Enabled = toggle;
            button_Exit.Enabled = toggle;
            label_Platform.Enabled = toggle;
            comboBox_Platform.Enabled = toggle;
            label_API.Enabled = toggle;
            combBox_API.Enabled = toggle;
        }

        public void UpdateProgressBar(int value)
        {
            progressBar.Value = value;
        }

        private void comboBox_Platform_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            if (comboBox.SelectedIndex == 0)
                Config.SelectedPlatform = Platform.Windows;
        }

        private void combBox_API_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            if (comboBox.SelectedIndex == 0)
                Config.SelectedGraphics = GraphicsAPI.DirectX;
            else if (comboBox.SelectedIndex == 1)
                Config.SelectedGraphics = GraphicsAPI.OpenGL;
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
