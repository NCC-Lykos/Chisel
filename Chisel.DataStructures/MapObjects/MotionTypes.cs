using System;
using System.Runtime.Serialization;
using Chisel.DataStructures.Geometric;

namespace Chisel.DataStructures.MapObjects
{
    [Serializable]
    public class MotionKeyFrames : ISerializable
    {
        public Motion Parent { get; set; }

        public float KeyTime { get; set; }
        /*Rotation*/
        public float rotX { get; set; }
        public float rotY { get; set; }
        public float rotZ { get; set; }
        public float rotW { get; set; }
        /*Translation*/
        public float traX { get; set; }
        public float traY { get; set; }
        public float traZ { get; set; }

        public MotionKeyFrames(float keytime, Motion parent)
        {
            Parent = parent;
            KeyTime = keytime;
        }

        public MotionKeyFrames(Coordinate rotation,Coordinate translation, Motion parent)
        {
            Parent = parent;

            rotX = (float)rotation.X; rotY = (float)rotation.Y; rotZ = (float)rotation.Z;
            traX = (float)translation.X; traY = (float)translation.Y; traZ = (float)translation.Z;
        }

        public void SetRotation(Quaternion q)
        {
            rotX = (float)q.X;
            rotY = (float)q.Y;
            rotZ = (float)q.Z;
            rotW = (float)q.W;
        }

        public void SetTranslation(Coordinate c)
        {
            traX = (float)c.X;
            traY = (float)c.Y;
            traZ = (float)c.Z;
        }

        protected MotionKeyFrames(SerializationInfo info, StreamingContext context)
        {
            rotX = (float)info.GetDecimal("rotX");
            rotY = (float)info.GetDecimal("rotY");
            rotZ = (float)info.GetDecimal("rotZ");

            traX = (float)info.GetDecimal("traX");
            rotY = (float)info.GetDecimal("rotY");
            rotZ = (float)info.GetDecimal("rotZ");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("rotX", rotX);
            info.AddValue("rotY", rotX);
            info.AddValue("rotZ", rotX);

            info.AddValue("rotX", rotX);
            info.AddValue("rotY", rotX);
            info.AddValue("rotZ", rotX);
        }
    }
    
}
