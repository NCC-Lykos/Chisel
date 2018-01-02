using System;
using System.Collections.Generic;
using System.Linq;
using Chisel.Common;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Actions;
using Chisel.Editor.Actions.MapObjects.Operations;

namespace Chisel.Editor.Problems
{
    public class NoPlayerStart : IProblemCheck
    {
        public IEnumerable<Problem> Check(Map map, bool visibleOnly)
        {
            if (map.WorldSpawn.Find(x => x is Entity && x.GetEntityData() != null && string.Equals(x.GetEntityData().Name, "info_player_start", StringComparison.InvariantCultureIgnoreCase)).Any()) yield break;
            yield return new Problem(GetType(), map, Fix, "This map has no player start", "There is no info_player_start entity in this map. The player will spawn at the origin instead. This may place the player inside geometry or in the void. Fixing the issue will place a player start entity at the map origin. It is recommended that you fix this problem manually.");
        }

        public IAction Fix(Problem problem)
        {
            return new Create(problem.Map.WorldSpawn.ID, new Entity(problem.Map.IDGenerator.GetNextObjectID())
                                  {
                                      EntityData = new EntityData { Name = "info_player_start" },
                                      ClassName = "info_player_start",
                                      Colour = Colour.GetDefaultEntityColour(),
                                      Origin = Coordinate.Zero
                                  });
        }
    }
}