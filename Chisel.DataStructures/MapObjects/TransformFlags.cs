using System;

namespace Chisel.DataStructures.MapObjects
{
    [Flags]
    public enum TransformFlags
    {
        None = 1 << 0,
        TextureLock = 1 << 1,
        TextureScalingLock = 1 << 2,
        Rotation = 1 << 3,
        Translate = 1 << 4, //Move
        Scale = 1 << 5,
        Skew = 1 << 6
    }
}