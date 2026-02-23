using System;
using System.Windows.Forms;

namespace LADXHD_Migrater
{
    public partial class Form_MainForm : Form
    {
        public Form_MainForm()
        {
            InitializeComponent();
        }
        public void ToggleDialog(bool toggle)
        {
            button1.Enabled = toggle;
            button2.Enabled = toggle;
            button3.Enabled = toggle;
            button4.Enabled = toggle;
            button5.Enabled = toggle;
        }

        public static bool VerifyMigrate()
        {
            if (!Config.Orig_Content.TestPath() || !Config.Orig_Data.TestPath())
            {
                Forms.okayDialog.Display("Error: Assets Missing", 250, 40, 26, 16, 15,
                    "Either the original \"Content\" folder, \"Data\" folder, or both are missing from the \"assets_original\" folder.");
                return false;
            }
            bool verify = Forms.yesNoDialog.Display("Confirm Migration", 250, 40, 31, 16, true, 
                "Are you sure you wish to migrate assets? This will apply current patches and overwrite your assets!");
            return verify;
        }

        private static bool VerifyCreatePatch()
        {
            // If the original "Content" or "Data" folder is missing.
            if (!Config.Orig_Content.TestPath() || !Config.Orig_Data.TestPath())
            {
                Forms.okayDialog.Display("Error: Assets Missing", 250, 40, 26, 16, 15,
                    "Either the original \"Content\" folder, \"Data\" folder, or both are missing from the \"assets_original\" folder.");
                return false;
            }
            // If the updated "Content" or "Data" folder is missing.
            if (!Config.Update_Content.TestPath() || !Config.Update_Data.TestPath())
            {
                Forms.okayDialog.Display("Assets Missing", 250, 40, 34, 16, 15,
                    "Either the \"Content\" folder, \"Data\" folder, or both are missing from \"ladxhd_game_source_code\".");
                return false;
            }
            // Ask the user if they want to create patches.
            bool verify = Forms.yesNoDialog.Display("Confirm Create Patches", 250, 40, 31, 16, true, 
                "Are you sure you wish to create patches? This will overwrite all current patches with recent changes!");
            return verify;
        }

        private static bool VerifyCleanFiles()
        {
            bool verify = Forms.yesNoDialog.Display("Clean Build Files", 250, 40, 29, 9, true, 
                "Are you sure you wish to clean build files? This will remove all instances of \'obj\', \'bin\', \'Publish\', and \'zelda_ladxhd_build\' folders if they currently exist.");
            return verify;
        }

        private void button_Migrate_Click(object sender, EventArgs e)
        {
            // Check to see if files should be migrated.
            if (!VerifyMigrate()) return;

            // Disable the dialog's controls.
            Forms.mainDialog.ToggleDialog(false);

            // Migrate to the latest versions of files.
            Functions.MigrateFiles();

            // Let the user know that files were migrated.
            Forms.okayDialog.Display("Finished Migration", 280, 40, 45, 26, 15, 
                "Updated Content/Data files to latest versions.");

            // Enable the dialog's controls.
            Forms.mainDialog.ToggleDialog(true);
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
            Forms.okayDialog.Display("Patches Created", 250, 40, 27, 9, 15,
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
            Forms.okayDialog.Display("Finished", 260, 40, 26, 26, 15,
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
            Forms.okayDialog.Display("Finished", 250, 40, 28, 16, 15,
                "Finished build process. If the build was successful, it can be found in the \"zelda_ladxhd_build\" folder.");

            // Enable the dialog's controls.
            ToggleDialog(true);
        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
