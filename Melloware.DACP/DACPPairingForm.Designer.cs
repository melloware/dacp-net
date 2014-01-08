/*
 * Copyright (C) 2009 Melloware Inc, http://melloware.com/
 */
 
using System;
using System.Windows.Forms;

namespace Melloware.DACP
{
	partial class DACPPairingForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DACPPairingForm));
			this.lblInfo = new System.Windows.Forms.Label();
			this.txtPassCode = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// lblInfo
			// 
			this.lblInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblInfo.Location = new System.Drawing.Point(99, 9);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(292, 53);
			this.lblInfo.TabIndex = 0;
			this.lblInfo.Text = "A device wants to pair with this server.  Please enter the code displayed on the " +
			"device:";
			// 
			// txtPassCode
			// 
			this.txtPassCode.Font = new System.Drawing.Font("Courier New", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPassCode.Location = new System.Drawing.Point(140, 69);
			this.txtPassCode.MaxLength = 4;
			this.txtPassCode.Name = "txtPassCode";
			this.txtPassCode.Size = new System.Drawing.Size(114, 32);
			this.txtPassCode.TabIndex = 1;
			this.txtPassCode.WordWrap = false;
			this.txtPassCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtPassCodeKeyPress);
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(99, 116);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "&OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(208, 116);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(67, 72);
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			// 
			// DACPPairingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(394, 151);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.txtPassCode);
			this.Controls.Add(this.lblInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Location = new System.Drawing.Point(1, 1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DACPPairingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Pairing In Process...";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.TextBox txtPassCode;
		private System.Windows.Forms.Label lblInfo;
		
		void TxtPassCodeKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Return) {
				BtnOKClick(this, null);
			}
		}
		
		void BtnOKClick(object sender, System.EventArgs e)
		{
			if (txtPassCode.Text.Length != 4) {
				MessageBox.Show("Must be 4 digit passcode");
			} else {
				this.DialogResult = DialogResult.OK;
			}
		}
	}
}
