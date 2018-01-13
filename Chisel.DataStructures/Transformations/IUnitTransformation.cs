using System.Runtime.Serialization;
using Chisel.DataStructures.Geometric;

namespace Chisel.DataStructures.Transformations
{
    public interface IUnitTransformation : ISerializable
    {
        Coordinate Transform(Coordinate c);
        Matrix GetMatrix();
    }
}
