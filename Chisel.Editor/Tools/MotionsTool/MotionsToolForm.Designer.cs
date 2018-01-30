using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Chisel.Common;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;
//using Chisel.Editor.UI;
using Chisel.Providers.Texture;
using Chisel.Settings;
using Chisel.Settings.Models;

namespace Chisel.Editor.Tools.MotionsTool
{
    partial class MotionsToolForm
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
            this.MotionsList = new System.Windows.Forms.CheckedListBox();
            this.grpRaw = new System.Windows.Forms.GroupBox();
            this.btnSetOriginCenter = new System.Windows.Forms.Button();
            this.txtCurrentKey = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMotionID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMotionName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AddMotion = new System.Windows.Forms.Button();
            this.RemoveMotion = new System.Windows.Forms.Button();
            this.grpEditKeyframes = new System.Windows.Forms.GroupBox();
            this.btnSetRotation = new System.Windows.Forms.Button();
            this.btnSetMovement = new System.Windows.Forms.Button();
            this.btnRemoveKeyframe = new System.Windows.Forms.Button();
            this.btnAddKeyFrame = new System.Windows.Forms.Button();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.btnUpdateMotion = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtOrigX = new System.Windows.Forms.TextBox();
            this.txtOrigY = new System.Windows.Forms.TextBox();
            this.txtOrigZ = new System.Windows.Forms.TextBox();
            this.btnAnimate = new System.Windows.Forms.Button();
            this.btnStopAnimation = new System.Windows.Forms.Button();
            this.grpRaw.SuspendLayout();
            this.grpEditKeyframes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.SuspendLayout();
            // 
            // MotionsList
            // 
            this.MotionsList.FormattingEnabled = true;
            this.MotionsList.Location = new System.Drawing.Point(12, 12);
            this.MotionsList.Name = "MotionsList";
            this.MotionsList.Size = new System.Drawing.Size(120, 304);
            this.MotionsList.TabIndex = 0;
            this.MotionsList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.MotionSelectionChanged);
            // 
            // grpRaw
            // 
            this.grpRaw.Controls.Add(this.txtOrigZ);
            this.grpRaw.Controls.Add(this.txtOrigY);
            this.grpRaw.Controls.Add(this.btnUpdateMotion);
            this.grpRaw.Controls.Add(this.txtOrigX);
            this.grpRaw.Controls.Add(this.label4);
            this.grpRaw.Controls.Add(this.btnSetOriginCenter);
            this.grpRaw.Controls.Add(this.txtCurrentKey);
            this.grpRaw.Controls.Add(this.txtMotionID);
            this.grpRaw.Controls.Add(this.label2);
            this.grpRaw.Controls.Add(this.label3);
            this.grpRaw.Controls.Add(this.txtMotionName);
            this.grpRaw.Controls.Add(this.label1);
            this.grpRaw.Location = new System.Drawing.Point(138, 263);
            this.grpRaw.Name = "grpRaw";
            this.grpRaw.Size = new System.Drawing.Size(575, 81);
            this.grpRaw.TabIndex = 1;
            this.grpRaw.TabStop = false;
            this.grpRaw.Text = "Motion Data";
            // 
            // btnSetOriginCenter
            // 
            this.btnSetOriginCenter.Location = new System.Drawing.Point(409, 23);
            this.btnSetOriginCenter.Name = "btnSetOriginCenter";
            this.btnSetOriginCenter.Size = new System.Drawing.Size(160, 23);
            this.btnSetOriginCenter.TabIndex = 4;
            this.btnSetOriginCenter.Text = "Set Origin to center of Solids";
            this.btnSetOriginCenter.UseVisualStyleBackColor = true;
            // 
            // txtCurrentKey
            // 
            this.txtCurrentKey.Location = new System.Drawing.Point(262, 47);
            this.txtCurrentKey.Name = "txtCurrentKey";
            this.txtCurrentKey.Size = new System.Drawing.Size(141, 20);
            this.txtCurrentKey.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(194, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Current Key";
            // 
            // txtMotionID
            // 
            this.txtMotionID.Enabled = false;
            this.txtMotionID.Location = new System.Drawing.Point(47, 47);
            this.txtMotionID.Name = "txtMotionID";
            this.txtMotionID.Size = new System.Drawing.Size(141, 20);
            this.txtMotionID.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "ID";
            // 
            // txtMotionName
            // 
            this.txtMotionName.Location = new System.Drawing.Point(47, 25);
            this.txtMotionName.Name = "txtMotionName";
            this.txtMotionName.Size = new System.Drawing.Size(141, 20);
            this.txtMotionName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // AddMotion
            // 
            this.AddMotion.Location = new System.Drawing.Point(12, 321);
            this.AddMotion.Name = "AddMotion";
            this.AddMotion.Size = new System.Drawing.Size(54, 23);
            this.AddMotion.TabIndex = 2;
            this.AddMotion.Text = "Add";
            this.AddMotion.UseVisualStyleBackColor = true;
            // 
            // RemoveMotion
            // 
            this.RemoveMotion.Location = new System.Drawing.Point(72, 321);
            this.RemoveMotion.Name = "RemoveMotion";
            this.RemoveMotion.Size = new System.Drawing.Size(60, 23);
            this.RemoveMotion.TabIndex = 3;
            this.RemoveMotion.Text = "Remove";
            this.RemoveMotion.UseVisualStyleBackColor = true;
            // 
            // grpEditKeyframes
            // 
            this.grpEditKeyframes.Controls.Add(this.btnSetRotation);
            this.grpEditKeyframes.Controls.Add(this.btnSetMovement);
            this.grpEditKeyframes.Controls.Add(this.btnRemoveKeyframe);
            this.grpEditKeyframes.Controls.Add(this.btnAddKeyFrame);
            this.grpEditKeyframes.Controls.Add(this.dataGridView2);
            this.grpEditKeyframes.Location = new System.Drawing.Point(138, 41);
            this.grpEditKeyframes.Name = "grpEditKeyframes";
            this.grpEditKeyframes.Size = new System.Drawing.Size(575, 216);
            this.grpEditKeyframes.TabIndex = 29;
            this.grpEditKeyframes.TabStop = false;
            this.grpEditKeyframes.Text = "Keyframe Data";
            // 
            // btnSetRotation
            // 
            this.btnSetRotation.Location = new System.Drawing.Point(377, 184);
            this.btnSetRotation.Name = "btnSetRotation";
            this.btnSetRotation.Size = new System.Drawing.Size(93, 23);
            this.btnSetRotation.TabIndex = 33;
            this.btnSetRotation.Text = "Set Rotation";
            this.btnSetRotation.UseVisualStyleBackColor = true;
            // 
            // btnSetMovement
            // 
            this.btnSetMovement.Location = new System.Drawing.Point(476, 184);
            this.btnSetMovement.Name = "btnSetMovement";
            this.btnSetMovement.Size = new System.Drawing.Size(93, 23);
            this.btnSetMovement.TabIndex = 32;
            this.btnSetMovement.Text = "Set Movement";
            this.btnSetMovement.UseVisualStyleBackColor = true;
            // 
            // btnRemoveKeyframe
            // 
            this.btnRemoveKeyframe.Location = new System.Drawing.Point(80, 184);
            this.btnRemoveKeyframe.Name = "btnRemoveKeyframe";
            this.btnRemoveKeyframe.Size = new System.Drawing.Size(80, 23);
            this.btnRemoveKeyframe.TabIndex = 31;
            this.btnRemoveKeyframe.Text = "Remove Key";
            this.btnRemoveKeyframe.UseVisualStyleBackColor = true;
            // 
            // btnAddKeyFrame
            // 
            this.btnAddKeyFrame.Location = new System.Drawing.Point(6, 184);
            this.btnAddKeyFrame.Name = "btnAddKeyFrame";
            this.btnAddKeyFrame.Size = new System.Drawing.Size(68, 23);
            this.btnAddKeyFrame.TabIndex = 30;
            this.btnAddKeyFrame.Text = "Add Key";
            this.btnAddKeyFrame.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(6, 19);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(563, 159);
            this.dataGridView2.TabIndex = 26;
            // 
            // btnUpdateMotion
            // 
            this.btnUpdateMotion.Location = new System.Drawing.Point(509, 49);
            this.btnUpdateMotion.Name = "btnUpdateMotion";
            this.btnUpdateMotion.Size = new System.Drawing.Size(60, 23);
            this.btnUpdateMotion.TabIndex = 30;
            this.btnUpdateMotion.Text = "Update Motion";
            this.btnUpdateMotion.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(194, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "Origin";
            // 
            // txtOrigX
            // 
            this.txtOrigX.Location = new System.Drawing.Point(262, 25);
            this.txtOrigX.Name = "txtOrigX";
            this.txtOrigX.Size = new System.Drawing.Size(43, 20);
            this.txtOrigX.TabIndex = 35;
            // 
            // txtOrigY
            // 
            this.txtOrigY.Location = new System.Drawing.Point(311, 25);
            this.txtOrigY.Name = "txtOrigY";
            this.txtOrigY.Size = new System.Drawing.Size(43, 20);
            this.txtOrigY.TabIndex = 36;
            // 
            // txtOrigZ
            // 
            this.txtOrigZ.Location = new System.Drawing.Point(360, 25);
            this.txtOrigZ.Name = "txtOrigZ";
            this.txtOrigZ.Size = new System.Drawing.Size(43, 20);
            this.txtOrigZ.TabIndex = 37;
            // 
            // btnAnimate
            // 
            this.btnAnimate.Location = new System.Drawing.Point(138, 12);
            this.btnAnimate.Name = "btnAnimate";
            this.btnAnimate.Size = new System.Drawing.Size(74, 23);
            this.btnAnimate.TabIndex = 31;
            this.btnAnimate.Text = "Animate";
            this.btnAnimate.UseVisualStyleBackColor = true;
            // 
            // btnStopAnimation
            // 
            this.btnStopAnimation.Location = new System.Drawing.Point(218, 12);
            this.btnStopAnimation.Name = "btnStopAnimation";
            this.btnStopAnimation.Size = new System.Drawing.Size(80, 23);
            this.btnStopAnimation.TabIndex = 32;
            this.btnStopAnimation.Text = "Stop Animation";
            this.btnStopAnimation.UseVisualStyleBackColor = true;
            // 
            // MotionsToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 350);
            this.Controls.Add(this.btnStopAnimation);
            this.Controls.Add(this.btnAnimate);
            this.Controls.Add(this.grpEditKeyframes);
            this.Controls.Add(this.RemoveMotion);
            this.Controls.Add(this.AddMotion);
            this.Controls.Add(this.grpRaw);
            this.Controls.Add(this.MotionsList);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MotionsToolForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Motions (Models) Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.grpRaw.ResumeLayout(false);
            this.grpRaw.PerformLayout();
            this.grpEditKeyframes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox MotionsList;
        private System.Windows.Forms.GroupBox grpRaw;
        private System.Windows.Forms.Button btnSetOriginCenter;
        private System.Windows.Forms.TextBox txtCurrentKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMotionID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMotionName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AddMotion;
        private System.Windows.Forms.Button RemoveMotion;
        private System.Windows.Forms.GroupBox grpEditKeyframes;
        private System.Windows.Forms.Button btnSetRotation;
        private System.Windows.Forms.Button btnSetMovement;
        private System.Windows.Forms.Button btnRemoveKeyframe;
        private System.Windows.Forms.Button btnAddKeyFrame;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.Button btnUpdateMotion;
        private TextBox txtOrigX;
        private Label label4;
        private TextBox txtOrigZ;
        private TextBox txtOrigY;
        private Button btnAnimate;
        private Button btnStopAnimation;
    }
}