using System.Drawing;
using Chisel.Editor.Properties;
using Chisel.Settings;
using Chisel.UI;
using Chisel.Common.Mediator;
using Chisel.Editor.Documents;
using Chisel.Editor.History;
using Chisel.Editor.Actions.MapObjects.Selection;
using System.Linq;


namespace Chisel.Editor.Tools.MotionsTool
{
    class MotionsTool : BaseBoxTool
    {
        private readonly MotionsToolForm _form;
        //private readonly TextureToolSidebarPanel _sidebarPanel;

        public MotionsTool()
        {
            Usage = ToolUsage.Both;
            _form = new MotionsToolForm();
        }

        public override void ToolSelected(bool preventHistory)
        {
            _form.Show(Editor.Instance);
            Editor.Instance.Focus();
            
            _form.OnShow();
        }
        
        public override void ToolDeselected(bool preventHistory)
        {
            var selected = Document.Selection.GetSelectedFaces().ToList();

            if (!preventHistory)
            {
                Document.History.AddHistoryItem(new HistoryAction("Switch selection mode", new ChangeToObjectSelectionMode(GetType(), selected)));
                var currentSelection = Document.Selection.GetSelectedFaces().Select(x => x.Parent);
                Document.Selection.SwitchToObjectSelection();
                var newSelection = Document.Selection.GetSelectedObjects();
                Document.RenderSelection(currentSelection.Union(newSelection));
            }

            _form.Clear();
            _form.Hide();
            Mediator.UnsubscribeAll(this);
        }
        
        public override void DocumentChanged()
        {
            _form.SetDocument(Document);
        }

        protected override void Render3D(Viewport3D viewport)
        {
            base.Render3D(viewport);
        }

        protected override void Render2D(Viewport2D viewport)
        {
            if(!_form.MotionSelected)
            {
                base.Render2D(viewport);
                return;
            }
            var box = Document.Selection.GetSelectionBoundingBox();
            State.BoxStart = box.Start;
            State.BoxEnd = box.End;

            var start = viewport.Flatten(State.BoxStart);
            var end = viewport.Flatten(State.BoxEnd);
            
            if (true)
            {
                RenderResizeBox(viewport, start, end);
            }
        }

        #region inherit
        
        public override Image GetIcon()
        {
            return Resources.Menu_ObjectProperties;
        }

        public override string GetName()
        {
            return "Motions Tool";
        }

        public override HotkeyTool? GetHotkeyToolType()
        {
            return HotkeyTool.Motions;
        }

        public override string GetContextualHelp()
        {
            return "";
        }

        public override void MouseMove(ViewportBase viewport, ViewportEvent e)
        {
            base.MouseMove(viewport, e);
        }

        public override void MouseDown(ViewportBase viewport, ViewportEvent e)
        {
            base.MouseDown(viewport, e);
        }

        public override void MouseUp(ViewportBase viewport, ViewportEvent e)
        {
            base.MouseUp(viewport, e);
        }

        public override void MouseClick(ViewportBase viewport, ViewportEvent e)
        {
            base.MouseClick(viewport, e);
        }

        public override void MouseEnter(ViewportBase viewport, ViewportEvent e)
        {
            base.MouseEnter(viewport, e);
        }

        public override void MouseLeave(ViewportBase viewport, ViewportEvent e)
        {
            base.MouseLeave(viewport, e);
        }

        public override void PreRender(ViewportBase viewport)
        {
            base.PreRender(viewport);
        }

        public override void Render(ViewportBase viewport)
        {
            base.Render(viewport);
        }

        public override void UpdateFrame(ViewportBase viewport, FrameInfo frame)
        {
            base.UpdateFrame(viewport, frame);
        }
        
        public override void KeyDown(ViewportBase viewport, ViewportEvent e)
        {
            //throw new NotImplementedException();
        }
        
        public override HotkeyInterceptResult InterceptHotkey(HotkeysMediator hotkeyMessage, object parameters)
        {
            switch (hotkeyMessage)
            {
                case HotkeysMediator.OperationsCopy:
                case HotkeysMediator.OperationsCut:
                case HotkeysMediator.OperationsPaste:
                case HotkeysMediator.OperationsPasteSpecial:
                case HotkeysMediator.OperationsDelete:
                case HotkeysMediator.Transform:
                    return HotkeyInterceptResult.Abort;
            }
            return HotkeyInterceptResult.Continue;
        }
        
        public override void MouseDoubleClick(ViewportBase viewport, ViewportEvent e)
        {
            // Not used
        }
        
        public override void MouseWheel(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void KeyPress(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void KeyUp(ViewportBase viewport, ViewportEvent e)
        {
            //
        }
        
        protected override Color BoxColour
        {
            get { return Color.Green; }
        }

        protected override Color FillColour
        {
            get { return Color.FromArgb(Chisel.Settings.View.SelectionBoxBackgroundOpacity, Color.ForestGreen); }
        }

        

        #endregion
    }
}
