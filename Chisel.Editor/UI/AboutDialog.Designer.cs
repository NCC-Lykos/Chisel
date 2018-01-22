﻿namespace Chisel.Editor.UI
{
    partial class AboutDialog
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
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LTLink = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.GithubLink = new System.Windows.Forms.LinkLabel();
            this.GPLLink = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.TWHLLink = new System.Windows.Forms.LinkLabel();
            this.label10 = new System.Windows.Forms.Label();
            this.AJLink = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(213, 355);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Chisel.Editor.Properties.Resources.Chisel_Large;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(256, 256);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("HandelGothicBold", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(338, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 47);
            this.label1.TabIndex = 3;
            this.label1.Text = "Chisel";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(74, 16);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(47, 13);
            this.VersionLabel.TabIndex = 4;
            this.VersionLabel.Text = "(version)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(325, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(172, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Based on Sledge by Daniel Walder";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // LTLink
            // 
            this.LTLink.AutoSize = true;
            this.LTLink.Location = new System.Drawing.Point(364, 111);
            this.LTLink.Name = "LTLink";
            this.LTLink.Size = new System.Drawing.Size(96, 13);
            this.LTLink.TabIndex = 6;
            this.LTLink.TabStop = true;
            this.LTLink.Text = "logic-and-trick.com";
            this.LTLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LTLink_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(285, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(231, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Open source software - licensed under the GPL";
            // 
            // GithubLink
            // 
            this.GithubLink.AutoSize = true;
            this.GithubLink.Location = new System.Drawing.Point(347, 155);
            this.GithubLink.Name = "GithubLink";
            this.GithubLink.Size = new System.Drawing.Size(150, 13);
            this.GithubLink.TabIndex = 6;
            this.GithubLink.TabStop = true;
            this.GithubLink.Text = "github.com/NCC-Lykos/Chisel";
            // 
            // GPLLink
            // 
            this.GPLLink.AutoSize = true;
            this.GPLLink.Location = new System.Drawing.Point(347, 177);
            this.GPLLink.Name = "GPLLink";
            this.GPLLink.Size = new System.Drawing.Size(145, 13);
            this.GPLLink.TabIndex = 6;
            this.GPLLink.TabStop = true;
            this.GPLLink.Text = "gnu.org/licenses/gpl-2.0.html";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(297, 155);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Source:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(294, 177);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "License:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(285, 199);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Special Thanks";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(295, 219);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(215, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Strider && Instant Mix (from TWHL) - Graphics";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(295, 237);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(123, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "AJ Scarcella - Website - ";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(289, 255);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(171, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "The members of TWHL - Testing - ";
            // 
            // TWHLLink
            // 
            this.TWHLLink.AutoSize = true;
            this.TWHLLink.Location = new System.Drawing.Point(464, 255);
            this.TWHLLink.Name = "TWHLLink";
            this.TWHLLink.Size = new System.Drawing.Size(46, 13);
            this.TWHLLink.TabIndex = 6;
            this.TWHLLink.TabStop = true;
            this.TWHLLink.Text = "twhl.info";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(295, 291);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(231, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "...and everyone else in the modding community!";
            // 
            // AJLink
            // 
            this.AJLink.AutoSize = true;
            this.AJLink.Location = new System.Drawing.Point(416, 237);
            this.AJLink.Name = "AJLink";
            this.AJLink.Size = new System.Drawing.Size(92, 13);
            this.AJLink.TabIndex = 6;
            this.AJLink.TabStop = true;
            this.AJLink.Text = "scrub-studios.com";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.VersionLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 278);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(256, 71);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Version Info";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // AboutDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 382);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.GPLLink);
            this.Controls.Add(this.GithubLink);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AJLink);
            this.Controls.Add(this.TWHLLink);
            this.Controls.Add(this.LTLink);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Chisel";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel LTLink;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel GithubLink;
        private System.Windows.Forms.LinkLabel GPLLink;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.LinkLabel TWHLLink;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.LinkLabel AJLink;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}