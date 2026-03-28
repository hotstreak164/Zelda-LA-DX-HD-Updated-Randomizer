using System;
using System.Windows.Forms;
using static LADXHD_Migrater.Config;

namespace LADXHD_Migrater
{
    public partial class Form_MainForm : Form
    {
        public Form_MainForm()
        {
            InitializeComponent();
        }

        private void Form_MainForm_Load(object sender, EventArgs e)
        {
            comboBox_Platform.SelectedIndex = 0;
            combBox_API.SelectedIndex = 0;
        }

        public void ToggleDialog(bool toggle)
        {
            button_Migrate.Enabled = toggle;
            button_Patches.Enabled = toggle;
            button_Exit.Enabled = toggle;
            button_Clean.Enabled = toggle;
            button_Build.Enabled = toggle;
            comboBox_Platform.Enabled = toggle;
            combBox_API.Enabled = toggle;
            label_Platform.Enabled = toggle;
            label_API.Enabled = toggle;
        }

        private void comboBox_Platform_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            if (comboBox.SelectedIndex == 0)
            {
                Config.SelectedPlatform = Platform.Windows;

                combBox_API.Items.Clear();
                combBox_API.Items.Add("DirectX");
                combBox_API.Items.Add("OpenGL");
            }
            if (comboBox.SelectedIndex == 1)
            {
                Config.SelectedPlatform = Platform.Android;

                combBox_API.Items.Clear();
                combBox_API.Items.Add("OpenGL");
            }
            if (comboBox.SelectedIndex == 2)
            {
                Config.SelectedPlatform = Platform.Linux_x86;

                combBox_API.Items.Clear();
                combBox_API.Items.Add("OpenGL");
            }
            if (comboBox.SelectedIndex == 3)
            {
                Config.SelectedPlatform = Platform.Linux_Arm64;

                combBox_API.Items.Clear();
                combBox_API.Items.Add("OpenGL");
            }
            if (comboBox.SelectedIndex == 4)
            {
                Config.SelectedPlatform = Platform.MacOS_x64;

                combBox_API.Items.Clear();
                combBox_API.Items.Add("OpenGL");
            }
            if (comboBox.SelectedIndex == 5)
            {
                Config.SelectedPlatform = Platform.MacOS_Arm64;

                combBox_API.Items.Clear();
                combBox_API.Items.Add("OpenGL");
            }
            combBox_API.SelectedIndex = 0;
        }

        private void combBox_API_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            if (comboBox.SelectedIndex == 0)
                Config.SelectedGraphics = GraphicsAPI.DirectX;
            else if (comboBox.SelectedIndex == 1)
                Config.SelectedGraphics = GraphicsAPI.OpenGL;
        }

        public static bool VerifyMigrate()
        {
            if (!Config.Orig_Content.TestPath() || !Config.Orig_Data.TestPath())
            {
                Forms.OkayDialog.Display("Error: Assets Missing", 250, 40, 26, 16, 15,
                    "Either the original \"Content\" folder, \"Data\" folder, or both are missing from the \"assets_original\" folder.");
                return false;
            }
            bool verify = Forms.YesNoDialog.Display("Confirm Migration", 250, 40, 31, 16, true, 
                "Are you sure you wish to migrate assets? This will apply current patches and overwrite your assets!");
            return verify;
        }

        private static bool VerifyCreatePatch()
        {
            // If the original "Content" or "Data" folder is missing.
            if (!Config.Orig_Content.TestPath() || !Config.Orig_Data.TestPath())
            {
                Forms.OkayDialog.Display("Error: Assets Missing", 250, 40, 26, 16, 15,
                    "Either the original \"Content\" folder, \"Data\" folder, or both are missing from the \"assets_original\" folder.");
                return false;
            }
            // If the updated "Content" or "Data" folder is missing.
            if (!Config.Update_Content.TestPath() || !Config.Update_Data.TestPath())
            {
                Forms.OkayDialog.Display("Assets Missing", 250, 40, 34, 16, 15,
                    "Either the \"Content\" folder, \"Data\" folder, or both are missing from \"ladxhd_game_source_code\".");
                return false;
            }
            // Ask the user if they want to create patches.
            bool verify = Forms.YesNoDialog.Display("Confirm Create Patches", 250, 40, 31, 16, true, 
                "Are you sure you wish to create patches? This will overwrite all current patches with recent changes!");
            return verify;
        }

        private static bool VerifyCleanFiles()
        {
            bool verify = Forms.YesNoDialog.Display("Clean Build Files", 250, 40, 29, 9, true, 
                "Are you sure you wish to clean build files? This will remove all instances of \'obj\', \'bin\', \'Publish\', and \'zelda_ladxhd_build\' folders if they currently exist.");
            return verify;
        }

        private void button_Migrate_Click(object sender, EventArgs e)
        {
            // Check to see if files should be migrated.
            if (!VerifyMigrate()) return;

            // Disable the dialog's controls.
            Forms.MainDialog.ToggleDialog(false);

            // Migrate to the latest versions of files.
            Functions.MigrateFiles();

            // Let the user know that files were migrated.
            Forms.OkayDialog.Display("Finished Migration", 280, 40, 45, 26, 15, 
                "Updated Content/Data files to latest versions.");

            // Enable the dialog's controls.
            Forms.MainDialog.ToggleDialog(true);
        }

        private void button_Patches_click(object sender, EventArgs e)
        {
            // Check to see if patches should be created.
            if (!VerifyCreatePatch()) return;

            // Disable the dialog's controls.
            ToggleDialog(false);

            // Create the patches.
            Functions.CreatePatches();

            // Let the user know that the patches were created.
            Forms.OkayDialog.Display("Patches Created", 250, 40, 27, 9, 15,
                "Finished creating xdelta patches from modified files. If any files were intentionally modifed, these can be shared as a new PR for the GitHub repository.");

            // Enable the dialog's controls.
            ToggleDialog(true);
        }

        private void button_Clean_Click(object sender, EventArgs e)
        {
            // Verify cleaning out junk files.
            if (!VerifyCleanFiles()) return;

            // Disable the dialog's controls.
            ToggleDialog(false);

            // Clean out the junk files.
            Functions.CleanBuildFiles();

            // Let the user know that it finished.
            Forms.OkayDialog.Display("Finished", 260, 40, 26, 26, 15,
                "Finished cleaning build files (obj/bin/Publish folders).");

            // Enable the dialog's controls.
            ToggleDialog(true);
        }

        private void button_Build_Click(object sender, EventArgs e)
        {
            // Disable the dialog's controls.
            ToggleDialog(false);
            
            // Try to create a new build.
            Functions.CreateBuild();

            // Let the user know it's finished.
            Forms.OkayDialog.Display("Finished", 250, 40, 28, 16, 15,
                "Finished build process. If the build was successful, it can be found in the \"~Publish\" folder.");

            // Enable the dialog's controls.
            ToggleDialog(true);
        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
