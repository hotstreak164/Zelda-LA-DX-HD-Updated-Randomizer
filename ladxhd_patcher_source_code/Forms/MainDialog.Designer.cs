namespace LADXHD_Patcher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_MainForm));
            this.button_Patch = new System.Windows.Forms.Button();
            this.button_Exit = new System.Windows.Forms.Button();
            this.button_ChangeLog = new System.Windows.Forms.Button();
            this.groupBox_Main = new System.Windows.Forms.GroupBox();
            this.picturebox_Main = new System.Windows.Forms.PictureBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox_Graphics = new System.Windows.Forms.GroupBox();
            this.combBox_API = new System.Windows.Forms.ComboBox();
            this.label_API = new System.Windows.Forms.Label();
            this.comboBox_Platform = new System.Windows.Forms.ComboBox();
            this.label_Platform = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_Main)).BeginInit();
            this.groupBox_Graphics.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Patch
            // 
            this.button_Patch.Location = new System.Drawing.Point(9, 456);
            this.button_Patch.Name = "button_Patch";
            this.button_Patch.Size = new System.Drawing.Size(106, 30);
            this.button_Patch.TabIndex = 0;
            this.button_Patch.Text = "Patch";
            this.button_Patch.UseVisualStyleBackColor = true;
            this.button_Patch.Click += new System.EventHandler(this.button_Patch_Click);
            // 
            // button_Exit
            // 
            this.button_Exit.Location = new System.Drawing.Point(251, 456);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(106, 30);
            this.button_Exit.TabIndex = 1;
            this.button_Exit.Text = "Exit";
            this.button_Exit.UseVisualStyleBackColor = true;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // button_ChangeLog
            // 
            this.button_ChangeLog.Location = new System.Drawing.Point(130, 456);
            this.button_ChangeLog.Name = "button_ChangeLog";
            this.button_ChangeLog.Size = new System.Drawing.Size(106, 30);
            this.button_ChangeLog.TabIndex = 3;
            this.button_ChangeLog.Text = "Changelog";
            this.button_ChangeLog.UseVisualStyleBackColor = true;
            this.button_ChangeLog.Click += new System.EventHandler(this.button_ChangeLog_Click);
            // 
            // groupBox_Main
            // 
            this.groupBox_Main.Location = new System.Drawing.Point(9, 244);
            this.groupBox_Main.Name = "groupBox_Main";
            this.groupBox_Main.Size = new System.Drawing.Size(347, 130);
            this.groupBox_Main.TabIndex = 5;
            this.groupBox_Main.TabStop = false;
            // 
            // picturebox_Main
            // 
            this.picturebox_Main.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picturebox_Main.ErrorImage = global::LADXHD_Patcher.Resources.la;
            this.picturebox_Main.Image = global::LADXHD_Patcher.Resources.la;
            this.picturebox_Main.InitialImage = global::LADXHD_Patcher.Resources.la;
            this.picturebox_Main.Location = new System.Drawing.Point(9, 0);
            this.picturebox_Main.Name = "picturebox_Main";
            this.picturebox_Main.Size = new System.Drawing.Size(347, 241);
            this.picturebox_Main.TabIndex = 4;
            this.picturebox_Main.TabStop = false;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(9, 432);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(347, 16);
            this.progressBar.TabIndex = 6;
            // 
            // groupBox_Graphics
            // 
            this.groupBox_Graphics.Controls.Add(this.combBox_API);
            this.groupBox_Graphics.Controls.Add(this.label_API);
            this.groupBox_Graphics.Controls.Add(this.comboBox_Platform);
            this.groupBox_Graphics.Controls.Add(this.label_Platform);
            this.groupBox_Graphics.Location = new System.Drawing.Point(9, 376);
            this.groupBox_Graphics.Name = "groupBox_Graphics";
            this.groupBox_Graphics.Size = new System.Drawing.Size(347, 48);
            this.groupBox_Graphics.TabIndex = 7;
            this.groupBox_Graphics.TabStop = false;
            // 
            // combBox_API
            // 
            this.combBox_API.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combBox_API.FormattingEnabled = true;
            this.combBox_API.Items.AddRange(new object[] {
            "DirectX",
            "OpenGL"});
            this.combBox_API.Location = new System.Drawing.Point(223, 16);
            this.combBox_API.Name = "combBox_API";
            this.combBox_API.Size = new System.Drawing.Size(110, 21);
            this.combBox_API.TabIndex = 13;
            this.combBox_API.SelectedIndexChanged += new System.EventHandler(this.combBox_API_SelectedIndexChanged);
            // 
            // label_API
            // 
            this.label_API.AutoSize = true;
            this.label_API.Location = new System.Drawing.Point(178, 19);
            this.label_API.Name = "label_API";
            this.label_API.Size = new System.Drawing.Size(41, 13);
            this.label_API.TabIndex = 12;
            this.label_API.Text = "Target:";
            // 
            // comboBox_Platform
            // 
            this.comboBox_Platform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Platform.FormattingEnabled = true;
            this.comboBox_Platform.Items.AddRange(new object[] {
            "Windows",
            "Android",
            "Linux (x86-64)",
            "Linux (Arm64)",
            "MacOS (x86-64)",
            "MacOS (Arm64)"});
            this.comboBox_Platform.Location = new System.Drawing.Point(56, 16);
            this.comboBox_Platform.Name = "comboBox_Platform";
            this.comboBox_Platform.Size = new System.Drawing.Size(110, 21);
            this.comboBox_Platform.TabIndex = 11;
            this.comboBox_Platform.SelectedIndexChanged += new System.EventHandler(this.comboBox_Platform_SelectedIndexChanged);
            // 
            // label_Platform
            // 
            this.label_Platform.AutoSize = true;
            this.label_Platform.Location = new System.Drawing.Point(6, 19);
            this.label_Platform.Name = "label_Platform";
            this.label_Platform.Size = new System.Drawing.Size(48, 13);
            this.label_Platform.TabIndex = 10;
            this.label_Platform.Text = "Platform:";
            // 
            // Form_MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 494);
            this.Controls.Add(this.groupBox_Graphics);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.groupBox_Main);
            this.Controls.Add(this.picturebox_Main);
            this.Controls.Add(this.button_ChangeLog);
            this.Controls.Add(this.button_Exit);
            this.Controls.Add(this.button_Patch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Link\'s Awakening DX HD Patcher vX.X.X";
            this.Load += new System.EventHandler(this.Form_MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_Main)).EndInit();
            this.groupBox_Graphics.ResumeLayout(false);
            this.groupBox_Graphics.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_Patch;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.Button button_ChangeLog;
        private System.Windows.Forms.PictureBox picturebox_Main;
        public System.Windows.Forms.GroupBox groupBox_Main;
        public System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.GroupBox groupBox_Graphics;
        private System.Windows.Forms.ComboBox combBox_API;
        private System.Windows.Forms.Label label_API;
        private System.Windows.Forms.ComboBox comboBox_Platform;
        private System.Windows.Forms.Label label_Platform;
    }
}

