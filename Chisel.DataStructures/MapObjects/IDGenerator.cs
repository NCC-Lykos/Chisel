using System;
using System.Runtime.Serialization;

namespace Chisel.DataStructures.MapObjects
{
    [Serializable]
    public class IDGenerator : ISerializable
    {
        private long _lastObjectId;
        private long _lastFaceId;
        private long _lastMotionId;

        public IDGenerator()
        {
            _lastFaceId = 0;
            _lastObjectId = 0;
            _lastMotionId = 0;
        }

        protected IDGenerator(SerializationInfo info, StreamingContext context)
        {
            _lastObjectId = info.GetInt32("LastObjectID");
            _lastFaceId = info.GetInt32("LastFaceID");
            _lastMotionId = info.GetInt32("LastMotionID");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LastObjectID", _lastObjectId);
            info.AddValue("LastFaceID", _lastFaceId);
            info.AddValue("LastMotionID", _lastMotionId);
        }

        public long GetNextObjectID()
        {
            _lastObjectId++;
            return _lastObjectId;
        }

        public long GetNextFaceID()
        {
            _lastFaceId++;
            return _lastFaceId;
        }

        public long GetNextMotionID()
        {
            _lastMotionId++;
            return _lastMotionId;
        }

        public void Reset()
        {
            Reset(0, 0, 0);
        }

        public void Reset(long maxObjectId, long maxFaceId, long maxMotionID)
        {
            _lastFaceId = maxFaceId;
            _lastObjectId = maxObjectId;
            _lastMotionId = maxMotionID;
        }
    }
}