using System;

namespace Chisel.DataStructures.MapObjects
{
    [Flags]
    public enum TransformFlags
    {
        None = 1 << 0,
        TextureLock = 1 << 1,
        TextureScalingLock = 1 << 2,
        RotationX = 1 << 3,
        RotationY = 1 << 4,
        RotationZ = 1 << 5,
        Translate = 1 << 6,
        Scale = 1 << 7,
        Move = 2 << 8
    }
}