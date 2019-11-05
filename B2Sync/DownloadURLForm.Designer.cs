namespace B2Sync
{
	partial class DownloadURLForm
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
			this.downloadUrlDisplay = new System.Windows.Forms.LinkLabel();
			this.durationLabel = new System.Windows.Forms.Label();
			this.durationInput = new System.Windows.Forms.NumericUpDown();
			this.createUrlButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.durationInput)).BeginInit();
			this.SuspendLayout();
			// 
			// downloadUrlDisplay
			// 
			this.downloadUrlDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.downloadUrlDisplay.AutoSize = true;
			this.downloadUrlDisplay.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.downloadUrlDisplay.Location = new System.Drawing.Point(13, 66);
			this.downloadUrlDisplay.Name = "downloadUrlDisplay";
			this.downloadUrlDisplay.Size = new System.Drawing.Size(147, 29);
			this.downloadUrlDisplay.TabIndex = 0;
			this.downloadUrlDisplay.TabStop = true;
			this.downloadUrlDisplay.Text = "Dummy Text";
			this.downloadUrlDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// durationLabel
			// 
			this.durationLabel.AutoSize = true;
			this.durationLabel.Location = new System.Drawing.Point(13, 13);
			this.durationLabel.Name = "durationLabel";
			this.durationLabel.Size = new System.Drawing.Size(212, 29);
			this.durationLabel.TabIndex = 2;
			this.durationLabel.Text = "Valid Duration (hr):";
			// 
			// durationInput
			// 
			this.durationInput.Location = new System.Drawing.Point(231, 11);
			this.durationInput.Maximum = new decimal(new int[] {
            168,
            0,
            0,
            0});
			this.durationInput.Name = "durationInput";
			this.durationInput.Size = new System.Drawing.Size(120, 35);
			this.durationInput.TabIndex = 3;
			this.durationInput.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
			// 
			// createUrlButton
			// 
			this.createUrlButton.Location = new System.Drawing.Point(358, 11);
			this.createUrlButton.Name = "createUrlButton";
			this.createUrlButton.Size = new System.Drawing.Size(166, 35);
			this.createUrlButton.TabIndex = 4;
			this.createUrlButton.Text = "Create URL";
			this.createUrlButton.UseVisualStyleBackColor = true;
			// 
			// DownloadURLForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(684, 113);
			this.Controls.Add(this.createUrlButton);
			this.Controls.Add(this.durationInput);
			this.Controls.Add(this.durationLabel);
			this.Controls.Add(this.downloadUrlDisplay);
			this.Name = "DownloadURLForm";
			this.Text = "Download URL";
			((System.ComponentModel.ISupportInitialize)(this.durationInput)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.LinkLabel downloadUrlDisplay;
		private System.Windows.Forms.Label durationLabel;
		private System.Windows.Forms.NumericUpDown durationInput;
		private System.Windows.Forms.Button createUrlButton;
	}
}