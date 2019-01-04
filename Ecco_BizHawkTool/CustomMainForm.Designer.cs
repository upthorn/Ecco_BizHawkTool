namespace BizHawk.Client.EmuHawk
{
	public partial class CustomMainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomMainForm));
			this.mapDumpFolder = new System.Windows.Forms.TextBox();
			this.mapDumpCheckbox = new System.Windows.Forms.CheckBox();
			this.mapDumpFolder_Label = new System.Windows.Forms.Label();
			this.autoFireCheckbox = new System.Windows.Forms.CheckBox();
			this.mapDumpFolderBrowse = new System.Windows.Forms.Button();
			this.mapFolderBrowseDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.StatusLabel = new System.Windows.Forms.Label();
			this.StatusTextBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// mapDumpFolder
			// 
			this.mapDumpFolder.Location = new System.Drawing.Point(300, 10);
			this.mapDumpFolder.Name = "mapDumpFolder";
			this.mapDumpFolder.Size = new System.Drawing.Size(100, 20);
			this.mapDumpFolder.TabIndex = 3;
			// 
			// mapDumpCheckbox
			// 
			this.mapDumpCheckbox.AutoSize = true;
			this.mapDumpCheckbox.Location = new System.Drawing.Point(110, 10);
			this.mapDumpCheckbox.Name = "mapDumpCheckbox";
			this.mapDumpCheckbox.Size = new System.Drawing.Size(128, 17);
			this.mapDumpCheckbox.TabIndex = 1;
			this.mapDumpCheckbox.Text = "Enable Map Dumping";
			this.mapDumpCheckbox.UseVisualStyleBackColor = true;
			this.mapDumpCheckbox.CheckedChanged += new System.EventHandler(this.mapDumpCheckBox_CheckedChanged);
			// 
			// mapDumpFolder_Label
			// 
			this.mapDumpFolder_Label.AutoSize = true;
			this.mapDumpFolder_Label.Location = new System.Drawing.Point(235, 12);
			this.mapDumpFolder_Label.Name = "mapDumpFolder_Label";
			this.mapDumpFolder_Label.Size = new System.Drawing.Size(63, 13);
			this.mapDumpFolder_Label.TabIndex = 2;
			this.mapDumpFolder_Label.Text = "Map Folder:";
			// 
			// autoFireCheckbox
			// 
			this.autoFireCheckbox.AutoSize = true;
			this.autoFireCheckbox.Location = new System.Drawing.Point(10, 10);
			this.autoFireCheckbox.Name = "autoFireCheckbox";
			this.autoFireCheckbox.Size = new System.Drawing.Size(98, 17);
			this.autoFireCheckbox.TabIndex = 0;
			this.autoFireCheckbox.Text = "Enable Autofire";
			this.autoFireCheckbox.UseVisualStyleBackColor = true;
			this.autoFireCheckbox.CheckedChanged += new System.EventHandler(this.autoFireCheckbox_CheckedChanged);
			// 
			// mapDumpFolderBrowse
			// 
			this.mapDumpFolderBrowse.Location = new System.Drawing.Point(420, 10);
			this.mapDumpFolderBrowse.Name = "mapDumpFolderBrowse";
			this.mapDumpFolderBrowse.Size = new System.Drawing.Size(61, 23);
			this.mapDumpFolderBrowse.TabIndex = 4;
			this.mapDumpFolderBrowse.Text = "Browse";
			this.mapDumpFolderBrowse.UseVisualStyleBackColor = true;
			this.mapDumpFolderBrowse.Click += new System.EventHandler(this.mapDumpFolderBrowse_Click);
			// 
			// StatusLabel
			// 
			this.StatusLabel.AutoSize = true;
			this.StatusLabel.Location = new System.Drawing.Point(10, 30);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(71, 13);
			this.StatusLabel.TabIndex = 5;
			this.StatusLabel.Text = "Game Status:";
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.StatusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.StatusTextBox.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StatusTextBox.ForeColor = System.Drawing.Color.Black;
			this.StatusTextBox.Location = new System.Drawing.Point(8, 46);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.StatusTextBox.Size = new System.Drawing.Size(640, 480);
			this.StatusTextBox.TabIndex = 6;
			this.StatusTextBox.TabStop = false;
			this.StatusTextBox.Text = "";
			this.StatusTextBox.WordWrap = false;
			// 
			// CustomMainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(656, 534);
			this.Controls.Add(this.StatusTextBox);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.mapDumpFolderBrowse);
			this.Controls.Add(this.autoFireCheckbox);
			this.Controls.Add(this.mapDumpFolder_Label);
			this.Controls.Add(this.mapDumpCheckbox);
			this.Controls.Add(this.mapDumpFolder);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "CustomMainForm";
			this.Text = "Ecco Tas Assistant";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.CheckBox mapDumpCheckbox;
		private System.Windows.Forms.Label mapDumpFolder_Label;
		private System.Windows.Forms.CheckBox autoFireCheckbox;
		private System.Windows.Forms.Button mapDumpFolderBrowse;
		private System.Windows.Forms.FolderBrowserDialog mapFolderBrowseDialog;
		public System.Windows.Forms.TextBox mapDumpFolder;
		private System.Windows.Forms.Label StatusLabel;
		public System.Windows.Forms.RichTextBox StatusTextBox;
	}
}