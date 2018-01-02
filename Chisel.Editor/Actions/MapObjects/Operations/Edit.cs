﻿using System;
using System.Collections.Generic;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Actions.MapObjects.Operations.EditOperations;
using Chisel.Editor.Documents;

namespace Chisel.Editor.Actions.MapObjects.Operations
{
    /// <summary>
    /// Perform: Changes the given objects (by ID) to the "after" state.
    /// Reverse: Changes the given objects (by ID) to the "before" state.
    /// </summary>
    public class Edit : CreateEditDelete
    {
        public Edit(IEnumerable<MapObject> before, IEnumerable<MapObject> after)
        {
            Edit(before, after);
        }

        public Edit(IEnumerable<MapObject> objects, IEditOperation editOperation)
        {
            Edit(objects, editOperation);
        }
    }
}
