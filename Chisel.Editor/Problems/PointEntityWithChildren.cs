using System.Collections.Generic;
using System.Linq;
using Chisel.DataStructures.GameData;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Actions;
using Chisel.Editor.Actions.MapObjects.Operations;

namespace Chisel.Editor.Problems
{
    public class PointEntityWithChildren : IProblemCheck
    {
        public IEnumerable<Problem> Check(Map map, bool visibleOnly)
        {
            foreach (var entity in map.WorldSpawn
                .Find(x => x is Entity && (!visibleOnly || (!x.IsVisgroupHidden && !x.IsCodeHidden)))
                .OfType<Entity>()
                .Where(x => x.GameData != null)
                .Where(x => x.GameData.ClassType != ClassType.Solid && x.GetChildren().Any()))
            {
                yield return new Problem(GetType(), map, new[] { entity }, Fix, "Point entity has children", "A point entity with children was found. A point entity cannot have any contents. Fixing the issue will move the children outside of the entity's group.");
            }
        }

        public IAction Fix(Problem problem)
        {
            return new Reparent(problem.Objects[0].Parent.ID, problem.Objects[0].GetChildren());
        }
    }
}