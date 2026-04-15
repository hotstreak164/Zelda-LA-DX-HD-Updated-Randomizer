namespace LADXHD_ModMaker
{
    partial class Form_ModForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ModForm));
            this.picturebox_Mod = new System.Windows.Forms.PictureBox();
            this.groupBox_ModDescription = new System.Windows.Forms.GroupBox();
            this.label_Description = new System.Windows.Forms.Label();
            this.label_ModName = new System.Windows.Forms.Label();
            this.button_Install = new System.Windows.Forms.Button();
            this.button_Close = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox_ModGamePath = new System.Windows.Forms.GroupBox();
            this.button_GamePath = new System.Windows.Forms.Button();
            this.label_GamePath = new System.Windows.Forms.Label();
            this.textBox_GamePath = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_Mod)).BeginInit();
            this.groupBox_ModDescription.SuspendLayout();
            this.groupBox_ModGamePath.SuspendLayout();
            this.SuspendLayout();
            // 
            // picturebox_Mod
            // 
            this.picturebox_Mod.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picturebox_Mod.ErrorImage = global::LADXHD_ModMaker.Properties.Resources.la;
            this.picturebox_Mod.Image = global::LADXHD_ModMaker.Properties.Resources.la;
            this.picturebox_Mod.InitialImage = global::LADXHD_ModMaker.Properties.Resources.la;
            this.picturebox_Mod.Location = new System.Drawing.Point(9, 0);
            this.picturebox_Mod.Name = "picturebox_Mod";
            this.picturebox_Mod.Size = new System.Drawing.Size(350, 248);
            this.picturebox_Mod.TabIndex = 1;
            this.picturebox_Mod.TabStop = false;
            // 
            // groupBox_ModDescription
            // 
            this.groupBox_ModDescription.Controls.Add(this.label_Description);
            this.groupBox_ModDescription.Controls.Add(this.label_ModName);
            this.groupBox_ModDescription.Location = new System.Drawing.Point(9, 316);
            this.groupBox_ModDescription.Name = "groupBox_ModDescription";
            this.groupBox_ModDescription.Size = new System.Drawing.Size(347, 112);
            this.groupBox_ModDescription.TabIndex = 2;
            this.groupBox_ModDescription.TabStop = false;
            // 
            // label_Description
            // 
            this.label_Description.Location = new System.Drawing.Point(5, 29);
            this.label_Description.Name = "label_Description";
            this.label_Description.Size = new System.Drawing.Size(340, 80);
            this.label_Description.TabIndex = 1;
            this.label_Description.Text = "This is where the description of the mod goes.";
            // 
            // label_ModName
            // 
            this.label_ModName.AutoSize = true;
            this.label_ModName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ModName.Location = new System.Drawing.Point(5, 9);
            this.label_ModName.Name = "label_ModName";
            this.label_ModName.Size = new System.Drawing.Size(170, 13);
            this.label_ModName.TabIndex = 0;
            this.label_ModName.Text = "The Name of the Mod v1.0.0";
            // 
            // button_Install
            // 
            this.button_Install.Location = new System.Drawing.Point(8, 458);
            this.button_Install.Name = "button_Install";
            this.button_Install.Size = new System.Drawing.Size(140, 30);
            this.button_Install.TabIndex = 8;
            this.button_Install.Text = "Install Mod";
            this.button_Install.UseVisualStyleBackColor = true;
            this.button_Install.Click += new System.EventHandler(this.button_Install_Click);
            // 
            // button_Close
            // 
            this.button_Close.Location = new System.Drawing.Point(217, 458);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(140, 30);
            this.button_Close.TabIndex = 9;
            this.button_Close.Text = "Exit";
            this.button_Close.UseVisualStyleBackColor = true;
            this.button_Close.Click += new System.EventHandler(this.button_Close_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(9, 435);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(347, 16);
            this.progressBar.TabIndex = 10;
            // 
            // groupBox_ModGamePath
            // 
            this.groupBox_ModGamePath.Controls.Add(this.button_GamePath);
            this.groupBox_ModGamePath.Controls.Add(this.label_GamePath);
            this.groupBox_ModGamePath.Controls.Add(this.textBox_GamePath);
            this.groupBox_ModGamePath.Location = new System.Drawing.Point(9, 251);
            this.groupBox_ModGamePath.Name = "groupBox_ModGamePath";
            this.groupBox_ModGamePath.Size = new System.Drawing.Size(351, 62);
            this.groupBox_ModGamePath.TabIndex = 3;
            this.groupBox_ModGamePath.TabStop = false;
            // 
            // button_GamePath
            // 
            this.button_GamePath.Location = new System.Drawing.Point(317, 29);
            this.button_GamePath.Name = "button_GamePath";
            this.button_GamePath.Size = new System.Drawing.Size(28, 24);
            this.button_GamePath.TabIndex = 12;
            this.button_GamePath.Text = "...";
            this.button_GamePath.UseVisualStyleBackColor = true;
            this.button_GamePath.Click += new System.EventHandler(this.button_GamePath_Click);
            // 
            // label_GamePath
            // 
            this.label_GamePath.AutoSize = true;
            this.label_GamePath.Location = new System.Drawing.Point(5, 12);
            this.label_GamePath.Name = "label_GamePath";
            this.label_GamePath.Size = new System.Drawing.Size(110, 13);
            this.label_GamePath.TabIndex = 11;
            this.label_GamePath.Text = "LADXHD Game Path:";
            // 
            // textBox_GamePath
            // 
            this.textBox_GamePath.AllowDrop = true;
            this.textBox_GamePath.Location = new System.Drawing.Point(5, 31);
            this.textBox_GamePath.Name = "textBox_GamePath";
            this.textBox_GamePath.Size = new System.Drawing.Size(308, 20);
            this.textBox_GamePath.TabIndex = 10;
            this.textBox_GamePath.TextChanged += new System.EventHandler(this.textBox_GamePath_TextChanged);
            this.textBox_GamePath.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox_GamePath_DragDrop);
            this.textBox_GamePath.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox_GamePath_DragEnter);
            this.textBox_GamePath.Leave += new System.EventHandler(this.textBox_GamePath_Leave);
            // 
            // Form_ModForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 495);
            this.Controls.Add(this.groupBox_ModGamePath);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.button_Close);
            this.Controls.Add(this.button_Install);
            this.Controls.Add(this.groupBox_ModDescription);
            this.Controls.Add(this.picturebox_Mod);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_ModForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ModForm";
            this.Load += new System.EventHandler(this.Form_ModForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_Mod)).EndInit();
            this.groupBox_ModDescription.ResumeLayout(false);
            this.groupBox_ModDescription.PerformLayout();
            this.groupBox_ModGamePath.ResumeLayout(false);
            this.groupBox_ModGamePath.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picturebox_Mod;
        private System.Windows.Forms.GroupBox groupBox_ModDescription;
        private System.Windows.Forms.Button button_Install;
        private System.Windows.Forms.Button button_Close;
        public System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.GroupBox groupBox_ModGamePath;
        private System.Windows.Forms.Button button_GamePath;
        private System.Windows.Forms.Label label_GamePath;
        private System.Windows.Forms.TextBox textBox_GamePath;
        private System.Windows.Forms.Label label_ModName;
        private System.Windows.Forms.Label label_Description;
    }
}