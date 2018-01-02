using Chisel.DataStructures.Geometric;

namespace Chisel.DataStructures.Models
{
    public class BoneAnimationFrame
    {
        public Bone Bone { get; private set; }
        public CoordinateF Position { get; private set; }
        public QuaternionF Angles { get; private set; }

        public BoneAnimationFrame(Bone bone, CoordinateF position, QuaternionF angles)
        {
            Bone = bone;
            Position = position;
            Angles = angles;
        }
    }
}