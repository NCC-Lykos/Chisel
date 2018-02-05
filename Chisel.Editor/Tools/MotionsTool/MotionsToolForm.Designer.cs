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
            this.txtOrigZ = new System.Windows.Forms.TextBox();
            this.txtOrigY = new System.Windows.Forms.TextBox();
            this.btnUpdateMotion = new System.Windows.Forms.Button();
            this.txtOrigX = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSetOriginCenter = new System.Windows.Forms.Button();
            this.txtCurrentKey = new System.Windows.Forms.TextBox();
            this.txtMotionID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMotionName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddMotion = new System.Windows.Forms.Button();
            this.btnRemoveMotion = new System.Windows.Forms.Button();
            this.grpEditKeyframes = new System.Windows.Forms.GroupBox();
            this.btnSetKeyFrame = new System.Windows.Forms.Button();
            this.btnRemoveKeyframe = new System.Windows.Forms.Button();
            this.btnAddKeyFrame = new System.Windows.Forms.Button();
            this.KeyFrameData = new System.Windows.Forms.DataGridView();
            this.btnAnimate = new System.Windows.Forms.Button();
            this.btnStopAnimation = new System.Windows.Forms.Button();
            this.rdoMove = new System.Windows.Forms.RadioButton();
            this.rdoRotate = new System.Windows.Forms.RadioButton();
            this.grpRaw.SuspendLayout();
            this.grpEditKeyframes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.KeyFrameData)).BeginInit();
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
            // txtOrigZ
            // 
            this.txtOrigZ.Location = new System.Drawing.Point(422, 25);
            this.txtOrigZ.Name = "txtOrigZ";
            this.txtOrigZ.Size = new System.Drawing.Size(75, 20);
            this.txtOrigZ.TabIndex = 37;
            // 
            // txtOrigY
            // 
            this.txtOrigY.Location = new System.Drawing.Point(341, 25);
            this.txtOrigY.Name = "txtOrigY";
            this.txtOrigY.Size = new System.Drawing.Size(75, 20);
            this.txtOrigY.TabIndex = 36;
            // 
            // btnUpdateMotion
            // 
            this.btnUpdateMotion.Location = new System.Drawing.Point(509, 25);
            this.btnUpdateMotion.Name = "btnUpdateMotion";
            this.btnUpdateMotion.Size = new System.Drawing.Size(60, 50);
            this.btnUpdateMotion.TabIndex = 30;
            this.btnUpdateMotion.Text = "Update Motion Data";
            this.btnUpdateMotion.UseVisualStyleBackColor = true;
            this.btnUpdateMotion.Click += new System.EventHandler(this.UpdateMotionDataClicked);
            // 
            // txtOrigX
            // 
            this.txtOrigX.Location = new System.Drawing.Point(260, 25);
            this.txtOrigX.Name = "txtOrigX";
            this.txtOrigX.Size = new System.Drawing.Size(75, 20);
            this.txtOrigX.TabIndex = 35;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(220, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "Origin";
            // 
            // btnSetOriginCenter
            // 
            this.btnSetOriginCenter.Location = new System.Drawing.Point(260, 52);
            this.btnSetOriginCenter.Name = "btnSetOriginCenter";
            this.btnSetOriginCenter.Size = new System.Drawing.Size(237, 23);
            this.btnSetOriginCenter.TabIndex = 4;
            this.btnSetOriginCenter.Text = "Set Origin to center of Solids";
            this.btnSetOriginCenter.UseVisualStyleBackColor = true;
            this.btnSetOriginCenter.Click += new System.EventHandler(this.SetOriginCenterClicked);
            // 
            // txtCurrentKey
            // 
            this.txtCurrentKey.Enabled = false;
            this.txtCurrentKey.Location = new System.Drawing.Point(166, 51);
            this.txtCurrentKey.Name = "txtCurrentKey";
            this.txtCurrentKey.Size = new System.Drawing.Size(48, 20);
            this.txtCurrentKey.TabIndex = 5;
            // 
            // txtMotionID
            // 
            this.txtMotionID.Enabled = false;
            this.txtMotionID.Location = new System.Drawing.Point(47, 51);
            this.txtMotionID.Name = "txtMotionID";
            this.txtMotionID.Size = new System.Drawing.Size(45, 20);
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(98, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Current Key";
            // 
            // txtMotionName
            // 
            this.txtMotionName.Location = new System.Drawing.Point(47, 25);
            this.txtMotionName.Name = "txtMotionName";
            this.txtMotionName.Size = new System.Drawing.Size(167, 20);
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
            // btnAddMotion
            // 
            this.btnAddMotion.Location = new System.Drawing.Point(12, 321);
            this.btnAddMotion.Name = "btnAddMotion";
            this.btnAddMotion.Size = new System.Drawing.Size(54, 23);
            this.btnAddMotion.TabIndex = 2;
            this.btnAddMotion.Text = "Add";
            this.btnAddMotion.UseVisualStyleBackColor = true;
            this.btnAddMotion.Click += new System.EventHandler(this.AddMotionClicked);
            // 
            // btnRemoveMotion
            // 
            this.btnRemoveMotion.Location = new System.Drawing.Point(72, 321);
            this.btnRemoveMotion.Name = "btnRemoveMotion";
            this.btnRemoveMotion.Size = new System.Drawing.Size(60, 23);
            this.btnRemoveMotion.TabIndex = 3;
            this.btnRemoveMotion.Text = "Remove";
            this.btnRemoveMotion.UseVisualStyleBackColor = true;
            this.btnRemoveMotion.Click += new System.EventHandler(this.RemoveMotionClicked);
            // 
            // grpEditKeyframes
            // 
            this.grpEditKeyframes.Controls.Add(this.btnSetKeyFrame);
            this.grpEditKeyframes.Controls.Add(this.btnRemoveKeyframe);
            this.grpEditKeyframes.Controls.Add(this.btnAddKeyFrame);
            this.grpEditKeyframes.Controls.Add(this.KeyFrameData);
            this.grpEditKeyframes.Location = new System.Drawing.Point(138, 41);
            this.grpEditKeyframes.Name = "grpEditKeyframes";
            this.grpEditKeyframes.Size = new System.Drawing.Size(575, 216);
            this.grpEditKeyframes.TabIndex = 29;
            this.grpEditKeyframes.TabStop = false;
            this.grpEditKeyframes.Text = "Keyframe Data";
            // 
            // btnSetKeyFrame
            // 
            this.btnSetKeyFrame.Location = new System.Drawing.Point(465, 187);
            this.btnSetKeyFrame.Name = "btnSetKeyFrame";
            this.btnSetKeyFrame.Size = new System.Drawing.Size(104, 23);
            this.btnSetKeyFrame.TabIndex = 32;
            this.btnSetKeyFrame.Text = "Set Keyframe";
            this.btnSetKeyFrame.UseVisualStyleBackColor = true;
            this.btnSetKeyFrame.Click += new System.EventHandler(this.SetKeyFrameClicked);
            // 
            // btnRemoveKeyframe
            // 
            this.btnRemoveKeyframe.Location = new System.Drawing.Point(80, 187);
            this.btnRemoveKeyframe.Name = "btnRemoveKeyframe";
            this.btnRemoveKeyframe.Size = new System.Drawing.Size(80, 23);
            this.btnRemoveKeyframe.TabIndex = 31;
            this.btnRemoveKeyframe.Text = "Remove Key";
            this.btnRemoveKeyframe.UseVisualStyleBackColor = true;
            this.btnRemoveKeyframe.Click += new System.EventHandler(this.RemoveKeyFrameClicked);
            // 
            // btnAddKeyFrame
            // 
            this.btnAddKeyFrame.Location = new System.Drawing.Point(6, 187);
            this.btnAddKeyFrame.Name = "btnAddKeyFrame";
            this.btnAddKeyFrame.Size = new System.Drawing.Size(68, 23);
            this.btnAddKeyFrame.TabIndex = 30;
            this.btnAddKeyFrame.Text = "Add Key";
            this.btnAddKeyFrame.UseVisualStyleBackColor = true;
            this.btnAddKeyFrame.Click += new System.EventHandler(this.AddKeyFrameClicked);
            // 
            // KeyFrameData
            // 
            this.KeyFrameData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.KeyFrameData.Location = new System.Drawing.Point(6, 19);
            this.KeyFrameData.Name = "KeyFrameData";
            this.KeyFrameData.ReadOnly = true;
            this.KeyFrameData.Size = new System.Drawing.Size(563, 162);
            this.KeyFrameData.TabIndex = 26;
            this.KeyFrameData.CurrentCellChanged += new System.EventHandler(this.CurrentKeyframeChanged);
            // 
            // btnAnimate
            // 
            this.btnAnimate.Enabled = false;
            this.btnAnimate.Location = new System.Drawing.Point(138, 12);
            this.btnAnimate.Name = "btnAnimate";
            this.btnAnimate.Size = new System.Drawing.Size(74, 23);
            this.btnAnimate.TabIndex = 31;
            this.btnAnimate.Text = "Animate";
            this.btnAnimate.UseVisualStyleBackColor = true;
            this.btnAnimate.Click += new System.EventHandler(this.AnimateClicked);
            // 
            // btnStopAnimation
            // 
            this.btnStopAnimation.Enabled = false;
            this.btnStopAnimation.Location = new System.Drawing.Point(347, 12);
            this.btnStopAnimation.Name = "btnStopAnimation";
            this.btnStopAnimation.Size = new System.Drawing.Size(72, 23);
            this.btnStopAnimation.TabIndex = 32;
            this.btnStopAnimation.Text = "Stop Animation";
            this.btnStopAnimation.UseVisualStyleBackColor = true;
            this.btnStopAnimation.Click += new System.EventHandler(this.StopAnimationClicked);
            // 
            // rdoMove
            // 
            this.rdoMove.AutoSize = true;
            this.rdoMove.Location = new System.Drawing.Point(218, 15);
            this.rdoMove.Name = "rdoMove";
            this.rdoMove.Size = new System.Drawing.Size(52, 17);
            this.rdoMove.TabIndex = 33;
            this.rdoMove.TabStop = true;
            this.rdoMove.Tag = "Move";
            this.rdoMove.Text = "Move";
            this.rdoMove.UseVisualStyleBackColor = true;
            this.rdoMove.CheckedChanged += new System.EventHandler(this.AnimateTypeChanged);
            // 
            // rdoRotate
            // 
            this.rdoRotate.AutoSize = true;
            this.rdoRotate.Location = new System.Drawing.Point(276, 15);
            this.rdoRotate.Name = "rdoRotate";
            this.rdoRotate.Size = new System.Drawing.Size(65, 17);
            this.rdoRotate.TabIndex = 34;
            this.rdoRotate.TabStop = true;
            this.rdoRotate.Tag = "Rotate";
            this.rdoRotate.Text = "Rotation";
            this.rdoRotate.UseVisualStyleBackColor = true;
            this.rdoRotate.CheckedChanged += new System.EventHandler(this.AnimateTypeChanged);
            // 
            // MotionsToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 350);
            this.Controls.Add(this.rdoRotate);
            this.Controls.Add(this.rdoMove);
            this.Controls.Add(this.btnStopAnimation);
            this.Controls.Add(this.btnAnimate);
            this.Controls.Add(this.grpEditKeyframes);
            this.Controls.Add(this.btnRemoveMotion);
            this.Controls.Add(this.btnAddMotion);
            this.Controls.Add(this.grpRaw);
            this.Controls.Add(this.MotionsList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MotionsToolForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Motions (Models) Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.grpRaw.ResumeLayout(false);
            this.grpRaw.PerformLayout();
            this.grpEditKeyframes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.KeyFrameData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Button btnAddMotion;
        private System.Windows.Forms.Button btnRemoveMotion;
        private System.Windows.Forms.GroupBox grpEditKeyframes;
        private System.Windows.Forms.Button btnSetKeyFrame;
        private System.Windows.Forms.Button btnRemoveKeyframe;
        private System.Windows.Forms.Button btnAddKeyFrame;
        private System.Windows.Forms.DataGridView KeyFrameData;
        private System.Windows.Forms.Button btnUpdateMotion;
        private TextBox txtOrigX;
        private Label label4;
        private TextBox txtOrigZ;
        private TextBox txtOrigY;
        private Button btnAnimate;
        private Button btnStopAnimation;
        private RadioButton rdoMove;
        private RadioButton rdoRotate;
    }
}