using System;
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
        public Document _document { get; set; }
        public bool InAnimation = false;
        public List<Solid> Solids;
        public Coordinate SolidsOrigin;
        public Coordinate MotionsOrigin;
        public bool MotionSelected = false;
        public float CurrentKeyFrame;
        private bool _freeze = false;
        private TransformFlags flags;
        private DataStructures.Geometric.Quaternion PrevRotation;

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
            rdoMove.Checked = rdoRotate.Checked = false;
            rdoMove.Enabled = rdoRotate.Enabled = false;
            btnAnimate.Enabled = btnStopAnimation.Enabled = false;
            flags = 0;
        }

        public void Clear(bool KeepSelection = false)
        {
            MotionsList.Items.Clear();

            if (!KeepSelection)
            { 
                _document.Selection.Clear();
                _document.RenderAll();
                Mediator.Publish(EditorMediator.SelectionChanged);
            }
            
            Solids = null;
            SolidsOrigin = null;
            MotionsOrigin = null;
            MotionSelected = false;
            PrevRotation = null;

            _freeze = true;
            KeyFrameData.Rows.Clear();
            KeyFrameData.Columns.Clear();
            _freeze = false;
            ClearText();
        }

        private void PopulateMotions(int MotionID = 0)
        {
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
            KeyFrameData.Columns.Add("RotW", "Rot W");

            foreach (DataGridViewColumn c in KeyFrameData.Columns) c.Width = 65;

            PrevRotation = new DataStructures.Geometric.Quaternion(0, 0, 0, -1);//No Rotation
            List<Motion> motions = _document.Map.Motions;
            foreach (Motion m in motions)
            {
                CheckState chk = CheckState.Unchecked;
                if (m.ID == MotionID) chk = CheckState.Checked;
                MotionsList.Items.Add(m.Name, chk);
            }
        }
        
        private int SelectionMotionID()
        {
            int result = 0;
            var Objects = _document.Selection.GetSelectedObjects();

            //TODO: allow if multiple faces are selected and all are the same objectID to select object
            if (!Objects.All(x => x is Solid) || Objects.Count() != 1) return result;
            foreach(Solid s in Objects) result = Convert.ToInt32(s.MetaData.Get<string>("ModelId"));
            
            return result;
        }
        
        public void OnShow()
        {
            var MotionID = SelectionMotionID();
            Clear(MotionID != 0);
            PopulateMotions(MotionID);
        }

        private void SetTransformFlags()
        {
            if (rdoMove.Checked)
            {
                flags = _document.Map.GetTransformFlags();
                flags |= TransformFlags.Translate;
            }
            else
            {
                flags = _document.Map.GetTransformFlags();
                flags |= TransformFlags.Rotation;
            }
            
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
                                         new TransformEditOperation(ResetTransform(), flags)));
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
            var mov = Matrix.Translation(-_document.Selection.GetSelectionBoundingBox().Center);
            

            OpenTK.Quaternion rev = new OpenTK.Quaternion((float)PrevRotation.X,
                                                          (float)PrevRotation.Y,
                                                          (float)PrevRotation.Z,
                                                          (float)PrevRotation.W);
            rev.Invert();
            var q = new DataStructures.Geometric.Quaternion(0, 0, 0, -1);
            var rot = Matrix.Rotation(q);
            var fin = Matrix.Translation(SolidsOrigin);
            PrevRotation = new DataStructures.Geometric.Quaternion((decimal)rev.X, (decimal)rev.Y,
                                                                   (decimal)rev.Z, (decimal)rev.W);
            var prev = Matrix.Rotation(PrevRotation);
            PrevRotation = q;
            return new UnitMatrixMult(fin * (rot * prev) * mov);
        }

        private Coordinate GetKeyframeMov()
        {
            Coordinate c = Coordinate.Zero;

            c.X = Convert.ToDecimal(KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[1].Value.ToString());
            c.Y = Convert.ToDecimal(KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[2].Value.ToString());
            c.Z = Convert.ToDecimal(KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[3].Value.ToString());

            return c;
        }

        private DataStructures.Geometric.Quaternion GetKeyframeRot(bool prev = false)
        {
            int index = KeyFrameData.CurrentCell.RowIndex;
            if (prev) index -= 1;
            var x = Convert.ToDecimal(KeyFrameData.Rows[index].Cells[4].Value.ToString());
            var y = Convert.ToDecimal(KeyFrameData.Rows[index].Cells[5].Value.ToString());
            var z = Convert.ToDecimal(KeyFrameData.Rows[index].Cells[6].Value.ToString());
            var w = Convert.ToDecimal(KeyFrameData.Rows[index].Cells[7].Value.ToString());

            DataStructures.Geometric.Quaternion q = new DataStructures.Geometric.Quaternion(x,y,z,w);
            
            return q;
        }
        
        private IUnitTransformation KeyFrameTransform()
        {
            var mov = Matrix.Translation(-_document.Selection.GetSelectionBoundingBox().Center); // Move to zero
            OpenTK.Quaternion rev = new OpenTK.Quaternion((float)PrevRotation.X, 
                                                          (float)PrevRotation.Y,
                                                          (float)PrevRotation.Z,
                                                          (float)PrevRotation.W);
            rev.Invert();
            PrevRotation = new DataStructures.Geometric.Quaternion((decimal)rev.X, (decimal)rev.Y,
                                                                   (decimal)rev.Z, (decimal)rev.W);
            var prev = Matrix.Rotation(PrevRotation);

            DataStructures.Geometric.Quaternion q = GetKeyframeRot();
            
            var rot = Matrix.Rotation(q); // Do rotation
            var fin = Matrix.Translation(SolidsOrigin + GetKeyframeMov()); // Move to final origin
            
            PrevRotation = q;
            return new UnitMatrixMult(fin * (rot * prev) * mov);
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
            
            _freeze = true;
            KeyFrameData.Rows.Clear();

            int CurrentRow = 0;
            foreach (MotionKeyFrames k in _document.Map.Motions[i].KeyFrames)
            {
                KeyFrameData.Rows.Add(k.KeyTime, k.traX, k.traY, k.traZ, k.rotX, k.rotY, k.rotZ, k.rotW);
                if (k.KeyTime == CurrentKeyFrame) CurrentRow = KeyFrameData.Rows.Count - 1;
            }
            KeyFrameData.ClearSelection();
            KeyFrameData.CurrentCell = KeyFrameData.Rows[CurrentRow].Cells[0];
            KeyFrameData.Rows[CurrentRow].Selected = true;
            _freeze = false;
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
            //Keyframe
            _document.PerformAction("Transform selection",
                                     new Edit(Solids,
                                     new TransformEditOperation(KeyFrameTransform(), flags)));

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
                btnAnimate.Enabled = true;
            }
            else
            {
                ResetSolids();
                
                ClearText();
                _freeze = true;
                KeyFrameData.Rows.Clear();
                _freeze = false;
                MotionSelected = false;
                btnAnimate.Enabled = false;
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
            InAnimation = btnStopAnimation.Enabled = rdoMove.Enabled = rdoRotate.Enabled = true;
            btnAnimate.Enabled = false;
            rdoMove.Checked = true;
            SetTransformFlags();
        }

        private void StopAnimationClicked(object sender, EventArgs e)
        {
            InAnimation = btnStopAnimation.Enabled = rdoMove.Enabled = rdoRotate.Enabled = false;
            rdoMove.Checked = rdoRotate.Checked = false;
            btnAnimate.Enabled = true;
            flags = 0;
        }

        private void CurrentKeyframeChanged(object sender,EventArgs e)
        {
            if (_freeze) return;
            CurrentKeyFrame = (float)KeyFrameData.Rows[KeyFrameData.CurrentCell.RowIndex].Cells[0].Value;
            txtCurrentKey.Text = CurrentKeyFrame.ToString();
            if (Solids != null)
            {
                _document.PerformAction("Transform selection",
                                         new Edit(Solids,
                                         new TransformEditOperation(KeyFrameTransform(), flags)));
            }
        }
    }
}
