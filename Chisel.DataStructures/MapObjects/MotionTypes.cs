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
        Quaternion q { get; set; }
        /*Translation*/
        Coordinate c { get; set; }

        public MotionKeyFrames(float keytime, Motion parent)
        {
            Parent = parent;
            KeyTime = keytime;
        }

        public MotionKeyFrames(Quaternion rotation,Coordinate translation, Motion parent)
        {
            Parent = parent;
            q = rotation;
            c = translation;
        }

        public Quaternion GetRotation()
        {
            return q;
        }
        public Coordinate GetTranslation()
        {
            return c;
        }

        public void SetRotation(Quaternion rot)
        {
            q = rot;
        }
        public void SetTranslation(Coordinate tra)
        {
            c = tra;
        }

        protected MotionKeyFrames(SerializationInfo info, StreamingContext context)
        {
            q = new Quaternion(info.GetDecimal("qX"), 
                               info.GetDecimal("qY"),
                               info.GetDecimal("qZ"), 
                               info.GetDecimal("qW"));
            
            c.X = info.GetDecimal("cX");
            c.Y = info.GetDecimal("cY");
            c.Y = info.GetDecimal("cZ");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("qX", q.X);
            info.AddValue("qY", q.Y);
            info.AddValue("qZ", q.Z);
            info.AddValue("qW", q.W);

            info.AddValue("qX", q.X);
            info.AddValue("qY", q.Y);
            info.AddValue("qZ", q.Z);
        }
    }
    
}
