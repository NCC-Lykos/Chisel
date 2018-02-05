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
using Chisel.Editor.Tools.SelectTool.TransformationTools;
using Chisel.Editor.UI.ObjectProperties;

namespace Chisel.Editor.Tools.MotionsTool
{
    public partial class MotionsToolForm : UI.HotkeyForm
    {
        public delegate void ChangeAnimateTypeEventHandler(object sender, Type transformationToolType);
        public delegate void StopAnimateEventHandler(object sender, Type b);

        public event ChangeAnimateTypeEventHandler ChangeAnimateType;
        public event StopAnimateEventHandler AnimationStop;

        protected virtual void OnChangeAnimateType(Type transformationToolType)
        {
            if (ChangeAnimateType != null)
            {
                ChangeAnimateType(this, transformationToolType);
            }
        } 

        protected virtual void OnStopAnimate()
        {
            if (AnimationStop != null)
            {
                AnimationStop(this, typeof(bool));
            }
        }

        public Document _document { get; set; }
        public bool InAnimation = false;
        public List<Solid> Solids;
        public Coordinate SolidsOrigin;
        public Coordinate MotionsOrigin;
        public bool MotionSelected = false;
        public float CurrentKeyFrame;
        private bool _freeze = false;
        private TransformFlags flags;
        private Quaternion PrevRotation;
        private int CurrentMotionIndex;

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

            btnRemoveMotion.Enabled = grpEditKeyframes.Enabled = grpRaw.Enabled = false;

            foreach (DataGridViewColumn c in KeyFrameData.Columns)
            {
                c.Width = 65;
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            KeyFrameData.RowHeadersWidth = 34;
            KeyFrameData.Columns[0].Width = 55;

            PrevRotation = new Quaternion(0, 0, 0, -1);//No Rotation
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
            flags = _document.Map.GetTransformFlags() | TransformFlags.Translate | TransformFlags.Rotation;
        }

        private void Assert(bool b, string message = "Assert failed.")
        {
            if (!b) throw new Exception(message);
        }

        public void Notify(string message, object data) {}

        public void ResetSolids()
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

        private Quaternion GetKeyframeRot(bool prev = false)
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
            PrevRotation = new Quaternion((decimal)rev.X, (decimal)rev.Y,
                                                                   (decimal)rev.Z, (decimal)rev.W);
            var prev = Matrix.Rotation(PrevRotation);

            Quaternion q = GetKeyframeRot();
            
            var rot = Matrix.Rotation(q); // Do rotation
            var fin = Matrix.Translation(SolidsOrigin + GetKeyframeMov()); // Move to final origin
            
            PrevRotation = q;
            return new UnitMatrixMult(fin * (rot * prev) * mov);
        }

        private void UpdateOrigin()
        {
            _document.Selection.Select(Solids);
            Mediator.Publish(EditorMediator.SelectionChanged);
            SolidsOrigin = _document.Selection.GetSelectionBoundingBox().Center;
            if (MotionsOrigin == SolidsOrigin) btnSetOriginCenter.Enabled = false;
            else btnSetOriginCenter.Enabled = true;
        }

        private void UpdateKeyFrameList(Motion m, float Key = -1)
        {
            _freeze = true;
            KeyFrameData.Rows.Clear();

            int CurrentRow = -1;
            if (Key == -1) Key = CurrentKeyFrame;
            foreach (MotionKeyFrames k in m.KeyFrames)
            {
                var rot = k.GetRotation();
                var tra = k.GetTranslation();
                KeyFrameData.Rows.Add(k.KeyTime, tra.X, tra.Y, tra.Z, rot.X, rot.Y, rot.Z, rot.W);
                if (k.KeyTime == Key) CurrentRow += KeyFrameData.Rows.Count;
            }
            if (CurrentRow == -1) CurrentRow += KeyFrameData.Rows.Count;
            
            KeyFrameData.ClearSelection();
            KeyFrameData.CurrentCell = KeyFrameData.Rows[CurrentRow].Cells[0];
            KeyFrameData.Rows[CurrentRow].Selected = true;
            CurrentKeyFrame = (float)KeyFrameData.Rows[CurrentRow].Cells[0].Value;
            txtCurrentKey.Text = KeyFrameData.Rows[CurrentRow].Cells[0].Value.ToString();
            m.CurrentKeyTime = CurrentKeyFrame;
            _freeze = false;
        }

        private bool UpdateFields(int i)
        {
            var s = _document.Map.WorldSpawn.GetChildren().OfType<Solid>().Where(x => x.MetaData.Get<string>("ModelId") == _document.Map.Motions[i].ID.ToString()).ToList();
            if (s.Count() == 0)
            {
                MessageBox.Show("This motion has no brushes attached to it. Set brushes to this motion in the Object Editor", 
                                "No Brushes in motion",
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
                return false;
            }

            var orig = _document.Map.Motions[i].GetOrigin();
            MotionsOrigin = orig;
            txtOrigX.Text = orig.X.ToString();
            txtOrigY.Text = orig.Y.ToString();
            txtOrigZ.Text = orig.Z.ToString();

            SetSolid(s);
            
            CurrentKeyFrame = (float)_document.Map.Motions[i].CurrentKeyTime;
            txtMotionName.Text = _document.Map.Motions[i].Name.ToString();
            txtMotionID.Text = _document.Map.Motions[i].ID.ToString();
            
            UpdateKeyFrameList(_document.Map.Motions[i]);

            return true;
        }
        
        private bool Update(int i)
        {
            if (!UpdateFields(i)) return false;
            //Keyframe
            _document.PerformAction("Transform selection",
                                     new Edit(Solids,
                                     new TransformEditOperation(KeyFrameTransform(), flags)));

            foreach (Solid s in Solids) foreach (Face f in s.Faces) f.Texture.Opacity *= 0.5m;
            
            _document.RenderAll();
            return true;
        }

        private float GetMaxKeyFrame()
        {
            float MaxTime = 0;
            foreach (DataGridViewRow r in KeyFrameData.Rows) if ((float)r.Cells[0].Value > MaxTime) MaxTime = (float)r.Cells[0].Value;
            return MaxTime;
        }

        private float PromptKeyTime(float time = -1)
        {
            if(time < 0) time = GetMaxKeyFrame() + 0.01f;
            var f = new NewKeyFrame(time);
            f.ShowDialog();
            if (f.cancel) f.Time = -f.Time;
            return f.Time;
        }

        private bool KeyFrameAdd(bool prompt = true, Coordinate c = null, Quaternion q = null)
        {
            if (prompt) {
                if (c == null) c = new Coordinate(0.000000m, 0.000000m, 0.000000m);
                if (q == null) q = new Quaternion(0.000000m, 0.000000m, 0.000000m, 1.000000m);
            }
            else //if we are not prompting user has clicked animation
            {
                if (c == null) c = new Coordinate((decimal)KeyFrameData.CurrentRow.Cells[1].Value,
                                                  (decimal)KeyFrameData.CurrentRow.Cells[2].Value,
                                                  (decimal)KeyFrameData.CurrentRow.Cells[3].Value);

                if (q == null) q = new Quaternion((decimal)KeyFrameData.CurrentRow.Cells[4].Value,
                                                  (decimal)KeyFrameData.CurrentRow.Cells[5].Value,
                                                  (decimal)KeyFrameData.CurrentRow.Cells[6].Value,
                                                  (decimal)KeyFrameData.CurrentRow.Cells[7].Value);
            }
            


            float time;
            if (prompt) time = PromptKeyTime();
            else time = (float)KeyFrameData.CurrentRow.Cells[0].Value;
            
            if (time < 0) return false;

            Motion m = _document.Map.Motions[CurrentMotionIndex];
            
            List<MotionKeyFrames> KeyFrames = m.KeyFrames;
            foreach (MotionKeyFrames k in KeyFrames) if (time == k.KeyTime) time += 0.01f;
            MotionKeyFrames NewFrame = new MotionKeyFrames(time, m);

            NewFrame.SetTranslation(c);
            NewFrame.SetRotation(q);

            KeyFrames.Add(NewFrame);
            m.KeyFrames = KeyFrames.OrderBy(x => x.KeyTime).ToList();
            
            UpdateKeyFrameList(m,time);
            _document.PerformAction("Transform selection",
                                         new Edit(Solids,
                                         new TransformEditOperation(KeyFrameTransform(), flags)));
            return true;
        }

        public void KeyFrameEdit(float time = -1, Coordinate c = null, Quaternion q = null)
        {
            if (c == null) c = new Coordinate(0.000000m, 0.000000m, 0.000000m);
            if (q == null) q = new Quaternion(0.000000m, 0.000000m, 0.000000m, -1.000000m);
            if (time < 0) time = (float)KeyFrameData.CurrentRow.Cells[0].Value;

            
            Motion m = _document.Map.Motions[CurrentMotionIndex];

            List<MotionKeyFrames> KeyFrames = m.KeyFrames;
            foreach (MotionKeyFrames k in KeyFrames)
            {
                if (time == k.KeyTime)
                {
                    k.SetRotation(q);
                    k.SetTranslation(c);
                }
            }

            UpdateKeyFrameList(m, time);
            _document.PerformAction("Transform selection",
                                         new Edit(Solids,
                                         new TransformEditOperation(KeyFrameTransform(), flags)));
        }

        private void KeyFrameRemove()
        {
            Motion m = _document.Map.Motions[CurrentMotionIndex];
            List<MotionKeyFrames> KeyFrames = m.KeyFrames;
            MotionKeyFrames del = null;
            foreach(MotionKeyFrames k in KeyFrames) if (k.KeyTime == CurrentKeyFrame) del = k;
            if (del != null && KeyFrames.Count > 1) KeyFrames.Remove(del);

            UpdateKeyFrameList(m);
            _document.PerformAction("Transform selection",
                                         new Edit(Solids,
                                         new TransformEditOperation(KeyFrameTransform(), flags)));
        }

        private void MotionSelectionChanged(object sender, ItemCheckEventArgs e)
        {
            if (_freeze) return;
            bool r;
            if (e.NewValue == CheckState.Checked)
            {
                foreach (int i in MotionsList.CheckedIndices) MotionsList.SetItemCheckState(i, CheckState.Unchecked);
                r = Update(e.Index);
            }
            else r = false;

            if (!r)
            {
                //if (e.NewValue == CheckState.Checked) e.NewValue = CheckState.Unchecked;
                ResetSolids();
                ClearText();
                _freeze = true;
                KeyFrameData.Rows.Clear();
                _freeze = false;
                btnRemoveMotion.Enabled = false;
            }
            else CurrentMotionIndex = e.Index;

            if (e.NewValue == CheckState.Checked) btnRemoveMotion.Enabled = true;
            else btnRemoveMotion.Enabled = false;

            MotionSelected = btnAnimate.Enabled = r;
            grpEditKeyframes.Enabled = grpRaw.Enabled = r;
        }

        public void OnClosing(object sender, FormClosingEventArgs e)
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
            KeyFrameAdd(false);

            InAnimation = btnStopAnimation.Enabled = true;
            btnAnimate.Enabled = grpEditKeyframes.Enabled = grpRaw.Enabled = false;
            MotionsList.Enabled = btnAddMotion.Enabled = btnRemoveMotion.Enabled = false;

            OnChangeAnimateType(typeof(MoveTool));
            
        }

        private void StopAnimationClicked(object sender, EventArgs e)
        {
            InAnimation = btnStopAnimation.Enabled = false;
            btnAnimate.Enabled = grpEditKeyframes.Enabled = grpRaw.Enabled = true;
            MotionsList.Enabled = btnAddMotion.Enabled = btnRemoveMotion.Enabled = true;
            btnAnimate.Enabled = true;
            flags = 0;

            float oldtime = (float)KeyFrameData.CurrentRow.Cells[0].Value;
            float newtime = PromptKeyTime(oldtime);

            if(newtime != oldtime && !(newtime < 0))
            {
                Motion m = _document.Map.Motions[CurrentMotionIndex];
                List<MotionKeyFrames> KeyFrames = m.KeyFrames;
                foreach (MotionKeyFrames k in KeyFrames) if (k.KeyTime == oldtime) k.KeyTime = newtime;
                m.KeyFrames = KeyFrames.OrderBy(x => x.KeyTime).ToList();
                UpdateKeyFrameList(m, newtime);
                _document.PerformAction("Transform selection",
                                             new Edit(Solids,
                                             new TransformEditOperation(KeyFrameTransform(), flags)));
            }
            

            OnStopAnimate();
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

        private void AddMotionClicked(object sender, EventArgs e)
        {
            var i = _document.Map.IDGenerator.GetNextMotionID();
            var f = new NewMotion(i);
            f.ShowDialog();
            var name = f.Name;
            Motion m = new Motion(i);
            m.Name = name;
            m.SetBlank();
            _document.Map.Motions.Add(m);

            _document.Selection.Clear();
            if (name != null) Mediator.Publish(EditorMediator.SelectionChanged);

            Clear(false);
            PopulateMotions();
        }

        private void RemoveMotionClicked(object sender, EventArgs e) {
            
            _document.Map.Motions.Remove(_document.Map.Motions[MotionsList.SelectedIndex]);

            Mediator.Publish(EditorMediator.DocumentTreeStructureChanged);
            Clear(false);
            PopulateMotions();
        }

        private void AddKeyFrameClicked(object sender, EventArgs e) {
            KeyFrameAdd();
        }

        private void RemoveKeyFrameClicked(object sender, EventArgs e) {
            KeyFrameRemove();
        }
        
        private void SetKeyFrameClicked(object sender, EventArgs e) {
            var f = new KeyFrameEdit(CurrentKeyFrame);
            f.ShowDialog();
            if (f.change)
            {
                KeyFrameEdit(CurrentKeyFrame, f.c, f.q);
            }
        }

        private void SetOriginCenterClicked(object sender, EventArgs e)
        {
            MotionsOrigin = SolidsOrigin;
            
            _document.Map.Motions[CurrentMotionIndex].SetOrigin(MotionsOrigin);
            txtOrigX.Text = MotionsOrigin.X.ToString();
            txtOrigY.Text = MotionsOrigin.Y.ToString();
            txtOrigZ.Text = MotionsOrigin.Z.ToString();
            btnSetOriginCenter.Enabled = false;
        }

        private void UpdateMotionDataClicked(object s, EventArgs e)
        {
            _document.Map.Motions[CurrentMotionIndex].Name = txtMotionName.Text;
            MotionsOrigin = new Coordinate(Convert.ToDecimal(txtOrigX.Text),
                                           Convert.ToDecimal(txtOrigY.Text),
                                           Convert.ToDecimal(txtOrigZ.Text));
            _document.Map.Motions[CurrentMotionIndex].SetOrigin(MotionsOrigin);
            long CurrentMotion = _document.Map.Motions[CurrentMotionIndex].ID;
            Clear(false);
            PopulateMotions((int)CurrentMotion);
        }
        
        private void EditKeyTimeClicked(object s, EventArgs e)
        {
            float oldtime = (float)KeyFrameData.CurrentRow.Cells[0].Value;
            float newtime = PromptKeyTime(oldtime);

            if (newtime != oldtime && !(newtime < 0))
            {
                Motion m = _document.Map.Motions[CurrentMotionIndex];
                List<MotionKeyFrames> KeyFrames = m.KeyFrames;
                foreach (MotionKeyFrames k in KeyFrames) if (k.KeyTime == oldtime) k.KeyTime = newtime;
                m.KeyFrames = KeyFrames.OrderBy(x => x.KeyTime).ToList();
                UpdateKeyFrameList(m, newtime);
                _document.PerformAction("Transform selection",
                                             new Edit(Solids,
                                             new TransformEditOperation(KeyFrameTransform(), flags)));
            }
        }
    }
}
