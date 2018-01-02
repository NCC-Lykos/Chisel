using System.Collections.Generic;
using System.Linq;
using Chisel.Common.Mediator;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;

namespace Chisel.Editor.Actions.MapObjects.Selection
{
    public class SelectFace : ChangeFaceSelection
    {
        public SelectFace(IEnumerable<Face> objects) : base(objects, new Face[0])
        {
        }

        public SelectFace(params Face[] objects) : base(objects, new Face[0])
        {
        }
    }
}