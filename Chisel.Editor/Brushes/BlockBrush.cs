using System.Collections.Generic;
using System.Linq;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.MapObjects;
using Chisel.Common;
using Chisel.Editor.Brushes.Controls;

namespace Chisel.Editor.Brushes
{
    public class BlockBrush : IBrush
    {
        public string Name
        {
            get { return "Block"; }
        }

        public bool CanRound { get { return true; } }

        public IEnumerable<BrushControl> GetControls()
        {
            return new List<BrushControl>();
        }

        public IEnumerable<MapObject> Create(IDGenerator generator, Box box, ITexture texture, int roundDecimals)
        {
            var solid = new Solid(generator.GetNextObjectID()) { Colour = Colour.GetRandomBrushColour(), Flags = SolidFlags.solid };
            foreach (var arr in box.GetBoxFaces())
            {
                var face = new Face(generator.GetNextFaceID())
                {
                    Parent = solid,
                    Plane = new Plane(arr[0], arr[1], arr[2]),
                    Colour = solid.Colour,
                    Texture = { Texture = texture },
                    //Flags = FaceFlags.Visible
                };
                face.Vertices.AddRange(arr.Select(x => new Vertex(x.Round(roundDecimals), face)));
                face.Init();
                solid.Faces.Add(face);
            }
            solid.UpdateBoundingBox();
            yield return solid;
        }
    }
}
