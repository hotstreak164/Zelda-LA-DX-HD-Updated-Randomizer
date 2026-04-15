using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace LADXHD_ModMaker
{
    public partial class Form_ModForm : Form
    {
        public string _gamePath = "";

        public Form_ModForm()
        {
            InitializeComponent();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        UTILITY

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void EnableComponents(bool toggle)
        {
            textBox_GamePath.Enabled = toggle;
            button_GamePath.Enabled = toggle;
            button_Install.Enabled = toggle;
            button_Close.Enabled = toggle;
        }

        private void Form_ModForm_Load(object sender, EventArgs e)
        {
            string[] validExt = { ".png", ".jpg", ".bmp" };
            string imagePath = "";

            foreach (string ext in validExt) 
            {
                string testImage = Path.Combine(Config.BaseFolder, "image" + ext);

                if (File.Exists(testImage))
                    imagePath = testImage;
            }
            if (imagePath.TestPath())
            {
                Image uiImage = Image.FromFile(imagePath);
                picturebox_Mod.Image = uiImage;
                picturebox_Mod.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        public void SetInformation()
        {
            this.label_ModName.Text = Config.ModName;
            this.label_Description.Text = Config.Description.Replace("\\n", "\n");
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        GAME PATH

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void textBox_GamePath_TextChanged(object sender, EventArgs e)
        {
            _gamePath = this.textBox_GamePath.Text;
        }
        private void textBox_GamePath_Leave(object sender, EventArgs e)
        {
            if (_gamePath != "" && _gamePath.TestPath())
                Config.UpdateGamePaths(_gamePath);
            else
                this.textBox_GamePath.Text = _gamePath;
        }
        private void textBox_GamePath_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
        private void textBox_GamePath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] DroppedPath = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (DroppedPath[0] != "" && DroppedPath[0].TestPath())
                {
                    _gamePath = DroppedPath[0];
                    Config.UpdateGamePaths(_gamePath);
                    this.textBox_GamePath.Text = _gamePath;
                }
            }
        }
        private void button_GamePath_Click(object sender, EventArgs e)
        {
            string selectedPath = Forms.CreateFolderSelectDialog(Config.BaseFolder);

            if (selectedPath != "" && selectedPath.TestPath())
            {
                _gamePath = selectedPath;
                Config.UpdateGamePaths(_gamePath);
                this.textBox_GamePath.Text = _gamePath;
            }
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        PROGRESS BAR

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        public void UpdateProgressBar(int value)
        {
            progressBar.Value = value;
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        BUTTONS

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void button_Install_Click(object sender, EventArgs e)
        {
            string message;

            // The game path is empty.
            if (string.IsNullOrEmpty(Config.GamePath))
            {
                message = "The LADXHD Game Path can not be empty. It should point to the base folder of where the game is installed.";
                Forms.OkayDialog.Display("Game Path Empty", 260, 40, 24, 16, 10, message);
                return;
            }

            // The game executable was not found.
            string GameExePath = Path.Combine(Config.GamePath, "Link's Awakening DX HD.exe");
            if (!Config.GamePath.TestPath(true) || !GameExePath.TestPath())
            {
                message = "The Game Path must point to a valid installation that contains the file \"Link's Awakening DX HD.exe\".";
                Forms.OkayDialog.Display("Game Path Invalid", 260, 40, 24, 16, 10, message);
                return;
            }

            // Disable form, apply patches, enable form.
            EnableComponents(false);
            Functions.ApplyModPatches();
            EnableComponents(true);

            // Display that it's done.
            message = "Finished applying patches. Any \"Graphics\" mods or \"LAHDMods\" were generated in the \"Mods\" folder.";
            Forms.OkayDialog.Display("Mod Installed", 270, 40, 25, 16, 10, message);
            return;
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
