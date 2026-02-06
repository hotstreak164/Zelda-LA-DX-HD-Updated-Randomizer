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
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_Main)).BeginInit();
            this.SuspendLayout();
            // 
            // button_Patch
            // 
            this.button_Patch.Location = new System.Drawing.Point(9, 407);
            this.button_Patch.Name = "button_Patch";
            this.button_Patch.Size = new System.Drawing.Size(106, 30);
            this.button_Patch.TabIndex = 0;
            this.button_Patch.Text = "Patch";
            this.button_Patch.UseVisualStyleBackColor = true;
            this.button_Patch.Click += new System.EventHandler(this.button_Patch_Click);
            // 
            // button_Exit
            // 
            this.button_Exit.Location = new System.Drawing.Point(251, 407);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(106, 30);
            this.button_Exit.TabIndex = 1;
            this.button_Exit.Text = "Exit";
            this.button_Exit.UseVisualStyleBackColor = true;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // button_ChangeLog
            // 
            this.button_ChangeLog.Location = new System.Drawing.Point(130, 407);
            this.button_ChangeLog.Name = "button_ChangeLog";
            this.button_ChangeLog.Size = new System.Drawing.Size(106, 30);
            this.button_ChangeLog.TabIndex = 3;
            this.button_ChangeLog.Text = "Changelog";
            this.button_ChangeLog.UseVisualStyleBackColor = true;
            this.button_ChangeLog.Click += new System.EventHandler(this.button_ChangeLog_Click);
            // 
            // groupBox_Main
            // 
            this.groupBox_Main.Location = new System.Drawing.Point(9, 247);
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
            this.progressBar.Location = new System.Drawing.Point(9, 384);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(347, 16);
            this.progressBar.TabIndex = 6;
            // 
            // Form_MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 443);
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
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_Patch;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.Button button_ChangeLog;
        private System.Windows.Forms.PictureBox picturebox_Main;
        public System.Windows.Forms.GroupBox groupBox_Main;
        public System.Windows.Forms.ProgressBar progressBar;
    }
}

