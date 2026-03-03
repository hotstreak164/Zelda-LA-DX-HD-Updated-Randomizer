using System.Drawing;
using System.Windows.Forms;

namespace LADXHD_Patcher
{
    internal class Forms
    {
        public static Form_MainForm  MainDialog;
        public static Form_OkayForm  OkayDialog; 
        public static Form_YesNoForm YesNoDialog; 

        public static void Initialize()
        {
            MainDialog  = new Form_MainForm();
            OkayDialog  = new Form_OkayForm();
            YesNoDialog = new Form_YesNoForm();
        }

        public static void CreatePatcherText()
        {
            // Set the title including the version number.
            MainDialog.Text = "Link's Awakening DX HD Patcher v" + Config.Version;

            // Transparent overlay label
            MainDialog.TextBox_NoClick = new TransparentLabel
            {
                Text      = "",
                Size      = new Size(326, 114),
                Location  = new Point(10, 14),
                TabIndex  = 16
            };
            MainDialog.groupBox_Main.Controls.Add(MainDialog.TextBox_NoClick);

            // The Advanced RichTextBox allows for justified text.
            MainDialog.TextBox_Info = new AdvRichTextBox
            {
                Size        = new Size(326, 114),
                Location    = new Point(10, 14),
                TabStop     = false,
                BorderStyle = BorderStyle.None,
                ReadOnly    = true,
                BackColor   = ColorTranslator.FromHtml("#F0F0F0")
            };

            // Build text
            string header = "Legend of Zelda: Link's Awakening DX HD v" + Config.Version;
            string body   =
                "\n\nPatches v1.0.0 (or v1.1.4+) to v" + Config.Version + " with the \"Patch\" button " +
                "below. All patchers created since v1.1.4 back up the original " +
                "files so that all future patches no longer require v1.0.0. When updating " +
                "with this version of the patcher, future versions of the " +
                "patcher can use the stored backup files. Backups are stored in the " +
                "\"Data\\Backup\" folder. Do not move or delete them! ";

            MainDialog.TextBox_Info.Text = header + body;

            // ----- Bold the header -----
            MainDialog.TextBox_Info.Select(0, header.Length);
            MainDialog.TextBox_Info.SelectionFont = new Font(
                MainDialog.TextBox_Info.Font, FontStyle.Bold);

            // Reset selection so the rest of the text is not bold
            MainDialog.TextBox_Info.Select(MainDialog.TextBox_Info.TextLength, 0);
            MainDialog.TextBox_Info.SelectionFont = new Font(
                MainDialog.TextBox_Info.Font, FontStyle.Regular);

            // Apply justification after text is added
            MainDialog.TextBox_Info.SelectionAlignment = TextAlign.Justify;

            // Add to form
            MainDialog.groupBox_Main.Controls.Add(MainDialog.TextBox_Info);
        }
    }
}