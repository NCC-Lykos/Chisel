using System;
using System.Collections.Generic;
using System.Linq;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;
using Chisel.Editor.Tools;
using Chisel.Settings;

namespace Chisel.Editor.Actions.MapObjects.Selection
{
    public class ChangeToFaceSelectionMode : IAction
    {
        public bool SkipInStack { get { return Chisel.Settings.Select.SkipSelectionInUndoStack; } }
        public bool ModifiesState { get { return false; } }

        private readonly Type _toolType;
        private readonly List<long> _selection;

        public ChangeToFaceSelectionMode(Type toolType, IEnumerable<MapObject> selection)
        {
            _toolType = toolType;
            _selection = selection.Select(x => x.ID).ToList();
        }

        public void Dispose()
        {
            _selection.Clear();
        }

        public void Reverse(Document document)
        {
            ToolManager.Deactivate(true);

            document.Selection.SwitchToObjectSelection();
            document.Selection.Clear();

            var sel = _selection.Select(x => document.Map.WorldSpawn.FindByID(x)).Where(x => x != null && x.BoundingBox != null).ToList();
            document.Selection.Select(sel);

            ToolManager.Activate(HotkeyTool.Selection, true);

            Mediator.Publish(EditorMediator.DocumentTreeSelectedObjectsChanged, sel.Union(document.Selection.GetSelectedObjects()));
            Mediator.Publish(EditorMediator.SelectionChanged);
        }

        public void Perform(Document document)
        {
            ToolManager.Deactivate(true);

            document.Selection.SwitchToFaceSelection();

            ToolManager.Activate(_toolType, true);

            Mediator.Publish(EditorMediator.DocumentTreeSelectedFacesChanged, document.Selection.GetSelectedFaces());
            Mediator.Publish(EditorMediator.SelectionChanged);
        }
    }
}