using System.Collections.Generic;
using Chisel.DataStructures.Geometric;

namespace Chisel.DataStructures.MapObjects
{
    public class Motion
    {
        public const int ItemsNeeded = 6;
        public const int NameChecksum = 2379; //ignored in RF


        public string Name { get; set; }
        public int ID { get; set; }
        public double CurrentKeyTime { get; set; }  //Default is max key time.
        public Matrix Transform { get; set; } //Transform from Origin, center of solids

        public List<MotionKeyFrames> KeyFrames { get; set; }

        public List<string> RawModelLines = new List<string>();

        public Motion(int id)
        {
            ID = id;
            KeyFrames = new List<MotionKeyFrames>();
        }

        public void SetOrigin(Coordinate c)
        {
            Transform.Values[3] = c.X;
            Transform.Values[7] = c.Y;
            Transform.Values[11] = c.Z;
        }

        public void SetOrigin(decimal x, decimal y, decimal z)
        {
            Transform.Values[3] = x;
            Transform.Values[7] = y;
            Transform.Values[11] = z;
        }

        public Coordinate GetOrigin()
        {
            return new Coordinate(Transform.Values[3], Transform.Values[7], Transform.Values[11]);
        }
    }
}
