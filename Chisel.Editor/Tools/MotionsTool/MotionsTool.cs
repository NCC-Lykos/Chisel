using System.Drawing;
using Chisel.Editor.Properties;
using Chisel.Settings;
using Chisel.UI;
using Chisel.Common.Mediator;

namespace Chisel.Editor.Tools.MotionsTool
{
    class MotionsTool : BaseTool
    {
        private readonly MotionsToolForm _form;
        //private readonly TextureToolSidebarPanel _sidebarPanel;

        public MotionsTool()
        {
            Usage = ToolUsage.View3D;
            _form = new MotionsToolForm();
            //_form.PropertyChanged += TexturePropertyChanged;
            //_form.TextureAlign += TextureAligned;
            //_form.TextureApply += TextureApplied;
            //_form.TextureJustify += TextureJustified;
            //_form.HideMaskToggled += HideMaskToggled;
            //_form.TextureChanged += TextureChanged;

            //_sidebarPanel = new TextureToolSidebarPanel();
            //_sidebarPanel.TileFit += TileFit;
            //_sidebarPanel.RandomiseXShiftValues += RandomiseXShiftValues;
            //_sidebarPanel.RandomiseYShiftValues += RandomiseYShiftValues;
        }

        public override void ToolSelected(bool preventHistory)
        {
            _form.Show(Editor.Instance);
            Editor.Instance.Focus();
            
            _form.OnShow();
        }

        public override void ToolDeselected(bool preventHistory)
        {
            _form.Clear();
            _form.Hide();
        }


        public override void DocumentChanged()
        {
            _form.SetDocument(Document);
        }

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

        public override void MouseDown(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void KeyDown(ViewportBase viewport, ViewportEvent e)
        {
            //throw new NotImplementedException();
        }

        public override void Render(ViewportBase viewport)
        {
            //
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

        public override void MouseEnter(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void MouseLeave(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void MouseClick(ViewportBase viewport, ViewportEvent e)
        {
            // Not used
        }

        public override void MouseDoubleClick(ViewportBase viewport, ViewportEvent e)
        {
            // Not used
        }

        public override void MouseUp(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void MouseWheel(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void MouseMove(ViewportBase viewport, ViewportEvent e)
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

        public override void UpdateFrame(ViewportBase viewport, FrameInfo frame)
        {
            //
        }
    }
}
