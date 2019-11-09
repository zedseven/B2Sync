namespace B2Sync
{
	partial class OptionsForm
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
			this.label2 = new System.Windows.Forms.Label();
			this.keyIdInput = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.applicationKeyInput = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(133, 29);
			this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(95, 29);
			this.label2.TabIndex = 3;
			this.label2.Text = "Key ID: ";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// keyIdInput
			// 
			this.keyIdInput.AccessibleName = "Key ID Text Input";
			this.keyIdInput.Location = new System.Drawing.Point(252, 22);
			this.keyIdInput.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.keyIdInput.Name = "keyIdInput";
			this.keyIdInput.Size = new System.Drawing.Size(228, 35);
			this.keyIdInput.TabIndex = 2;
			this.keyIdInput.TextChanged += new System.EventHandler(this.OptionsFormTextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(37, 87);
			this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(191, 29);
			this.label3.TabIndex = 5;
			this.label3.Text = "Application Key: ";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// applicationKeyInput
			// 
			this.applicationKeyInput.AccessibleName = "Application Key Text Input";
			this.applicationKeyInput.Location = new System.Drawing.Point(252, 80);
			this.applicationKeyInput.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.applicationKeyInput.Name = "applicationKeyInput";
			this.applicationKeyInput.PasswordChar = '•';
			this.applicationKeyInput.Size = new System.Drawing.Size(228, 35);
			this.applicationKeyInput.TabIndex = 4;
			this.applicationKeyInput.TextChanged += new System.EventHandler(this.OptionsFormTextChanged);
			// 
			// OptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(616, 136);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.applicationKeyInput);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.keyIdInput);
			this.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
			this.Name = "OptionsForm";
			this.Text = "B2Sync Options";
			this.Load += new System.EventHandler(this.OptionsForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox keyIdInput;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox applicationKeyInput;
	}
}