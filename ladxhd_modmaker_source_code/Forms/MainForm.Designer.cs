namespace LADXHD_ModMaker
{
    partial class Form_MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_MainForm));
            this.picturebox_Main = new System.Windows.Forms.PictureBox();
            this.groupBox_Main = new System.Windows.Forms.GroupBox();
            this.button_Image = new System.Windows.Forms.Button();
            this.label_Image = new System.Windows.Forms.Label();
            this.textBox_Image = new System.Windows.Forms.TextBox();
            this.button_OutputPath = new System.Windows.Forms.Button();
            this.label_OutputPath = new System.Windows.Forms.Label();
            this.textBox_OutputPath = new System.Windows.Forms.TextBox();
            this.button_GamePath = new System.Windows.Forms.Button();
            this.label_GamePath = new System.Windows.Forms.Label();
            this.textBox_GamePath = new System.Windows.Forms.TextBox();
            this.label_Description = new System.Windows.Forms.Label();
            this.panel_Description = new System.Windows.Forms.Panel();
            this.richTextBox_Description = new System.Windows.Forms.RichTextBox();
            this.label_ModName = new System.Windows.Forms.Label();
            this.textBox_ModName = new System.Windows.Forms.TextBox();
            this.button_CreateMod = new System.Windows.Forms.Button();
            this.button_Close = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.mainTooltip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_Main)).BeginInit();
            this.groupBox_Main.SuspendLayout();
            this.panel_Description.SuspendLayout();
            this.SuspendLayout();
            // 
            // picturebox_Main
            // 
            this.picturebox_Main.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picturebox_Main.ErrorImage = global::LADXHD_ModMaker.Properties.Resources.la;
            this.picturebox_Main.Image = global::LADXHD_ModMaker.Properties.Resources.la;
            this.picturebox_Main.InitialImage = global::LADXHD_ModMaker.Properties.Resources.la;
            this.picturebox_Main.Location = new System.Drawing.Point(9, 0);
            this.picturebox_Main.Name = "picturebox_Main";
            this.picturebox_Main.Size = new System.Drawing.Size(347, 241);
            this.picturebox_Main.TabIndex = 0;
            this.picturebox_Main.TabStop = false;
            // 
            // groupBox_Main
            // 
            this.groupBox_Main.Controls.Add(this.button_Image);
            this.groupBox_Main.Controls.Add(this.label_Image);
            this.groupBox_Main.Controls.Add(this.textBox_Image);
            this.groupBox_Main.Controls.Add(this.button_OutputPath);
            this.groupBox_Main.Controls.Add(this.label_OutputPath);
            this.groupBox_Main.Controls.Add(this.textBox_OutputPath);
            this.groupBox_Main.Controls.Add(this.button_GamePath);
            this.groupBox_Main.Controls.Add(this.label_GamePath);
            this.groupBox_Main.Controls.Add(this.textBox_GamePath);
            this.groupBox_Main.Controls.Add(this.label_Description);
            this.groupBox_Main.Controls.Add(this.panel_Description);
            this.groupBox_Main.Controls.Add(this.label_ModName);
            this.groupBox_Main.Controls.Add(this.textBox_ModName);
            this.groupBox_Main.Location = new System.Drawing.Point(9, 247);
            this.groupBox_Main.Name = "groupBox_Main";
            this.groupBox_Main.Size = new System.Drawing.Size(347, 326);
            this.groupBox_Main.TabIndex = 0;
            this.groupBox_Main.TabStop = false;
            // 
            // button_Image
            // 
            this.button_Image.Location = new System.Drawing.Point(313, 196);
            this.button_Image.Name = "button_Image";
            this.button_Image.Size = new System.Drawing.Size(28, 24);
            this.button_Image.TabIndex = 6;
            this.button_Image.Text = "...";
            this.button_Image.UseVisualStyleBackColor = true;
            this.button_Image.Click += new System.EventHandler(this.button_Image_Click);
            // 
            // label_Image
            // 
            this.label_Image.AutoSize = true;
            this.label_Image.Location = new System.Drawing.Point(5, 179);
            this.label_Image.Name = "label_Image";
            this.label_Image.Size = new System.Drawing.Size(103, 13);
            this.label_Image.TabIndex = 0;
            this.label_Image.Text = "Image File (Optional)";
            this.mainTooltip.SetToolTip(this.label_Image, "Replaces default image. Must be formatted \r\nas PNG, BMP, or JPG. For the best fit" +
        ", resize\r\nyour image with the dimensions 350x248.");
            // 
            // textBox_Image
            // 
            this.textBox_Image.AllowDrop = true;
            this.textBox_Image.Location = new System.Drawing.Point(5, 198);
            this.textBox_Image.Name = "textBox_Image";
            this.textBox_Image.Size = new System.Drawing.Size(304, 20);
            this.textBox_Image.TabIndex = 5;
            this.textBox_Image.TextChanged += new System.EventHandler(this.textBox_Image_TextChanged);
            this.textBox_Image.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox_Image_DragDrop);
            this.textBox_Image.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox_Image_DragEnter);
            this.textBox_Image.Leave += new System.EventHandler(this.textBox_Image_Leave);
            // 
            // button_OutputPath
            // 
            this.button_OutputPath.Location = new System.Drawing.Point(314, 292);
            this.button_OutputPath.Name = "button_OutputPath";
            this.button_OutputPath.Size = new System.Drawing.Size(28, 24);
            this.button_OutputPath.TabIndex = 10;
            this.button_OutputPath.Text = "...";
            this.button_OutputPath.UseVisualStyleBackColor = true;
            this.button_OutputPath.Click += new System.EventHandler(this.button_OutputPath_Click);
            // 
            // label_OutputPath
            // 
            this.label_OutputPath.AutoSize = true;
            this.label_OutputPath.Location = new System.Drawing.Point(6, 275);
            this.label_OutputPath.Name = "label_OutputPath";
            this.label_OutputPath.Size = new System.Drawing.Size(67, 13);
            this.label_OutputPath.TabIndex = 0;
            this.label_OutputPath.Text = "Output Path:";
            this.mainTooltip.SetToolTip(this.label_OutputPath, "The path to output the finalized mod\r\nwhich will appear in \"~ModOutput\".");
            // 
            // textBox_OutputPath
            // 
            this.textBox_OutputPath.AllowDrop = true;
            this.textBox_OutputPath.Location = new System.Drawing.Point(6, 294);
            this.textBox_OutputPath.Name = "textBox_OutputPath";
            this.textBox_OutputPath.Size = new System.Drawing.Size(304, 20);
            this.textBox_OutputPath.TabIndex = 9;
            this.textBox_OutputPath.TextChanged += new System.EventHandler(this.textBox_OutputPath_TextChanged);
            this.textBox_OutputPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox_OutputPath_DragDrop);
            this.textBox_OutputPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox_OutputPath_DragEnter);
            this.textBox_OutputPath.Leave += new System.EventHandler(this.textBox_OutputPath_Leave);
            // 
            // button_GamePath
            // 
            this.button_GamePath.Location = new System.Drawing.Point(314, 244);
            this.button_GamePath.Name = "button_GamePath";
            this.button_GamePath.Size = new System.Drawing.Size(28, 24);
            this.button_GamePath.TabIndex = 8;
            this.button_GamePath.Text = "...";
            this.button_GamePath.UseVisualStyleBackColor = true;
            this.button_GamePath.Click += new System.EventHandler(this.button_GamePath_Click);
            // 
            // label_GamePath
            // 
            this.label_GamePath.AutoSize = true;
            this.label_GamePath.Location = new System.Drawing.Point(6, 227);
            this.label_GamePath.Name = "label_GamePath";
            this.label_GamePath.Size = new System.Drawing.Size(110, 13);
            this.label_GamePath.TabIndex = 0;
            this.label_GamePath.Text = "LADXHD Game Path:";
            this.mainTooltip.SetToolTip(this.label_GamePath, "The path to the root game folder that contains mod \r\nfiles. The mod maker will fi" +
        "nd mod files automatically.");
            // 
            // textBox_GamePath
            // 
            this.textBox_GamePath.AllowDrop = true;
            this.textBox_GamePath.Location = new System.Drawing.Point(6, 246);
            this.textBox_GamePath.Name = "textBox_GamePath";
            this.textBox_GamePath.Size = new System.Drawing.Size(304, 20);
            this.textBox_GamePath.TabIndex = 7;
            this.textBox_GamePath.TextChanged += new System.EventHandler(this.textBox_GamePath_TextChanged);
            this.textBox_GamePath.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox_GamePath_DragDrop);
            this.textBox_GamePath.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox_GamePath_DragEnter);
            this.textBox_GamePath.Leave += new System.EventHandler(this.textBox_GamePath_Leave);
            // 
            // label_Description
            // 
            this.label_Description.AutoSize = true;
            this.label_Description.Location = new System.Drawing.Point(6, 63);
            this.label_Description.Name = "label_Description";
            this.label_Description.Size = new System.Drawing.Size(87, 13);
            this.label_Description.TabIndex = 0;
            this.label_Description.Text = "Mod Description:";
            this.mainTooltip.SetToolTip(this.label_Description, "A brief description of the mod.");
            // 
            // panel_Description
            // 
            this.panel_Description.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel_Description.Controls.Add(this.richTextBox_Description);
            this.panel_Description.Location = new System.Drawing.Point(6, 83);
            this.panel_Description.Name = "panel_Description";
            this.panel_Description.Size = new System.Drawing.Size(334, 86);
            this.panel_Description.TabIndex = 3;
            // 
            // richTextBox_Description
            // 
            this.richTextBox_Description.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox_Description.Location = new System.Drawing.Point(1, 1);
            this.richTextBox_Description.Name = "richTextBox_Description";
            this.richTextBox_Description.Size = new System.Drawing.Size(332, 84);
            this.richTextBox_Description.TabIndex = 2;
            this.richTextBox_Description.Text = "";
            this.richTextBox_Description.TextChanged += new System.EventHandler(this.richTextBox_Description_TextChanged);
            // 
            // label_ModName
            // 
            this.label_ModName.AutoSize = true;
            this.label_ModName.Location = new System.Drawing.Point(6, 15);
            this.label_ModName.Name = "label_ModName";
            this.label_ModName.Size = new System.Drawing.Size(62, 13);
            this.label_ModName.TabIndex = 0;
            this.label_ModName.Text = "Mod Name:";
            this.mainTooltip.SetToolTip(this.label_ModName, "The name of the mod and \r\noptionally the mod version.");
            // 
            // textBox_ModName
            // 
            this.textBox_ModName.Location = new System.Drawing.Point(6, 34);
            this.textBox_ModName.Name = "textBox_ModName";
            this.textBox_ModName.Size = new System.Drawing.Size(334, 20);
            this.textBox_ModName.TabIndex = 1;
            this.textBox_ModName.TextChanged += new System.EventHandler(this.textBox_ModName_TextChanged);
            // 
            // button_CreateMod
            // 
            this.button_CreateMod.Location = new System.Drawing.Point(8, 603);
            this.button_CreateMod.Name = "button_CreateMod";
            this.button_CreateMod.Size = new System.Drawing.Size(140, 30);
            this.button_CreateMod.TabIndex = 11;
            this.button_CreateMod.Text = "Create Mod";
            this.button_CreateMod.UseVisualStyleBackColor = true;
            this.button_CreateMod.Click += new System.EventHandler(this.button_CreateMod_Click);
            // 
            // button_Close
            // 
            this.button_Close.Location = new System.Drawing.Point(218, 603);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(140, 30);
            this.button_Close.TabIndex = 12;
            this.button_Close.Text = "Exit";
            this.button_Close.UseVisualStyleBackColor = true;
            this.button_Close.Click += new System.EventHandler(this.button_Close_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(9, 580);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(347, 16);
            this.progressBar.TabIndex = 0;
            // 
            // Form_MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 639);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.button_Close);
            this.Controls.Add(this.button_CreateMod);
            this.Controls.Add(this.groupBox_Main);
            this.Controls.Add(this.picturebox_Main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LADX Mod Creator";
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_Main)).EndInit();
            this.groupBox_Main.ResumeLayout(false);
            this.groupBox_Main.PerformLayout();
            this.panel_Description.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picturebox_Main;
        public System.Windows.Forms.GroupBox groupBox_Main;
        private System.Windows.Forms.Label label_ModName;
        private System.Windows.Forms.TextBox textBox_ModName;
        private System.Windows.Forms.Panel panel_Description;
        private System.Windows.Forms.RichTextBox richTextBox_Description;
        private System.Windows.Forms.Label label_Description;
        private System.Windows.Forms.Label label_GamePath;
        private System.Windows.Forms.TextBox textBox_GamePath;
        private System.Windows.Forms.Button button_CreateMod;
        private System.Windows.Forms.Button button_GamePath;
        private System.Windows.Forms.Button button_Close;
        private System.Windows.Forms.Button button_OutputPath;
        private System.Windows.Forms.Label label_OutputPath;
        private System.Windows.Forms.TextBox textBox_OutputPath;
        public System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button button_Image;
        private System.Windows.Forms.Label label_Image;
        private System.Windows.Forms.TextBox textBox_Image;
        private System.Windows.Forms.ToolTip mainTooltip;
    }
}

