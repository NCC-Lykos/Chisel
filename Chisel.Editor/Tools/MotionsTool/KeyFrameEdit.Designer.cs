namespace Chisel.Editor.Tools.MotionsTool
{
    partial class KeyFrameEdit
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
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tX = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tY = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tZ = new System.Windows.Forms.TextBox();
            this.rX = new System.Windows.Forms.TextBox();
            this.rY = new System.Windows.Forms.TextBox();
            this.rZ = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(75, 77);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.EditClicked);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(156, 77);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.CancelClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Translate";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Rotate";
            // 
            // tX
            // 
            this.tX.Location = new System.Drawing.Point(69, 25);
            this.tX.Name = "tX";
            this.tX.Size = new System.Drawing.Size(50, 20);
            this.tX.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(87, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "X";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(143, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Y";
            // 
            // tY
            // 
            this.tY.Location = new System.Drawing.Point(125, 25);
            this.tY.Name = "tY";
            this.tY.Size = new System.Drawing.Size(50, 20);
            this.tY.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(199, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Z";
            // 
            // tZ
            // 
            this.tZ.Location = new System.Drawing.Point(181, 25);
            this.tZ.Name = "tZ";
            this.tZ.Size = new System.Drawing.Size(50, 20);
            this.tZ.TabIndex = 9;
            // 
            // rX
            // 
            this.rX.Location = new System.Drawing.Point(69, 51);
            this.rX.Name = "rX";
            this.rX.Size = new System.Drawing.Size(50, 20);
            this.rX.TabIndex = 15;
            // 
            // rY
            // 
            this.rY.Location = new System.Drawing.Point(125, 51);
            this.rY.Name = "rY";
            this.rY.Size = new System.Drawing.Size(50, 20);
            this.rY.TabIndex = 16;
            // 
            // rZ
            // 
            this.rZ.Location = new System.Drawing.Point(181, 51);
            this.rZ.Name = "rZ";
            this.rZ.Size = new System.Drawing.Size(50, 20);
            this.rZ.TabIndex = 17;
            // 
            // KeyFrameEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 112);
            this.ControlBox = false;
            this.Controls.Add(this.rZ);
            this.Controls.Add(this.rY);
            this.Controls.Add(this.rX);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tZ);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tY);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tX);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnEdit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeyFrameEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit Keyframe";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tZ;
        private System.Windows.Forms.TextBox rX;
        private System.Windows.Forms.TextBox rY;
        private System.Windows.Forms.TextBox rZ;
    }
}