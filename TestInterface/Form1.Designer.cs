
namespace TestInterface
	{
	partial class Form1
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
			this.StatusTextBox = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.ModelComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.PTNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.RIButton = new System.Windows.Forms.Button();
			this.LOMButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ULUCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PTNumericUpDown)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Location = new System.Drawing.Point(12, 31);
			this.StatusTextBox.Multiline = true;
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.StatusTextBox.Size = new System.Drawing.Size(458, 448);
			this.StatusTextBox.TabIndex = 70;
			this.StatusTextBox.WordWrap = false;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.ModelComboBox);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.PTNumericUpDown);
			this.groupBox3.Controls.Add(this.RIButton);
			this.groupBox3.Location = new System.Drawing.Point(540, 64);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(289, 221);
			this.groupBox3.TabIndex = 72;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Inference";
			// 
			// ModelComboBox
			// 
			this.ModelComboBox.FormattingEnabled = true;
			this.ModelComboBox.Location = new System.Drawing.Point(50, 41);
			this.ModelComboBox.Name = "ModelComboBox";
			this.ModelComboBox.Size = new System.Drawing.Size(189, 28);
			this.ModelComboBox.TabIndex = 74;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(37, 111);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(151, 20);
			this.label1.TabIndex = 73;
			this.label1.Text = "Probability threshold";
			// 
			// PTNumericUpDown
			// 
			this.PTNumericUpDown.DecimalPlaces = 2;
			this.PTNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.PTNumericUpDown.Location = new System.Drawing.Point(189, 108);
			this.PTNumericUpDown.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            131072});
			this.PTNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.PTNumericUpDown.Name = "PTNumericUpDown";
			this.PTNumericUpDown.Size = new System.Drawing.Size(62, 26);
			this.PTNumericUpDown.TabIndex = 72;
			this.PTNumericUpDown.Value = new decimal(new int[] {
            8,
            0,
            0,
            65536});
			// 
			// RIButton
			// 
			this.RIButton.Location = new System.Drawing.Point(73, 173);
			this.RIButton.Name = "RIButton";
			this.RIButton.Size = new System.Drawing.Size(143, 25);
			this.RIButton.TabIndex = 71;
			this.RIButton.Text = "Run inferences";
			this.RIButton.UseVisualStyleBackColor = true;
			this.RIButton.Click += new System.EventHandler(this.RIButton_Click);
			// 
			// LOMButton
			// 
			this.LOMButton.Location = new System.Drawing.Point(71, 39);
			this.LOMButton.Name = "LOMButton";
			this.LOMButton.Size = new System.Drawing.Size(143, 25);
			this.LOMButton.TabIndex = 73;
			this.LOMButton.Text = "Load all models";
			this.LOMButton.UseVisualStyleBackColor = true;
			this.LOMButton.Click += new System.EventHandler(this.LOMButton_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.ULUCheckBox);
			this.groupBox1.Controls.Add(this.LOMButton);
			this.groupBox1.Location = new System.Drawing.Point(542, 325);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(289, 121);
			this.groupBox1.TabIndex = 74;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Load all models";
			// 
			// ULUCheckBox
			// 
			this.ULUCheckBox.AutoSize = true;
			this.ULUCheckBox.Location = new System.Drawing.Point(53, 84);
			this.ULUCheckBox.Name = "ULUCheckBox";
			this.ULUCheckBox.Size = new System.Drawing.Size(182, 24);
			this.ULUCheckBox.TabIndex = 74;
			this.ULUCheckBox.Text = "Unload on completion";
			this.ULUCheckBox.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(859, 510);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.StatusTextBox);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.ShowIcon = false;
			this.Text = "Jetson AGX Orin Test Interface";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PTNumericUpDown)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

			}

		#endregion

		private System.Windows.Forms.TextBox StatusTextBox;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ComboBox ModelComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown PTNumericUpDown;
		private System.Windows.Forms.Button RIButton;
		private System.Windows.Forms.Button LOMButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox ULUCheckBox;
		}
	}

