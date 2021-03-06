﻿using System.Collections.Generic;
using System.Linq;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.MapObjects;
using Chisel.Common;
using Chisel.Editor.Brushes.Controls;
using Chisel.Extensions;

namespace Chisel.Editor.Brushes
{
    public class CylinderBrush : IBrush
    {
        private readonly NumericControl _numSides;

        public CylinderBrush()
        {
            _numSides = new NumericControl(this) { LabelText = "Number of sides" };
        }

        public string Name
        {
            get { return "Cylinder"; }
        }

        public bool CanRound { get { return true; } }

        public IEnumerable<BrushControl> GetControls()
        {
            yield return _numSides;
        }

        public IEnumerable<MapObject> Create(IDGenerator generator, Box box, ITexture texture, int roundDecimals)
        {
            var numSides = (int) _numSides.GetValue();
            if (numSides < 3) yield break;

            // Cylinders can be elliptical so use both major and minor rather than just the radius
            // NOTE: when a low number (< 10ish) of faces are selected this will cause the cylinder to not touch all the edges of the box.
            var width = box.Width;
            var length = box.Length;
            var height = box.Height;
            var major = width / 2;
            var minor = length / 2;
            var angle = 2 * DMath.PI / numSides;

            // Calculate the X and Y points for the ellipse
            var points = new Coordinate[numSides];
            for (var i = 0; i < numSides; i++)
            {
                var a = i * angle;
                var xval = box.Center.X + major * DMath.Cos(a);
                var yval = box.Center.Y + minor * DMath.Sin(a);
                var zval = box.Start.Z;
                points[i] = new Coordinate(xval, yval, zval).Round(roundDecimals);
            }

            var faces = new List<Coordinate[]>();

            // Add the vertical faces
            var z = new Coordinate(0, 0, height).Round(roundDecimals);
            for (var i = 0; i < numSides; i++)
            {
                var next = (i + 1) % numSides;
                faces.Add(new[] {points[i], points[i] + z, points[next] + z, points[next]});
            }
            // Add the elliptical top and bottom faces
            faces.Add(points.ToArray());
            faces.Add(points.Select(x => x + z).Reverse().ToArray());

            // Nothing new here, move along
            var solid = new Solid(generator.GetNextObjectID()) { Colour = Colour.GetRandomBrushColour(), Flags = (System.UInt32)SolidFlags.solid };
            foreach (var arr in faces)
            {
                var face = new Face(generator.GetNextFaceID())
                {
                    Parent = solid,
                    Plane = new Plane(arr[0], arr[1], arr[2]),
                    Colour = solid.Colour,
                    Texture = { Texture = texture }
                };
                face.Vertices.AddRange(arr.Select(x => new Vertex(x, face)));
                face.Init();
                solid.Faces.Add(face);
            }
            solid.UpdateBoundingBox();
            yield return solid;
        }
    }
}
