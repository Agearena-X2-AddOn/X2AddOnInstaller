namespace X2AddOnInstaller
{
	partial class MainForm
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this._aoe2FolderTextBox = new System.Windows.Forms.TextBox();
			this._folderSelectButton = new System.Windows.Forms.Button();
			this._exitButton = new System.Windows.Forms.Button();
			this._installButton = new System.Windows.Forms.Button();
			this._folderDialog = new System.Windows.Forms.FolderBrowserDialog();
			this._statusLabel = new System.Windows.Forms.Label();
			this._languageGermanButton = new System.Windows.Forms.RadioButton();
			this._languageEnglishButton = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// _aoe2FolderTextBox
			// 
			this._aoe2FolderTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(202)))), ((int)(((byte)(58)))));
			this._aoe2FolderTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._aoe2FolderTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._aoe2FolderTextBox.ForeColor = System.Drawing.Color.Black;
			this._aoe2FolderTextBox.Location = new System.Drawing.Point(568, 387);
			this._aoe2FolderTextBox.Name = "_aoe2FolderTextBox";
			this._aoe2FolderTextBox.Size = new System.Drawing.Size(233, 24);
			this._aoe2FolderTextBox.TabIndex = 0;
			// 
			// _folderSelectButton
			// 
			this._folderSelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._folderSelectButton.BackColor = System.Drawing.Color.Transparent;
			this._folderSelectButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this._folderSelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._folderSelectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._folderSelectButton.ForeColor = System.Drawing.Color.Black;
			this._folderSelectButton.Location = new System.Drawing.Point(815, 387);
			this._folderSelectButton.Name = "_folderSelectButton";
			this._folderSelectButton.Size = new System.Drawing.Size(31, 24);
			this._folderSelectButton.TabIndex = 1;
			this._folderSelectButton.Text = "...";
			this._folderSelectButton.UseVisualStyleBackColor = false;
			this._folderSelectButton.Click += new System.EventHandler(this._folderSelectButton_Click);
			// 
			// _exitButton
			// 
			this._exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._exitButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(167)))));
			this._exitButton.BackgroundImage = global::X2AddOnInstaller.Properties.Resources.ExitButton;
			this._exitButton.FlatAppearance.BorderSize = 0;
			this._exitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
			this._exitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this._exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._exitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._exitButton.ForeColor = System.Drawing.Color.Black;
			this._exitButton.Location = new System.Drawing.Point(836, 9);
			this._exitButton.Name = "_exitButton";
			this._exitButton.Size = new System.Drawing.Size(24, 24);
			this._exitButton.TabIndex = 2;
			this._exitButton.UseVisualStyleBackColor = false;
			this._exitButton.Click += new System.EventHandler(this._exitButton_Click);
			// 
			// _installButton
			// 
			this._installButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._installButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(167)))));
			this._installButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this._installButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._installButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._installButton.ForeColor = System.Drawing.Color.Black;
			this._installButton.Location = new System.Drawing.Point(654, 430);
			this._installButton.Name = "_installButton";
			this._installButton.Size = new System.Drawing.Size(192, 24);
			this._installButton.TabIndex = 3;
			this._installButton.Text = "Herunterladen und installieren!";
			this._installButton.UseVisualStyleBackColor = false;
			this._installButton.Click += new System.EventHandler(this._installButton_Click);
			// 
			// _statusLabel
			// 
			this._statusLabel.AutoEllipsis = true;
			this._statusLabel.BackColor = System.Drawing.Color.Transparent;
			this._statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._statusLabel.ForeColor = System.Drawing.Color.Black;
			this._statusLabel.Location = new System.Drawing.Point(4, 609);
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.Size = new System.Drawing.Size(860, 23);
			this._statusLabel.TabIndex = 4;
			this._statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _languageGermanButton
			// 
			this._languageGermanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._languageGermanButton.Appearance = System.Windows.Forms.Appearance.Button;
			this._languageGermanButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(167)))));
			this._languageGermanButton.BackgroundImage = global::X2AddOnInstaller.Properties.Resources.FlagGermany;
			this._languageGermanButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this._languageGermanButton.Checked = true;
			this._languageGermanButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.Yellow;
			this._languageGermanButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this._languageGermanButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._languageGermanButton.Location = new System.Drawing.Point(782, 9);
			this._languageGermanButton.Name = "_languageGermanButton";
			this._languageGermanButton.Size = new System.Drawing.Size(48, 24);
			this._languageGermanButton.TabIndex = 5;
			this._languageGermanButton.TabStop = true;
			this._languageGermanButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this._languageGermanButton.UseVisualStyleBackColor = false;
			// 
			// _languageEnglishButton
			// 
			this._languageEnglishButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._languageEnglishButton.Appearance = System.Windows.Forms.Appearance.Button;
			this._languageEnglishButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(167)))));
			this._languageEnglishButton.BackgroundImage = global::X2AddOnInstaller.Properties.Resources.FlagUnitedKingdom;
			this._languageEnglishButton.Enabled = false;
			this._languageEnglishButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.Yellow;
			this._languageEnglishButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this._languageEnglishButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._languageEnglishButton.Location = new System.Drawing.Point(728, 9);
			this._languageEnglishButton.Name = "_languageEnglishButton";
			this._languageEnglishButton.Size = new System.Drawing.Size(48, 24);
			this._languageEnglishButton.TabIndex = 6;
			this._languageEnglishButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this._languageEnglishButton.UseVisualStyleBackColor = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = global::X2AddOnInstaller.Properties.Resources.Background;
			this.ClientSize = new System.Drawing.Size(868, 637);
			this.Controls.Add(this._languageEnglishButton);
			this.Controls.Add(this._languageGermanButton);
			this.Controls.Add(this._statusLabel);
			this.Controls.Add(this._installButton);
			this.Controls.Add(this._exitButton);
			this.Controls.Add(this._folderSelectButton);
			this.Controls.Add(this._aoe2FolderTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "X2-AddOn :: Installieren";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseUp);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _aoe2FolderTextBox;
		private System.Windows.Forms.Button _folderSelectButton;
		private System.Windows.Forms.Button _exitButton;
		private System.Windows.Forms.Button _installButton;
		private System.Windows.Forms.FolderBrowserDialog _folderDialog;
		private System.Windows.Forms.Label _statusLabel;
		private System.Windows.Forms.RadioButton _languageGermanButton;
		private System.Windows.Forms.RadioButton _languageEnglishButton;
	}
}

