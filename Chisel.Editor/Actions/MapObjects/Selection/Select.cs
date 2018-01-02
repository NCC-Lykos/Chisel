using System.Collections.Generic;
using System.Linq;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;

namespace Chisel.Editor.Actions.MapObjects.Selection
{
    public class Select : ChangeSelection
    {
        public Select(IEnumerable<MapObject> objects) : base(objects, new MapObject[0])
        {
        }

        public Select(params MapObject[] objects) : base(objects, new MapObject[0])
        {
        }
    }
}