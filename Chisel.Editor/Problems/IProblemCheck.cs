using System.Collections.Generic;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Actions;

namespace Chisel.Editor.Problems
{
    public interface IProblemCheck
    {
        IEnumerable<Problem> Check(Map map, bool visibleOnly);
        IAction Fix(Problem problem);
    }
}
