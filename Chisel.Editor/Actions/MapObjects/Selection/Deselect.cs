using System.Collections.Generic;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;

namespace Chisel.Editor.Actions.MapObjects.Selection
{
    public class Deselect : ChangeSelection
    {
        public Deselect(IEnumerable<MapObject> objects) : base(new MapObject[0], objects)
        {
        }
    }
}