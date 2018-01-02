using System.Collections.Generic;
using System.Linq;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;

namespace Chisel.Editor.Actions.MapObjects.Selection
{
    public class DeselectFace : ChangeFaceSelection
    {
        public DeselectFace(IEnumerable<Face> objects) : base(new Face[0], objects)
        {
        }

        public DeselectFace(params Face[] objects) : base(new Face[0], objects)
        {
        }
    }
}