namespace LADXHD_Migrater
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
            this.button_Migrate = new System.Windows.Forms.Button();
            this.button_Patches = new System.Windows.Forms.Button();
            this.button_Exit = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button_Build = new System.Windows.Forms.Button();
            this.button_Clean = new System.Windows.Forms.Button();
            this.label_Platform = new System.Windows.Forms.Label();
            this.comboBox_Platform = new System.Windows.Forms.ComboBox();
            this.combBox_API = new System.Windows.Forms.ComboBox();
            this.label_API = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_Migrate
            // 
            this.button_Migrate.Location = new System.Drawing.Point(9, 282);
            this.button_Migrate.Name = "button_Migrate";
            this.button_Migrate.Size = new System.Drawing.Size(100, 40);
            this.button_Migrate.TabIndex = 0;
            this.button_Migrate.Text = "Migrate Assets From v1.0.0";
            this.button_Migrate.UseVisualStyleBackColor = true;
            this.button_Migrate.Click += new System.EventHandler(this.button_Migrate_Click);
            // 
            // button_Patches
            // 
            this.button_Patches.Location = new System.Drawing.Point(117, 282);
            this.button_Patches.Name = "button_Patches";
            this.button_Patches.Size = new System.Drawing.Size(100, 40);
            this.button_Patches.TabIndex = 1;
            this.button_Patches.Text = "Create Patches of Updated Assets";
            this.button_Patches.UseVisualStyleBackColor = true;
            this.button_Patches.Click += new System.EventHandler(this.button_Patches_click);
            // 
            // button_Exit
            // 
            this.button_Exit.Location = new System.Drawing.Point(225, 282);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(100, 40);
            this.button_Exit.TabIndex = 2;
            this.button_Exit.Text = "Exit";
            this.button_Exit.UseVisualStyleBackColor = true;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::LADXHD_Migrater.Properties.Resources.la;
            this.pictureBox1.Location = new System.Drawing.Point(-8, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(350, 248);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // button_Build
            // 
            this.button_Build.Location = new System.Drawing.Point(171, 329);
            this.button_Build.Name = "button_Build";
            this.button_Build.Size = new System.Drawing.Size(153, 40);
            this.button_Build.TabIndex = 4;
            this.button_Build.Text = "Create a New Build";
            this.button_Build.UseVisualStyleBackColor = true;
            this.button_Build.Click += new System.EventHandler(this.button_Build_Click);
            // 
            // button_Clean
            // 
            this.button_Clean.Location = new System.Drawing.Point(9, 329);
            this.button_Clean.Name = "button_Clean";
            this.button_Clean.Size = new System.Drawing.Size(153, 40);
            this.button_Clean.TabIndex = 5;
            this.button_Clean.Text = "Clean Build Files";
            this.button_Clean.UseVisualStyleBackColor = true;
            this.button_Clean.Click += new System.EventHandler(this.button_Clean_Click);
            // 
            // label_Platform
            // 
            this.label_Platform.AutoSize = true;
            this.label_Platform.Location = new System.Drawing.Point(13, 258);
            this.label_Platform.Name = "label_Platform";
            this.label_Platform.Size = new System.Drawing.Size(48, 13);
            this.label_Platform.TabIndex = 6;
            this.label_Platform.Text = "Platform:";
            // 
            // comboBox_Platform
            // 
            this.comboBox_Platform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Platform.FormattingEnabled = true;
            this.comboBox_Platform.Items.AddRange(new object[] {
            "Windows",
            "Android",
            "Linux (x86-64)",
            "Linux (Arm64)"});
            this.comboBox_Platform.Location = new System.Drawing.Point(63, 255);
            this.comboBox_Platform.Name = "comboBox_Platform";
            this.comboBox_Platform.Size = new System.Drawing.Size(97, 21);
            this.comboBox_Platform.TabIndex = 7;
            this.comboBox_Platform.SelectedIndexChanged += new System.EventHandler(this.comboBox_Platform_SelectedIndexChanged);
            // 
            // combBox_API
            // 
            this.combBox_API.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combBox_API.FormattingEnabled = true;
            this.combBox_API.Items.AddRange(new object[] {
            "DirectX",
            "OpenGL"});
            this.combBox_API.Location = new System.Drawing.Point(217, 255);
            this.combBox_API.Name = "combBox_API";
            this.combBox_API.Size = new System.Drawing.Size(97, 21);
            this.combBox_API.TabIndex = 9;
            this.combBox_API.SelectedIndexChanged += new System.EventHandler(this.combBox_API_SelectedIndexChanged);
            // 
            // label_API
            // 
            this.label_API.AutoSize = true;
            this.label_API.Location = new System.Drawing.Point(172, 258);
            this.label_API.Name = "label_API";
            this.label_API.Size = new System.Drawing.Size(41, 13);
            this.label_API.TabIndex = 8;
            this.label_API.Text = "Target:";
            // 
            // Form_MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 378);
            this.Controls.Add(this.combBox_API);
            this.Controls.Add(this.label_API);
            this.Controls.Add(this.comboBox_Platform);
            this.Controls.Add(this.label_Platform);
            this.Controls.Add(this.button_Clean);
            this.Controls.Add(this.button_Build);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button_Exit);
            this.Controls.Add(this.button_Patches);
            this.Controls.Add(this.button_Migrate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Link\'s Awakening DX HD Migration Tool";
            this.Load += new System.EventHandler(this.Form_MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Migrate;
        private System.Windows.Forms.Button button_Patches;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button_Build;
        private System.Windows.Forms.Button button_Clean;
        private System.Windows.Forms.Label label_Platform;
        private System.Windows.Forms.ComboBox comboBox_Platform;
        private System.Windows.Forms.ComboBox combBox_API;
        private System.Windows.Forms.Label label_API;
    }
}

