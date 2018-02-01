using System;
using System.Drawing;
using System.Windows.Forms;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Settings;
using System.Linq;
using System.Collections.Generic;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.Transformations;
using Chisel.Editor.Documents;
using Chisel.Editor.Actions.MapObjects.Operations;
using Chisel.Editor.Actions.MapObjects.Operations.EditOperations;
using OpenTK;

namespace Chisel.Editor.Tools.MotionsTool
{
    public partial class MotionsToolForm : UI.HotkeyForm
    {
        public Documents.Document _document { get; set; }
        public bool InAnimation = false;
        public List<Solid> Solids;
        public Coordinate SolidsOrigin;
        public Coordinate MotionsOrigin;
        public bool MotionSelected = false;
        public float CurrentKeyFrame;
        private bool _loadingKeyFrame = false;
        //private TransformFlags flags;

        public void SetDocument(Document Document)
        {
            _document = Document;
        }

        public MotionsToolForm()
        {
            InitializeComponent();
            Solids = new List<Solid>();
        }

        private void ClearText()
        {
            txtCurrentKey.Text = null;
            txtMotionID.Text = null;
            txtMotionName.Text = null;
            txtOrigX.Text = null;
            txtOrigY.Text = null;
            txtOrigZ.Text = null;
        }

        public void Clear()
        {
            btnStopAnimation.Enabled = false;
            btnAnimate.Enabled = true;

            MotionsList.Items.Clear();
            
            _document.Selection.Clear();
            _document.RenderAll();
            Mediator.Publish(EditorMediator.SelectionChanged);

            Solids = null;
            SolidsOrigin = null;
            MotionsOrigin = null;
            MotionSelected = false;

            _loadingKeyFrame = true;
            KeyFrameData.Rows.Clear();
            KeyFrameData.Columns.Clear();
            _loadingKeyFrame = false;
            ClearText();
        }

        private void PopulateMotions()
        {
            List<Motion> motions = _document.Map.Motions;
            foreach(Motion m in motions) MotionsList.Items.Add(m.Name, CheckState.Unchecked);

            KeyFrameData.AllowUserToDeleteRows = false;
            KeyFrameData.AllowUserToAddRows = false;
            KeyFrameData.AllowUserToOrderColumns = false;
            KeyFrameData.AllowUserToResizeColumns = false;
            KeyFrameData.AllowUserToResizeRows = false;

            KeyFrameData.Columns.Add("Key", "Key");

            KeyFrameData.Columns.Add("TraX", "Mov X");
            KeyFrameData.Columns.Add("TraY", "Mov Y");
            KeyFrameData.Columns.Add("TraZ", "Mov Z");

            KeyFrameData.Columns.Add("RotX", "Rot X");
            KeyFrameData.Columns.Add("RotY", "Rot Y");
            KeyFrameData.Columns.Add("RotZ", "Rot Z");
            KeyFrameData.Columns.Add("RotD", "Rot D");

            foreach(DataGridViewColumn c in KeyFrameData.Columns) c.Width = 65;
        }
        
        public void OnShow()
        {
            Clear();
            PopulateMotions();
        }

        private TransformFlags GetFlags()
        {
            TransformFlags flags = _document.Map.GetTransformFlags();
            flags |= TransformFlags.Translate;
            return flags;
        }

        private void Assert(bool b, string message = "Assert failed.")
        {
            if (!b) throw new Exception(message);
        }

        public void Notify(string message, object data) {}

        private void ResetSolids()
        {
            if (Solids != null && Solids.Count != 0)
            {
                //Keyframe
                _document.PerformAction("Transform selection",
                                         new Edit(Solids,
                                         new TransformEditOperation(ResetTransform(), GetFlags())));
                foreach (Solid s in Solids) s.SetHighlights();

                _document.Selection.Clear();
                _document.RenderAll();
                Solids = null;
            }
        }

        private void SetSolid(List<Solid> s)
        {
            ResetSolids();
            Solids = s;
            if (Solids != null && Solids.Count > 0) UpdateOrigin();
        }

        private IUnitTransformation ResetTransform()
        {
            Coordinate c = _document.Selection.GetSelectionBoundingBox().Center;
            c = SolidsOrigin - c;
            
            Matrix4 mat = Matrix4.CreateTranslation((float)c.X, (float)c.Y, (float)c.Z); ;
            
            return new UnitMatrixMult(mat);
        }

        private Coordinate GetKeyframeMov()
        {
            Coordinate c = Coordinate.Zero;

            c.X = Convert.ToDecimal(KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[1].Value.ToString());
            c.Y = Convert.ToDecimal(KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[2].Value.ToString());
            c.Z = Convert.ToDecimal(KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[3].Value.ToString());

            return c;
        }

        private IUnitTransformation MotionOriginTransform()
        {
            Coordinate c = _document.Selection.GetSelectionBoundingBox().Center;
            c = (MotionsOrigin) - c;

            Matrix4 mat = Matrix4.CreateTranslation((float)c.X, (float)c.Y, (float)c.Z); ;

            return new UnitMatrixMult(mat);
        }

        private IUnitTransformation KeyFrameTransform()
        {
            Coordinate c = _document.Selection.GetSelectionBoundingBox().Center;
            c = (MotionsOrigin + GetKeyframeMov()) - c;

            Matrix4 mat = Matrix4.CreateTranslation((float)c.X, (float)c.Y, (float)c.Z); ;

            return new UnitMatrixMult(mat);
        }
        
        private void UpdateFields(int i)
        {
            SetSolid(_document.Map.WorldSpawn.GetChildren().OfType<Solid>().Where(x => x.MetaData.Get<string>("ModelId") == _document.Map.Motions[i].ID.ToString()).ToList());
            CurrentKeyFrame = (float)_document.Map.Motions[i].CurrentKeyTime;
            txtMotionName.Text = _document.Map.Motions[i].Name.ToString();
            txtMotionID.Text = _document.Map.Motions[i].ID.ToString();
            txtCurrentKey.Text = CurrentKeyFrame.ToString();

            var orig = _document.Map.Motions[i].GetOrigin();
            MotionsOrigin = orig;
            txtOrigX.Text = orig.X.ToString();
            txtOrigY.Text = orig.Y.ToString();
            txtOrigZ.Text = orig.Z.ToString();
            
            _loadingKeyFrame = true;
            KeyFrameData.Rows.Clear();

            int CurrentRow = 0;
            foreach (MotionKeyFrames k in _document.Map.Motions[i].KeyFrames)
            {
                KeyFrameData.Rows.Add(k.KeyTime, k.traX, k.traY, k.traZ, k.rotX, k.rotY, k.rotZ, k.rotD);
                if (k.KeyTime == CurrentKeyFrame) CurrentRow = KeyFrameData.Rows.Count - 1;
            }
            KeyFrameData.ClearSelection();
            KeyFrameData.CurrentCell = KeyFrameData.Rows[CurrentRow].Cells[0];
            KeyFrameData.Rows[CurrentRow].Selected = true;
            _loadingKeyFrame = false;
        }

        private void UpdateOrigin()
        {
            _document.Selection.Select(Solids);
            Mediator.Publish(EditorMediator.SelectionChanged);
            SolidsOrigin = _document.Selection.GetSelectionBoundingBox().Center;
        }

        private void Update(int i)
        {
            UpdateFields(i);
            //Motion Origin
            _document.PerformAction("Transform selection",
                                     new Edit(Solids,
                                     new TransformEditOperation(MotionOriginTransform(), GetFlags())));
            //Keyframe
            _document.PerformAction("Transform selection",
                                     new Edit(Solids,
                                     new TransformEditOperation(KeyFrameTransform(), GetFlags())));

            foreach (Solid s in Solids) foreach (Face f in s.Faces) f.Texture.Opacity *= 0.5m;
            
            _document.RenderAll();
        }

        private void MotionSelectionChanged(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                foreach (int i in MotionsList.CheckedIndices) MotionsList.SetItemCheckState(i, CheckState.Unchecked);
                Update(e.Index);
                MotionSelected = true;
            }
            else
            {
                ResetSolids();
                
                ClearText();
                _loadingKeyFrame = true;
                KeyFrameData.Rows.Clear();
                _loadingKeyFrame = false;
                MotionSelected = false;
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (_document != null) ResetSolids();
                Clear();
                e.Cancel = true;
                Mediator.Publish(HotkeysMediator.SwitchTool, HotkeyTool.Selection);
            }
        }
        
        private void AnimateClicked(object sender, EventArgs e)
        {
            InAnimation = true;
            btnStopAnimation.Enabled = true;
            btnAnimate.Enabled = false;
        }

        private void CurrentKeyframeChanged(object sender,EventArgs e)
        {
            if (_loadingKeyFrame) return;
            CurrentKeyFrame = (float)KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[0].Value;
            if (Solids != null)
            {
                _document.PerformAction("Transform selection",
                                         new Edit(Solids,
                                         new TransformEditOperation(KeyFrameTransform(), GetFlags())));
            }
        }
    }
}
