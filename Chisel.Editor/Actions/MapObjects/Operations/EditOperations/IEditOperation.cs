using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;

namespace Chisel.Editor.Actions.MapObjects.Operations.EditOperations
{
    public interface IEditOperation
    {
        void PerformOperation(MapObject mo);
    }
}
