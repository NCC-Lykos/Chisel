using System;
using System.Windows.Forms;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Settings;
using System.Linq;
using System.Collections.Generic;
using Chisel.DataStructures.Geometric;

namespace Chisel.Editor.Tools.MotionsTool
{
    public partial class MotionsToolForm : UI.HotkeyForm
    {
        public Documents.Document _document { get; set; }

        public void SetDocument(Documents.Document Document)
        {
            _document = Document;
        }

        public MotionsToolForm()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            MotionsList.Items.Clear();
            KeyFrameData.Rows.Clear();
            KeyFrameData.Columns.Clear();
            
            _document.Selection.Clear();
            _document.RenderAll();
            Mediator.Publish(EditorMediator.SelectionChanged);
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
        
        public void Notify(string message, object data) {}
        
        private void Update(int i)
        {
            _document.Selection.Clear();
            
            var objs = _document.Map.WorldSpawn.GetChildren().OfType<Solid>().Where(x => x.MetaData.Get<string>("ModelId") == _document.Map.Motions[i].ID.ToString()).ToList();

            txtMotionName.Text = _document.Map.Motions[i].Name.ToString();
            txtMotionID.Text = _document.Map.Motions[i].ID.ToString();
            txtCurrentKey.Text = _document.Map.Motions[i].CurrentKeyTime.ToString();

            var orig = _document.Map.Motions[i].GetOrigin();

            txtOrigX.Text = orig.X.ToString();
            txtOrigY.Text = orig.Y.ToString();
            txtOrigZ.Text = orig.Z.ToString();

            KeyFrameData.Rows.Clear();
            foreach (MotionKeyFrames k in _document.Map.Motions[i].KeyFrames)
            {
                KeyFrameData.Rows.Add(k.KeyTime, k.traX, k.traY, k.traZ, k.rotX, k.rotY, k.rotZ, k.rotD);
            }

            _document.Selection.Select(objs);

            _document.RenderAll();
            Mediator.Publish(EditorMediator.SelectionChanged);
        }

        private void MotionSelectionChanged(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                foreach (int i in MotionsList.CheckedIndices) MotionsList.SetItemCheckState(i, CheckState.Unchecked);
                Update(e.Index);
            }
            else KeyFrameData.Rows.Clear();
            
            
             
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Mediator.Publish(HotkeysMediator.SwitchTool, HotkeyTool.Selection);
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
