using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.MapObjects;
using Chisel.Common;
using Chisel.Editor.Brushes.Controls;

namespace Chisel.Editor.Brushes
{
    public interface IBrush
    {
        string Name { get; }
        bool CanRound { get; }
        IEnumerable<BrushControl> GetControls();
        IEnumerable<MapObject> Create(IDGenerator generator, Box box, ITexture texture, int roundDecimals);
    }
}
