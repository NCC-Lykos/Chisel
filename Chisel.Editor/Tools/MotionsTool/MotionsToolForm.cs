using System;
using System.Windows.Forms;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Settings;
using System.Linq;
using System.Collections.Generic;

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

            _document.Selection.Clear();
            _document.RenderAll();
            Mediator.Publish(EditorMediator.SelectionChanged);
        }
        private void Populate()
        {
            List<Motion> motions = _document.Map.Motions;
            foreach(Motion m in motions)
            {
                MotionsList.Items.Add(m.Name, CheckState.Unchecked);
            }
        }
        
        public void OnShow()
        {
            Clear();
            Populate();
        }
        
        public void Notify(string message, object data) {}

        private void SelectionChanged()
        {
            int a  = 0;
            if (1 == 1) a = 1;
        }

        private void Update(int i)
        {
            _document.Selection.Clear();
            
            var objs = _document.Map.WorldSpawn.GetChildren().OfType<Solid>().Where(x => x.MetaData.Get<string>("ModelId") == _document.Map.Motions[i].ID.ToString()).ToList();

            _document.Selection.Select(objs);

            _document.RenderAll();
            Mediator.Publish(EditorMediator.SelectionChanged);
        }

        private void MotionSelectionChanged(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                foreach (int i in MotionsList.CheckedIndices) MotionsList.SetItemCheckState(i, CheckState.Unchecked);
            }
            Update(e.Index);
            
             
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
