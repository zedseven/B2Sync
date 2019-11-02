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
			this.accountIdInput = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.keyIdInput = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.applicationKeyInput = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.bucketIdInput = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// accountIdInput
			// 
			this.accountIdInput.AccessibleName = "Account ID Text Input";
			this.accountIdInput.Location = new System.Drawing.Point(105, 10);
			this.accountIdInput.Name = "accountIdInput";
			this.accountIdInput.Size = new System.Drawing.Size(100, 20);
			this.accountIdInput.TabIndex = 0;
			this.accountIdInput.TextChanged += new System.EventHandler(this.OptionsFormTextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(32, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(67, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Account ID: ";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(54, 39);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(45, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Key ID: ";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// keyIdInput
			// 
			this.keyIdInput.AccessibleName = "Key ID Text Input";
			this.keyIdInput.Location = new System.Drawing.Point(105, 36);
			this.keyIdInput.Name = "keyIdInput";
			this.keyIdInput.Size = new System.Drawing.Size(100, 20);
			this.keyIdInput.TabIndex = 2;
			this.keyIdInput.TextChanged += new System.EventHandler(this.OptionsFormTextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 65);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(86, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Application Key: ";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// applicationKeyInput
			// 
			this.applicationKeyInput.AccessibleName = "Application Key Text Input";
			this.applicationKeyInput.Location = new System.Drawing.Point(105, 62);
			this.applicationKeyInput.Name = "applicationKeyInput";
			this.applicationKeyInput.Size = new System.Drawing.Size(100, 20);
			this.applicationKeyInput.TabIndex = 4;
			this.applicationKeyInput.TextChanged += new System.EventHandler(this.OptionsFormTextChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(38, 91);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(61, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Bucket ID: ";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// bucketIdInput
			// 
			this.bucketIdInput.AccessibleName = "Bucket ID Text Input";
			this.bucketIdInput.Location = new System.Drawing.Point(105, 88);
			this.bucketIdInput.Name = "bucketIdInput";
			this.bucketIdInput.Size = new System.Drawing.Size(100, 20);
			this.bucketIdInput.TabIndex = 6;
			this.bucketIdInput.TextChanged += new System.EventHandler(this.OptionsFormTextChanged);
			// 
			// OptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(220, 122);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.bucketIdInput);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.applicationKeyInput);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.keyIdInput);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.accountIdInput);
			this.Name = "OptionsForm";
			this.Text = "B2Sync Options";
			this.Load += new System.EventHandler(this.OptionsForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox accountIdInput;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox keyIdInput;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox applicationKeyInput;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox bucketIdInput;
	}
}