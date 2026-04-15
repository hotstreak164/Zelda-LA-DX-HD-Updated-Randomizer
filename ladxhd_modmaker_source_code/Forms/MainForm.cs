using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LADXHD_ModMaker
{
    public partial class Form_MainForm : Form
    {
        public string _gamePath   = "";
        public string _outputPath = "";
        public string _imagePath  = "";

        public Form_MainForm()
        {
            InitializeComponent();
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        UTILITY

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void EnableComponents(bool toggle)
        {
            this.textBox_ModName.Enabled = toggle;
            this.richTextBox_Description.Enabled = toggle;
            this.textBox_GamePath.Enabled = toggle;
            this.button_GamePath.Enabled = toggle;
            this.textBox_OutputPath.Enabled = toggle;
            this.button_OutputPath.Enabled = toggle;
            this.textBox_Image.Enabled = toggle;
            this.button_Image.Enabled = toggle;
            this.button_CreateMod.Enabled = toggle;
            this.button_Close.Enabled = toggle;
        }

/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        MOD NAME

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void textBox_ModName_TextChanged(object sender, EventArgs e)
        {
            Config.ModName = this.textBox_ModName.Text;
        }


/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        MOD DESCRIPTION

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void richTextBox_Description_TextChanged(object sender, EventArgs e)
        {
            Config.Description = this.richTextBox_Description.Text.Replace("\r\n", "\\n").Replace("\n", "\\n");
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

        OUTPUT PATH

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void textBox_OutputPath_TextChanged(object sender, EventArgs e)
        {
            _outputPath = this.textBox_OutputPath.Text;
        }
        private void textBox_OutputPath_Leave(object sender, EventArgs e)
        {
            if (_outputPath != "" && _outputPath.TestPath())
                Config.UpdateOutputPaths(_outputPath);
            else
                this.textBox_OutputPath.Text = _outputPath;
        }
        private void textBox_OutputPath_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
        private void textBox_OutputPath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] DroppedPath = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (DroppedPath[0] != "" && DroppedPath[0].TestPath())
                {
                    _outputPath = DroppedPath[0];
                    Config.UpdateOutputPaths(_outputPath);
                    this.textBox_OutputPath.Text = _outputPath;
                }
            }
        }
        private void button_OutputPath_Click(object sender, EventArgs e)
        {
            string selectedPath = Forms.CreateFolderSelectDialog(Config.BaseFolder);

            if (selectedPath != "" && selectedPath.TestPath())
            {
                _outputPath = selectedPath;
                Config.UpdateOutputPaths(_outputPath);
                this.textBox_OutputPath.Text = _outputPath;
            }
        }


/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        IMAGE FILE PATH

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void textBox_Image_TextChanged(object sender, EventArgs e)
        {
            _imagePath = this.textBox_Image.Text;
        }
        private void textBox_Image_Leave(object sender, EventArgs e)
        {
            if (_imagePath != "" && _imagePath.TestPath())
                Config.ImagePath = this.textBox_Image.Text;
            else
                this.textBox_Image.Text = Config.ImagePath;
        }
        private void textBox_Image_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
        private void textBox_Image_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] DroppedPath = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (DroppedPath[0] != "" && DroppedPath[0].TestPath())
                {
                    FileItem dropItem = new FileItem(DroppedPath[0]);
                    string[] validExt = {".png", ".jpg", ".bmp"};

                    if (validExt.Contains(dropItem.Extension))
                    {
                        _imagePath = DroppedPath[0];
                        Config.ImagePath = DroppedPath[0];
                        this.textBox_Image.Text = Config.ImagePath;
                    }
                }
            }
        }
        private void button_Image_Click(object sender, EventArgs e)
        {
            string selectedPath = Config.BaseFolder.ShowFileDialog(new string[]{"*.*"}, new string[]{"Image File"});

            if (selectedPath != "" && selectedPath.TestPath())
            {
                FileItem selectItem = new FileItem(selectedPath);
                string[] validExt = {".png", ".jpg", ".bmp"};

                if (validExt.Contains(selectItem.Extension))
                {
                    _imagePath = selectedPath;
                    Config.ImagePath = selectedPath;
                    this.textBox_Image.Text = Config.ImagePath;
                }
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

        private void button_CreateMod_Click(object sender, EventArgs e)
        {
            string message;

            // Mod name is empty.
            if (string.IsNullOrEmpty(Config.ModName))
            {
                message = "Error: You must enter a \"Mod Name\" to continue.";
                Forms.OkayDialog.Display("Game Path Empty", 260, 40, 33, 24, 10, message);
                return;
            }

            // Description is empty.
            if (string.IsNullOrEmpty(Config.Description))
            {
                message = "Error: You must enter a \"Mod Description\" to continue.";
                Forms.OkayDialog.Display("Game Path Empty", 270, 40, 22, 24, 10, message);
                return;
            }

            // Game path is empty.
            if (string.IsNullOrEmpty(Config.GamePath))
            {
                message = "The LADXHD Game Path can not be empty. It should point to the base folder of where the game is installed.";
                Forms.OkayDialog.Display("Game Path Empty", 260, 40, 24, 16, 10, message);
                return;
            }

            // Game executable not found.
            string GameExePath = Path.Combine(Config.GamePath, "Link's Awakening DX HD.exe");
            if (!Config.GamePath.TestPath(true) || !GameExePath.TestPath())
            {
                message = "The Game Path must point to a valid installation that contains the file \"Link's Awakening DX HD.exe\".";
                Forms.OkayDialog.Display("Game Path Invalid", 260, 40, 24, 16, 10, message);
                return;
            }

            // Output path invalid or empty.
            if (string.IsNullOrEmpty(_outputPath) || !_outputPath.TestPath(true))
            {
                message = "Error: Select a valid \"Output Path\" to continue.";
                Forms.OkayDialog.Display("Game Path Invalid", 260, 40, 36, 24, 10, message);
                return;
            }

            // Disable form, create patches, enable components.
            this.EnableComponents(false);
            Functions.CreateModPatches();
            this.EnableComponents(true);

            // Display that it's done.
            message = "Finished generating. Mod can be found in the \"Output Path\" in a folder named \"~ModOutput\".";
            Forms.OkayDialog.Display("Mod Created", 260, 40, 33, 16, 10, message);
            return;
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
