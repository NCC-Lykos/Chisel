using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Chisel.Editor.Properties;
using Chisel.Editor.Actions;
using Chisel.Editor.Clipboard;
using Chisel.Editor.Documents;
using Chisel.Editor.History;
using Chisel.Editor.Actions.MapObjects.Selection;
using Chisel.Editor.Actions.MapObjects.Operations;
using Chisel.Editor.Actions.MapObjects.Operations.EditOperations;
using Chisel.Editor.Tools.SelectTool.TransformationTools;
using Chisel.Editor.Tools.Widgets;
using Chisel.Settings;
using Chisel.UI;
using Chisel.Graphics;
using Chisel.Common.Mediator;
using Chisel.DataStructures.Transformations;
using Chisel.DataStructures.MapObjects;
using Chisel.DataStructures.Geometric;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Chisel.Editor.Tools.MotionsTool
{
    class MotionsTool : BaseBoxTool
    {
        private readonly MotionsToolForm _form;
        private readonly List<TransformationTool> _tools;
        private TransformationTool _lastTool;
        private TransformationTool _currentTool;
        private List<Widget> _widgets;
        private Matrix4? CurrentTransform { get; set; }

        private Coordinate TotalTranslation { get; set; }
        private OpenTK.Quaternion TotalRotation { get; set; }
        //private readonly TextureToolSidebarPanel _sidebarPanel;

        public MotionsTool()
        {
            Usage = ToolUsage.Both;
            _tools = new List<TransformationTool>
                         {
                             new MoveTool(),
                             new RotateTool()
                         };

            _form = new MotionsToolForm();

            _form.ChangeAnimateType += (sender, type) =>
            {
                var tool = _tools.FirstOrDefault(x => x.GetType() == type);
                SetCurrentTool(tool);
                SelectionChanged();
            };
            _form.AnimationStop += (sender, b) =>
            {
                ResetState();
            };
        }

        public override void ToolSelected(bool preventHistory)
        {
            _form.Show(Editor.Instance);
            Editor.Instance.Focus();

            TotalTranslation = Coordinate.Zero;
            TotalRotation = new OpenTK.Quaternion(0, 0, 0, 1);
            _currentTool = null;
            _lastTool = null;

            _form.OnShow();
        }

        private void ResetState()
        {
            _currentTool = null;
            _lastTool = null;
            State.Action = BoxAction.ReadyToDraw;
            State.BoxStart = null;
            State.BoxEnd = null;
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

            ResetState();
            _form.Clear();
            _form.Hide();
            Mediator.UnsubscribeAll(this);
        }
        
        public override void DocumentChanged()
        {
            _form.SetDocument(Document);
        }

        private void SelectionChanged()
        {
            if (Document == null) return;
            UpdateBoxBasedOnSelection();
            if (State.Action != BoxAction.ReadyToResize && _currentTool != null) SetCurrentTool(null);
            else if (State.Action == BoxAction.ReadyToResize && _currentTool == null) SetCurrentTool(_lastTool ?? _tools[0]);

            foreach (var widget in _widgets) widget.SelectionChanged();
        }
        
        private void UpdateBoxBasedOnSelection()
        {
            if (Document.Selection.IsEmpty())
            {
                State.BoxStart = State.BoxEnd = null;
                State.Action = BoxAction.ReadyToDraw;
            }
            else
            {
                State.Action = BoxAction.ReadyToResize;
                var box = Document.Selection.GetSelectionBoundingBox();
                State.BoxStart = box.Start;
                State.BoxEnd = box.End;
            }
            OnBoxChanged();
        }

        #region Widget
        private void SetCurrentTool(TransformationTool t)
        {
            if (t != null) _lastTool = t;
            _currentTool = t;
            _widgets = (_currentTool == null || !Chisel.Settings.Select.Show3DSelectionWidgets) ? new List<Widget>() : _currentTool.GetWidgets(Document).ToList();
            foreach (var widget in _widgets)
            {
                widget.OnTransforming = OnWidgetTransforming;
                widget.OnTransformed = OnWidgetTransformed;
                widget.SelectionChanged();
            }
        }

        private void OnWidgetTransformed(Matrix4? transformation)
        {
            if (transformation.HasValue)
            {
                //ExecuteTransform("Manipulate", CreateMatrixMultTransformation(transformation.Value), false);
            }

            Document.EndSelectionTransform();
        }

        private void OnWidgetTransforming(Matrix4? transformation)
        {
            if (transformation.HasValue) Document.SetSelectListTransform(transformation.Value);
        }

        #endregion Widget

        #region Transform
        
        /*
        private void ExecuteTransform(string transformationName, IUnitTransformation transform, bool clone)
        {
            if (clone) transformationName += "-clone";
            var objects = Document.Selection.GetSelectedParents().ToList();
            var name = String.Format("{0} {1} object{2}", transformationName, objects.Count, (objects.Count == 1 ? "" : "s"));

            var cad = new CreateEditDelete();
            var action = new ActionCollection(cad);

            if (clone)
            {
                // Copy the selection, transform it, and reselect
                var copies = ClipboardManager.CloneFlatHeirarchy(Document, Document.Selection.GetSelectedObjects()).ToList();
                foreach (var mo in copies)
                {
                    mo.Transform(transform, Document.Map.GetTransformFlags());
                    if (Chisel.Settings.Select.KeepVisgroupsWhenCloning) continue;
                    foreach (var o in mo.FindAll()) o.Visgroups.Clear();
                }
                cad.Create(Document.Map.WorldSpawn.ID, copies);
                var sel = new ChangeSelection(copies.SelectMany(x => x.FindAll()), Document.Selection.GetSelectedObjects());
                action.Add(sel);
            }
            else
            {
                TransformFlags Flags = Document.Map.GetTransformFlags();

                cad.Edit(objects, new TransformEditOperation(transform, Flags));
            }

            // Execute the action
            Document.PerformAction(name, action);
        }
        */

        private IUnitTransformation CreateMatrixMultTransformation(Matrix4 mat)
        {
            return new UnitMatrixMult(mat);
        }

        #endregion

        #region Render
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

        protected override void Render3D(Viewport3D viewport)
        {
            base.Render3D(viewport);
        }

        private void RenderHandles(Viewport2D viewport, Coordinate start, Coordinate end)
        {
            if (_currentTool == null) return;
            var circles = _currentTool.RenderCircleHandles;

            // Get the filtered list of handles, and convert them to vector locations
            var z = (double)viewport.Zoom;
            var handles = _currentTool.GetHandles(start, end, viewport.Zoom)
                .Where(x => _currentTool.FilterHandle(x.Item1))
                .Select(x => new Vector2d((double)x.Item2, (double)x.Item3))
                .ToList();

            // Draw the insides of the handles in white
            GL.Color3(Color.White);
            foreach (var handle in handles)
            {
                GL.Begin(BeginMode.Polygon);
                if (circles) GLX.Circle(handle, 4, z, loop: true);
                else GLX.Square(handle, 4, z, true);
                GL.End();
            }

            // Draw the borders of the handles in black
            GL.Color3(Color.Black);
            GL.Begin(BeginMode.Lines);
            foreach (var handle in handles)
            {
                if (circles) GLX.Circle(handle, 4, z);
                else GLX.Square(handle, 4, z);
            }
            GL.End();
        }

        private void RenderTransformBox(Viewport2D viewport)
        {
            if (!CurrentTransform.HasValue) return;

            var box = new Box(State.PreTransformBoxStart, State.PreTransformBoxEnd);
            var trans = CreateMatrixMultTransformation(CurrentTransform.Value);
            box = box.Transform(trans);
            var s = viewport.Flatten(box.Start);
            var e = viewport.Flatten(box.End);

            GL.Enable(EnableCap.LineStipple);
            GL.LineStipple(10, 0xAAAA);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Color.FromArgb(64, BoxColour));

            Coord(s.DX, s.DY, e.DZ);
            Coord(e.DX, s.DY, e.DZ);

            Coord(s.DX, e.DY, e.DZ);
            Coord(e.DX, e.DY, e.DZ);

            Coord(s.DX, s.DY, e.DZ);
            Coord(s.DX, e.DY, e.DZ);

            Coord(e.DX, s.DY, e.DZ);
            Coord(e.DX, e.DY, e.DZ);

            GL.End();
            GL.Disable(EnableCap.LineStipple);

            RenderBoxText(viewport, s, e);
        }

        protected override void Render2D(Viewport2D viewport)
        {
            if (_currentTool == null)
            {
                base.Render2D(viewport);
                return;
            }

            var box = Document.Selection.GetSelectionBoundingBox();
            State.BoxStart = box.Start;
            State.BoxEnd = box.End;

            var start = viewport.Flatten(State.BoxStart);
            var end = viewport.Flatten(State.BoxEnd);

            if (ShouldDrawBox(viewport)) RenderBox(viewport, start, end);

            if (ShouldRenderSnapHandle(viewport)) RenderSnapHandle(viewport);

            if (ShouldRenderResizeBox(viewport))
            {
                RenderResizeBox(viewport, start, end);
            }

            if(_currentTool != _tools.FirstOrDefault(x => x.GetType() == typeof(MoveTool))) RenderHandles(viewport, start, end);

            if (State.Action == BoxAction.Resizing && CurrentTransform.HasValue)
            {
                RenderTransformBox(viewport);
            }
            else if (ShouldDrawBox(viewport))
            {
                RenderBoxText(viewport, start, end);
            }
            return;
        }

        protected override Color BoxColour
        {
            get { return Color.Orange; }
        }

        protected override Color FillColour
        {
            get { return Color.FromArgb(Chisel.Settings.View.SelectionBoxBackgroundOpacity, Color.Orange); }
        }
        #endregion Render

        #region 3Dinteraction

        protected override void MouseMove3D(Viewport3D viewport, ViewportEvent e)
        {
            base.MouseMove3D(viewport, e);
        }
        
        protected override void MouseDown3D(Viewport3D viewport, ViewportEvent e)
        {
            // Do not perform selection
            return;
        }
        
        protected override void MouseUp3D(Viewport3D viewport, ViewportEvent e)
        {
            return;
        }
        
        public override void MouseWheel(ViewportBase viewport, ViewportEvent e)
        {
            return;
        }
        
        public override bool IsCapturingMouseWheel()
        {
            return false;
        }

        #endregion 3Dinteraction

        #region 2Dinteraction

        protected override Cursor CursorForHandle(ResizeHandle handle)
        {
            var def = base.CursorForHandle(handle);
            return _currentTool == null || handle == ResizeHandle.Center
                       ? def
                       : _currentTool.CursorForHandle(handle) ?? def;
        }
        
        protected override void MouseHoverWhenDrawn(Viewport2D viewport, ViewportEvent e)
        {
            if (_currentTool == null)
            {
                base.MouseHoverWhenDrawn(viewport, e);
                return;
            }

            var padding = 7 / viewport.Zoom;

            viewport.Cursor = Cursors.Default;
            State.Action = BoxAction.Drawn;
            State.ActiveViewport = null;

            var now = viewport.ScreenToWorld(e.X, viewport.Height - e.Y);
            var start = viewport.Flatten(State.BoxStart);
            var end = viewport.Flatten(State.BoxEnd);

            var ccs = new Coordinate(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), 0);
            var cce = new Coordinate(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), 0);

            // Check center handle
            if (now.X > ccs.X && now.X < cce.X && now.Y > ccs.Y && now.Y < cce.Y)
            {
                State.Handle = ResizeHandle.Center;
                State.ActiveViewport = viewport;
                State.Action = BoxAction.ReadyToResize;
                viewport.Cursor = CursorForHandle(State.Handle);
                return;
            }

            // Check other handles
            foreach (var handle in _currentTool.GetHandles(start, end, viewport.Zoom).Where(x => _currentTool.FilterHandle(x.Item1)))
            {
                var x = handle.Item2;
                var y = handle.Item3;
                if (now.X < x - padding || now.X > x + padding || now.Y < y - padding || now.Y > y + padding) continue;
                State.Handle = handle.Item1;
                State.ActiveViewport = viewport;
                State.Action = BoxAction.ReadyToResize;
                viewport.Cursor = CursorForHandle(State.Handle);
                return;
            }
        }
        
        protected override void LeftMouseDownToDraw(Viewport2D viewport, ViewportEvent e)
        {
            return;
        }

        private MapObject SelectionTest(Viewport2D viewport, ViewportEvent e)
        {
            // Create a box to represent the click, with a tolerance level
            var unused = viewport.GetUnusedCoordinate(new Coordinate(100000, 100000, 100000));
            var tolerance = 4 / viewport.Zoom; // Selection tolerance of four pixels
            var used = viewport.Expand(new Coordinate(tolerance, tolerance, 0));
            var add = used + unused;
            var click = viewport.Expand(viewport.ScreenToWorld(e.X, viewport.Height - e.Y));
            var box = new Box(click - add, click + add);

            var centerHandles = Chisel.Settings.Select.DrawCenterHandles;
            var centerOnly = Chisel.Settings.Select.ClickSelectByCenterHandlesOnly;
            // Get the first element that intersects with the box, selecting or deselecting as needed
            return Document.Map.WorldSpawn.GetAllNodesIntersecting2DLineTest(box, centerHandles, centerOnly).FirstOrDefault();
        }
        
        protected override void LeftMouseClick(Viewport2D viewport, ViewportEvent e)
        {
            base.LeftMouseClick(viewport, e);
        }

        protected override void LeftMouseClickOnResizeHandle(Viewport2D viewport, ViewportEvent e)
        {
            base.LeftMouseClickOnResizeHandle(viewport, e);

            if (_currentTool == null) return;

            // Cycle through active tools
            var idx = _tools.IndexOf(_currentTool);
            SetCurrentTool(_tools[(idx + 1) % _tools.Count]);
        }
        
        private Matrix4? GetTransformMatrix(Viewport2D viewport, ViewportEvent e)
        {
            if (_currentTool == null) return null;

            Matrix4? ret = null;

            if (State.Handle == ResizeHandle.Center) ret = _tools.OfType<MoveTool>().First().GetTransformationMatrix(viewport, e, State, Document, _widgets);
            else ret = _currentTool.GetTransformationMatrix(viewport, e, State, Document, _widgets);
            
            return ret;
        }

        protected override void LeftMouseUpDrawing(Viewport2D viewport, ViewportEvent e)
        {
            base.LeftMouseUpDrawing(viewport, e);
            if (Chisel.Settings.Select.AutoSelectBox)
            {
                BoxDrawnConfirm(viewport);
            }
        }

        private DataStructures.Geometric.Quaternion ToGeometric(OpenTK.Quaternion q)
        {
            DataStructures.Geometric.Quaternion ret;

            ret = new DataStructures.Geometric.Quaternion((decimal)q.X, (decimal)q.Y,
                                                          (decimal)q.Z, (decimal)q.W);

            return ret;
        }

        //No longer previewing
        protected override void LeftMouseUpResizing(Viewport2D viewport, ViewportEvent e)
        {
            if (_currentTool == null)
            {
                base.LeftMouseUpResizing(viewport, e);
                return;
            }
            var transformation = GetTransformMatrix(viewport, e);
            if (transformation.HasValue)
            {
                Matrix4 m = (Matrix4)transformation;
                Vector3 c = m.ExtractTranslation();
                OpenTK.Quaternion q = m.ExtractRotation();
                if (_currentTool.GetType() == typeof(MoveTool))
                {
                    TotalTranslation += new Coordinate((decimal)c.X, (decimal)c.Y, (decimal)c.Z);
                }
                else if (_currentTool.GetType() == typeof(RotateTool))
                {
                    q.Invert();
                    TotalRotation *= q;
                }
                _form.KeyFrameEdit(-1,TotalTranslation, ToGeometric(TotalRotation));

                //var createClone = KeyboardState.Shift && State.Handle == ResizeHandle.Center;
                //ExecuteTransform(_currentTool.GetTransformName(), CreateMatrixMultTransformation(transformation.Value), createClone);

            }
            Document.EndSelectionTransform();
            State.ActiveViewport = null;
            State.Action = BoxAction.Drawn;

            SelectionChanged();
        }

        protected override Coordinate GetResizeOrigin(Viewport2D viewport)
        {
            if (State.Action == BoxAction.Resizing && State.Handle == ResizeHandle.Center && !Document.Selection.IsEmpty())
            {
                var sel = Document.Selection.GetSelectedParents().ToList();
                if (sel.Count == 1 && sel[0] is Entity && !sel[0].HasChildren)
                {
                    return viewport.Flatten(((Entity)sel[0]).Origin);
                }
            }
            return base.GetResizeOrigin(viewport);
        }

        protected override void MouseDraggingToResize(Viewport2D viewport, ViewportEvent e)
        {
            if (_currentTool == null)
            {
                base.MouseDraggingToResize(viewport, e);
                return;
            }

            State.Action = BoxAction.Resizing;
            CurrentTransform = GetTransformMatrix(viewport, e);
            if (CurrentTransform.HasValue)
            {
                Document.SetSelectListTransform(CurrentTransform.Value);
                var box = new Box(State.PreTransformBoxStart, State.PreTransformBoxEnd);
                var trans = CreateMatrixMultTransformation(CurrentTransform.Value);
                Mediator.Publish(EditorMediator.SelectionBoxChanged, box.Transform(trans));
            }
            else
            {
                OnBoxChanged();
            }
        }

        public override void KeyDown(ViewportBase viewport, ViewportEvent e)
        {
            var nudge = GetNudgeValue(e.KeyCode);
            var vp = viewport as Viewport2D;
            if (nudge != null && vp != null && (State.Action == BoxAction.ReadyToResize || State.Action == BoxAction.Drawn) && !Document.Selection.IsEmpty())
            {
                var translate = vp.Expand(nudge);
                var transformation = Matrix4.CreateTranslation((float)translate.X, (float)translate.Y, (float)translate.Z);
                //ExecuteTransform("Nudge", CreateMatrixMultTransformation(transformation), KeyboardState.Shift);
                SelectionChanged();
            }
            base.KeyDown(viewport, e);
        }

        #endregion 2Dinteraction

        #region Sidebar
        public override Image GetIcon()
        {
            return Resources.Menu_ObjectProperties;
        }

        public override string GetName()
        {
            return "Motions Tool";
        }
        #endregion sidebar

        #region Misc
        public override HotkeyTool? GetHotkeyToolType()
        {
            return HotkeyTool.Motions;
        }
        public override string GetContextualHelp()
        {
            return "";
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
        #endregion Misc

    }
}
